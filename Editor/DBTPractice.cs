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
    private static AnimatorState animatorState;
    private static AnimatorStateMachine animatorStateMachine;

    [MenuItem("DBT/make")]
    static void createDBT()
    {
        // Make blendtree
        BlendTree = new BlendTree()
        {
            blendType = BlendTreeType.Direct,
            name = "DBT(WD On)",
        };
        BlendTree2 = new BlendTree()
        {
            blendType = BlendTreeType.Simple1D,
            blendParameter = "IIValue",
            name = $@"{PROJECTNAME}2",
            useAutomaticThresholds=false,
        };
        BlendTree.AddDirectChild(BlendTree2, ONE);
        for(int n = 0; n < 1000; n++)
        {
            int leftnum = n;
            int Digit1 = leftnum % 10;
            leftnum = (leftnum - Digit1) / 10;
            int Digit2 = leftnum % 10;
            leftnum = (leftnum - Digit2) / 10;
            int Digit3 = leftnum % 10;
            BlendTree2.AddChild(LoadMotion($@"Assets/DBTPractice/Res/Digit1/{Digit1}.anim"), (float)n);
            BlendTree2.AddChild(LoadMotion($@"Assets/DBTPractice/Res/Digit2/{Digit2}.anim"), (float)n);
            BlendTree2.AddChild(LoadMotion($@"Assets/DBTPractice/Res/Digit3/{Digit3}.anim"), (float)n);
        }

        createAnimatorController();
        PAssetsSave.run(PROJECTNAME, BlendTree2, BlendTree, animatorState, animatorStateMachine, animatorController); // [Warning!!] Save the smallest element first. If not, it will break when restart.
    }

    public static void createAnimatorController()
    {
        animatorState = new AnimatorState()
        {
            writeDefaultValues = true,
            motion = BlendTree,
            name = "DBT",
        };
        animatorStateMachine = new AnimatorStateMachine()
        {
            name = PROJECTNAME,
            states = new[]
            {
                new ChildAnimatorState
                {
                    state = animatorState,
                    position = Vector3.zero,
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
                    name = $@"{PROJECTNAME}Layer",
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
        animatorController.AddParameter(
            new AnimatorControllerParameter()
            {
                name = "IIValue",
                type = AnimatorControllerParameterType.Float,
                defaultFloat = 0
            }
        );
    }
    public static AnimationClip LoadMotion(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Animation clip path is empty.");
            return null;
        }
        if (!File.Exists(path))
        {
            Debug.LogError("Animation clip does not exist: " + path);
            return null;
        }
        if (!path.EndsWith(".anim"))
        {
            Debug.LogError("Animation clip file format is invalid: " + path);
            return null;
        }
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            Debug.LogError("Failed to load animation clip: " + path);
            return null;
        }
        return clip;
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
        //clearTempDirectory();
        createTempDirectory();
        saveAssets();
    }
    private static string tempDirectory()
    {
        string selectedFolderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        return $@"{Path.GetDirectoryName(selectedFolderPath)}/Gen/";
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
        createTempDirectory();
    }
    private static void createTempDirectory()
    {
        if (!Directory.Exists(tempDirectory()))
        {
            Directory.CreateDirectory(tempDirectory());
            AssetDatabase.Refresh();
        }
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
            Debug.Log($@"Asset Saved at: {savePath()}");
        }
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }
    private static string extension()
    {
        Type astType = workingAsset.GetType();
        if (extensionMap.ContainsKey(astType)) return extensionMap[astType];
        else return "asset";
    }
}