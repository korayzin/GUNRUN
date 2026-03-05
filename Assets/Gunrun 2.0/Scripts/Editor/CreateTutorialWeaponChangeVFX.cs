#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Silah değişim VFX prefab'ı oluşturur. WeaponManager vfxSecond...vfxNinth slot'larına atanabilir.
/// Menü: Tools > Gunrun > Create Tutorial Weapon Change VFX
/// </summary>
public static class CreateTutorialWeaponChangeVFX
{
    [MenuItem("Tools/Gunrun/Create Tutorial Weapon Change VFX")]
    public static void Create()
    {
        var go = new GameObject("TutorialWeaponChangeVFX");
        go.SetActive(false);

        var ps = go.AddComponent<ParticleSystem>();
        var renderer = go.GetComponent<ParticleSystemRenderer>();

        var main = ps.main;
        main.duration = 1.5f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = 1f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.95f, 0.75f, 0.85f),
            new Color(1f, 0.7f, 0.4f, 0.75f));
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 120;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 55, 75),
            new ParticleSystem.Burst(0.12f, 20, 28),
            new ParticleSystem.Burst(0.25f, 10, 15)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.03f;
        shape.radiusThickness = 0.6f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0f), new GradientColorKey(new Color(1f, 0.7f, 0.25f), 0.6f), new GradientColorKey(new Color(0.9f, 0.5f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0.6f, 0.4f), new GradientAlphaKey(0.35f, 0.7f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.15f));

        var vol = ps.velocityOverLifetime;
        vol.enabled = true;
        vol.y = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
        vol.space = ParticleSystemSimulationSpace.Local;

        var matPath = "Assets/Gunrun 2.0/VFX 2.0/TutorialWeaponChangeVFX_Material.mat";
        var mat = GetOrCreateParticleMaterial(matPath);
        if (mat != null)
        {
            renderer.sharedMaterial = mat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.minParticleSize = 0.001f;
        }

        var path = "Assets/Gunrun 2.0/VFX 2.0/TutorialWeaponChangeVFX.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        if (prefab != null)
        {
            AssetDatabase.Refresh();
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            Debug.Log($"Tutorial weapon change VFX prefab oluşturuldu: {path}. WeaponManager vfxSecond-vfxNinth slot'larına atayabilirsiniz.");
        }
        else
        {
            Debug.LogError($"Prefab oluşturulamadı: {path}");
        }
    }

    static Material GetOrCreateParticleMaterial(string assetPath)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (existing != null) return existing;

        var shader = Shader.Find("Particles/Standard Unlit")
            ?? Shader.Find("Universal Render Pipeline/Particles/Unlit")
            ?? Shader.Find("Particles/Default")
            ?? Shader.Find("Unlit/Color");
        if (shader == null) return null;

        var mat = new Material(shader);
        mat.SetColor("_Color", Color.white);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);

        AssetDatabase.CreateAsset(mat, assetPath);
        return mat;
    }
}
#endif
