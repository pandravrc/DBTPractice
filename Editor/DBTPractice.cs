using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.IO;
using System.Collections.Generic;

public class DBTPractice : MonoBehaviour
{
    private const string PROJECTNAME = "DBTPractice";
    private const string ONE = "ONEf";
    private static AnimatorController animatorController;
    private static BlendTree BlendTree, BlendTree2;

    [MenuItem("DBT/make")]
    static void createDBT()
    {
        // Make blendtree
        BlendTree = new BlendTree()
        {
            blendType = BlendTreeType.Direct,
            name = PROJECTNAME
        };
        BlendTree2 = new BlendTree()
        {
            blendType = BlendTreeType.Simple1D,
            blendParameter = ONE,
            name = $@"{PROJECTNAME}2"
        };
        BlendTree.AddDirectChild(BlendTree2, ONE);

        createAnimatorController();
        PAssetsSave.run(PROJECTNAME, animatorController, BlendTree, BlendTree2);
    }

    public static void createAnimatorController()
    {
        var animatorState = new AnimatorState()
        {
            writeDefaultValues = true,
            motion = BlendTree,
            name = "DBT",
        };
        var animatorStateMachine = new AnimatorStateMachine()
        {
            name = PROJECTNAME,
            states = new[]
            {
                new ChildAnimatorState
                {
                    state = animatorState,
                }
            },
            defaultState = animatorState,
        };
        animatorController = new AnimatorController()
        {
            name = PROJECTNAME,
            layers = new[]
            {
                new AnimatorControllerLayer
                {
                    blendingMode = AnimatorLayerBlendingMode.Override,
                    defaultWeight = 1,
                    name = PROJECTNAME,
                    stateMachine = animatorStateMachine
                }
            },
        };
        animatorController.AddParameter(
            new AnimatorControllerParameter()
            {
                name = ONE,
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 1
            }
        );
    }

}
public static class BlendTreeExtensions
{
    public static void AddDirectChild(this BlendTree blendTree, BlendTree childTree, string parameterName)
    {
        if (blendTree == null)
        {
            throw new ArgumentNullException("blendTree");
        }
        if (childTree == null)
        {
            throw new ArgumentNullException("childTree");
        }
        if (string.IsNullOrEmpty(parameterName))
        {
            throw new ArgumentNullException("parameterName");
        }
        blendTree.AddChild(childTree);
        var c = blendTree.children;
        c[c.Length - 1].directBlendParameter = parameterName;
        blendTree.children = c;
    }
}
public static class PAssetsSave 
{
    /// <summary>Pan Assets Save [[WARNING: This code will delete all files in the Gen folder without prior notice.]]</summary>
    private static string projectName;
    private static UnityEngine.Object[] assets;
    private static UnityEngine.Object workingAsset;
    private static Dictionary<Type, string> extensionMap = new Dictionary<Type, string>
    {
        { typeof(BlendTree), "asset" },
        { typeof(AnimatorController), "controller" },
    };
    public static void run(string _projectName, params UnityEngine.Object[] _assets)
    {
        Debug.Log("This code is for development use only and is destructive. DO NOT USE in production.");
        projectName = _projectName;
        assets = _assets;
        clearTempDirectory();
        saveAssets();
    }
    private static string tempDirectory()
    {
        return $@"Assets/{projectName}/Temp/";
    }
    private static string fileName()
    {
        return $@"{workingAsset.name}.{extension()}";
    }
    private static string savePath()
    {
        return $@"{tempDirectory()}{fileName()}";
    }
    private static void clearTempDirectory()
    {
        if (Directory.Exists(tempDirectory()))
        {
            Directory.Delete(tempDirectory(), true);
        }
        Directory.CreateDirectory(tempDirectory());
        AssetDatabase.Refresh();
    }
    private static void saveAssets()
    {
        foreach (var ast in assets)
        {
            workingAsset = ast;
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(savePath()) != null)
            {
                Debug.LogWarning($@"Collision occurred in {savePath()}");
            }
            AssetDatabase.CreateAsset(workingAsset, savePath());
            AssetDatabase.SaveAssets();
            Debug.Log($@"Asset Saved at: {savePath()}");
        }
    }
    private static string extension()
    {
        Type astType = workingAsset.GetType();
        if (extensionMap.ContainsKey(astType)) return extensionMap[astType];
        else return "asset";
    }
}