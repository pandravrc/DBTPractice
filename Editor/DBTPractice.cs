using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.VersionControl;
using System.Collections.Generic;

using System;

public class BlendTreeCreator : MonoBehaviour
{
    private const string PROJECTNAME = "DBTPractice";
    private const string WEIGHT = "Weight";
    private static string saveDirectory = $@"Assets/{PROJECTNAME}";

    [MenuItem("DBT/make")]
    static void CreateBlendTreeAsset()
    {
        // Variable definition
        var newController = new AnimatorController();
        var newStateMachine = new AnimatorStateMachine();
        var newState = new AnimatorState();
        BlendTree BlendTree, BlendTree2;
        AnimatorControllerParameter weightParameter;

        // Setting Controller
        newController.name = PROJECTNAME;

        // Make Layer
        newController.layers = new[]
        {
            new AnimatorControllerLayer
            {
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1,
                name = PROJECTNAME,
                stateMachine = newStateMachine
            }
        };

        // Make Parameter
        weightParameter = new AnimatorControllerParameter()
        {
            name = WEIGHT,
            type = AnimatorControllerParameterType.Float,
            defaultFloat = 1
        };
        newController.AddParameter(weightParameter);

        // Make Parent blendtree
        BlendTree = new BlendTree()
        {
            blendType = BlendTreeType.Direct,
            blendParameter = WEIGHT,
            name = PROJECTNAME
        };

        // Set BlendTree -> StateMachine
        newStateMachine.name = PROJECTNAME;
        newStateMachine.states = new[]
        {
            new ChildAnimatorState
            {
                state = newState,
                position = Vector3.zero
            }

        };
        newStateMachine.defaultState = newState;
        newState.writeDefaultValues = true;
        newState.motion = BlendTree;


        // Make BlendTree2
        BlendTree2 = new BlendTree()
        {
            blendType = BlendTreeType.Direct,
            blendParameter = WEIGHT,
            name = $@"{PROJECTNAME}2"
        };

        // Make Parent
        BlendTree.AddChild(BlendTree2);
        var children = BlendTree.children;
        children[children.Length - 1].directBlendParameter = WEIGHT;
        BlendTree.children = children;

        // Save to File
        SaveAssetToProjectFolder(newController);
        SaveAssetToProjectFolder(BlendTree);
        SaveAssetToProjectFolder(BlendTree2);
    }


    private static void SaveAssetToProjectFolder(UnityEngine.Object AST)
    {
        if (!AssetDatabase.IsValidFolder(saveDirectory))
        {
            AssetDatabase.CreateFolder("Assets", PROJECTNAME);
        }
        string savePath=saveDirectory + $@"/{AST.name}.{extension(AST)}";
        AssetDatabase.CreateAsset(AST, savePath);
        AssetDatabase.SaveAssets();
        Debug.Log($@"Asset Saved at: {savePath}");
    }

    private static string extension(UnityEngine.Object AST)
    {
        Dictionary<Type, string> extensionMap = new Dictionary<Type, string>
        {
            { typeof(BlendTree), "asset" },
            { typeof(AnimatorController), "controller" },
        };
        Type astType = AST.GetType();
        if (extensionMap.ContainsKey(astType))
        {
            return extensionMap[astType];
        }
        else
        {
            return "asset";
        }
    }
}
