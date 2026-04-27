#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class LRFBlendTreeSetup : EditorWindow
{
    [MenuItem("Tools/LRF/Setup BlendTrees")]
    static void Setup()
    {
        string controllerPath = "Assets/LRF/LRF_AnimatorController.controller";
        string clipsPath      = "Assets/LRF/Animations";

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            Debug.LogError("Animator Controller not found: " + controllerPath);
            return;
        }

        SetupDistDrive(controller, clipsPath);
        SetupBeamScale(controller, clipsPath);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("LRF BlendTrees setup complete.");
    }

    static void SetupDistDrive(AnimatorController controller, string clipsPath)
    {
        var layer = GetOrCreateLayer(controller, "LRF_DistDrive");
        var sm    = layer.stateMachine;

        var state = sm.AddState("DistDrive");
        state.writeDefaultValues = false;

        var bt = new BlendTree
        {
            name                   = "LRF_DistBlendTree",
            blendType              = BlendTreeType.Simple1D,
            blendParameter         = "LRF_Distance",
            useAutomaticThresholds = false
        };

        for (int d = 0; d <= 999; d++)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                $"{clipsPath}/LRF_Dist_{d:D3}.anim");
            if (clip == null) { Debug.LogWarning($"Clip not found: LRF_Dist_{d:D3}.anim"); continue; }
            bt.AddChild(clip, (float)d);
        }

        AssetDatabase.AddObjectToAsset(bt, controller);
        state.motion    = bt;
        sm.defaultState = state;
    }

    static void SetupBeamScale(AnimatorController controller, string clipsPath)
    {
        var layer = GetOrCreateLayer(controller, "LRF_BeamScale");
        var sm    = layer.stateMachine;

        var state = sm.AddState("BeamScale");
        state.writeDefaultValues = false;

        var bt = new BlendTree
        {
            name                   = "LRF_BeamBlendTree",
            blendType              = BlendTreeType.Simple1D,
            blendParameter         = "LRF_Ratio",
            useAutomaticThresholds = false
        };

        var clipMin = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{clipsPath}/LRF_Beam_Min.anim");
        var clipMax = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{clipsPath}/LRF_Beam_Max.anim");
        if (clipMin != null) bt.AddChild(clipMin, 0f);
        if (clipMax != null) bt.AddChild(clipMax, 1f);

        AssetDatabase.AddObjectToAsset(bt, controller);
        state.motion    = bt;
        sm.defaultState = state;
    }

    static AnimatorControllerLayer GetOrCreateLayer(AnimatorController ac, string name)
    {
        foreach (var l in ac.layers)
            if (l.name == name) return l;

        ac.AddLayer(name);
        var layers   = ac.layers;
        var newLayer = layers[layers.Length - 1];
        newLayer.defaultWeight = 1f;
        ac.layers = layers;
        return ac.layers[ac.layers.Length - 1];
    }
}
#endif
