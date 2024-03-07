using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class BlendTreeCreator : MonoBehaviour
{
    private const string PROJECTNAME = "DBTPractice";
    private static string saveDirectory = $@"Assets/{PROJECTNAME}";

    [MenuItem("DBT/make")]
    static void CreateBlendTreeAsset()
    {
        // Create a new AnimatorController & Layer
        AnimatorController animatorController = new AnimatorController();
        animatorController.AddLayer(PROJECTNAME);
        AnimatorControllerLayer layer = animatorController.layers[0];
        layer.defaultWeight = 1.0f;
        
        // Create an AnimatorControllerParameter -> Add to Controller
        AnimatorControllerParameter weightParameter = new AnimatorControllerParameter();
        weightParameter.name = "Weight";
        weightParameter.type = AnimatorControllerParameterType.Float;
        weightParameter.defaultFloat = 1f;
        animatorController.AddParameter(weightParameter);

        // Create BlendTree
        BlendTree blendTree = new BlendTree();
        blendTree.blendType = BlendTreeType.Direct;
        
        // Set BlendTree -> Layer
        AnimatorStateMachine stateMachine = animatorController.layers[0].stateMachine;
        UnityEditor.Animations.AnimatorState state = stateMachine.AddState(PROJECTNAME);
        state.motion = blendTree;

        // Save to File
        if (!AssetDatabase.IsValidFolder(saveDirectory))
        {
            AssetDatabase.CreateFolder("Assets", PROJECTNAME);
        }
        string controllerPath = saveDirectory + $@"/{PROJECTNAME}.controller";
        AssetDatabase.CreateAsset(animatorController, controllerPath);
        AssetDatabase.SaveAssets();
        Debug.Log("AnimatorController asset created at: " + controllerPath);
        string blendTreePath = saveDirectory + $@"/{PROJECTNAME}.asset";
        AssetDatabase.CreateAsset(blendTree, blendTreePath);
        AssetDatabase.SaveAssets();
        Debug.Log("BlendTree asset created at: " + blendTreePath);
    }
}
