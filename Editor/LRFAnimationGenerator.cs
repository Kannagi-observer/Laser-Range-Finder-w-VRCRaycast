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
        GenerateBeamClips(savePath);
        GenerateActiveClips(savePath);

        AssetDatabase.Refresh();
        Debug.Log("LRF Animations generated.");
    }

    // LRF_Attachment に Animator を置く構成のため、
    // パスは LRF_Attachment からの相対パスで指定する

    static EditorCurveBinding MakeBinding(string path, string propertyName) => new EditorCurveBinding
    {
        path         = path,
        type         = typeof(MeshRenderer),
        propertyName = propertyName
    };

    static EditorCurveBinding MakeTransformBinding(string path, string propertyName) => new EditorCurveBinding
    {
        path         = path,
        type         = typeof(Transform),
        propertyName = propertyName
    };

    static EditorCurveBinding MakeGameObjectBinding(string path) => new EditorCurveBinding
    {
        path         = path,
        type         = typeof(GameObject),
        propertyName = "m_IsActive"
    };

    static AnimationCurve Constant(float value) => new AnimationCurve(
        new Keyframe(0f,      value, 0f, 0f),
        new Keyframe(1f/60f, value, 0f, 0f)
    );

    static void GenerateDistanceClips(string path)
    {
        var bindingDist = MakeBinding("LRF_Display", "material._Distance");
        var bindingHit  = MakeBinding("LRF_Display", "material._Hit");

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
        var bindingDist = MakeBinding("LRF_Display", "material._Distance");
        var bindingHit  = MakeBinding("LRF_Display", "material._Hit");

        foreach (var (name, hitVal) in new[]{ ("Hit_On", 1f), ("Hit_Off", 0f) })
        {
            AnimationClip clip = new AnimationClip { frameRate = 60 };
            AnimationUtility.SetEditorCurve(clip, bindingDist, Constant(0f));
            AnimationUtility.SetEditorCurve(clip, bindingHit,  Constant(hitVal));
            AssetDatabase.CreateAsset(clip, $"{path}/LRF_{name}.anim");
        }
    }

    // BeamAnchor の Scale.z を 0/1000 にするClip
    // LaserBeam は BeamAnchor の子で Position.z=0.5 にオフセット済みのため
    // BeamAnchor の Scale.z を伸ばすことで根本起点のビームになる
    static void GenerateBeamClips(string path)
    {
        var binding = MakeTransformBinding("LRF_Emitter/BeamAnchor", "m_LocalScale.z");

        foreach (var (name, scaleZ) in new[]{ ("Beam_Min", 0f), ("Beam_Max", 1000f) })
        {
            AnimationClip clip = new AnimationClip { frameRate = 60 };
            AnimationUtility.SetEditorCurve(clip, binding, Constant(scaleZ));
            AssetDatabase.CreateAsset(clip, $"{path}/LRF_{name}.anim");
        }
    }

    // LRF_Active ON/OFF: HitDot の表示制御
    static void GenerateActiveClips(string path)
    {
        var bindingHitDot = MakeGameObjectBinding("LRF_HitPoint/HitDot");

        foreach (var (name, active) in new[]{ ("Active_On", 1f), ("Active_Off", 0f) })
        {
            AnimationClip clip = new AnimationClip { frameRate = 60 };
            AnimationUtility.SetEditorCurve(clip, bindingHitDot, Constant(active));
            AssetDatabase.CreateAsset(clip, $"{path}/LRF_{name}.anim");
        }
    }
}
#endif
