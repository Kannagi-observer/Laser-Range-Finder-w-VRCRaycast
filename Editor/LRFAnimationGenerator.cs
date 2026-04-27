#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class LRFAnimationGenerator : EditorWindow
{
    [MenuItem("Tools/LRF/Generate Animations")]
    static void Generate()
    {
        string savePath = "Assets/LRF/Animations";
        System.IO.Directory.CreateDirectory(savePath);

        GenerateDistanceClips(savePath);
        GenerateHitClips(savePath);

        AssetDatabase.Refresh();
        Debug.Log("LRF Animations generated.");
    }

    static EditorCurveBinding MakeBinding(string propertyName) => new EditorCurveBinding
    {
        path         = "LRF_Attachment/LRF_Display",
        type         = typeof(MeshRenderer),
        propertyName = propertyName
    };

    static AnimationCurve Constant(float value) => new AnimationCurve(
        new Keyframe(0f,      value, 0f, 0f),
        new Keyframe(1f/60f, value, 0f, 0f)
    );

    static void GenerateDistanceClips(string path)
    {
        var bindingDist = MakeBinding("material._Distance");
        var bindingHit  = MakeBinding("material._Hit");

        for (int d = 0; d <= 999; d++)
        {
            AnimationClip clip = new AnimationClip { frameRate = 60 };
            AnimationUtility.SetEditorCurve(clip, bindingDist, Constant((float)d));
            AnimationUtility.SetEditorCurve(clip, bindingHit,  Constant(1f));
            AssetDatabase.CreateAsset(clip, $"{path}/LRF_Dist_{d:D3}.anim");
        }
    }

    static void GenerateHitClips(string path)
    {
        var bindingDist = MakeBinding("material._Distance");
        var bindingHit  = MakeBinding("material._Hit");

        foreach (var (name, hitVal) in new[]{ ("Hit_On", 1f), ("Hit_Off", 0f) })
        {
            AnimationClip clip = new AnimationClip { frameRate = 60 };
            AnimationUtility.SetEditorCurve(clip, bindingDist, Constant(0f));
            AnimationUtility.SetEditorCurve(clip, bindingHit,  Constant(hitVal));
            AssetDatabase.CreateAsset(clip, $"{path}/LRF_{name}.anim");
        }
    }
}
#endif
