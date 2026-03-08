using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Silah mesh'ine dissolve/build efekti uygular. Tutorial silah geçişlerinde:
/// - Despawn: Yukarıdan aşağıya yavaşça siler (wipe)
/// - Spawn: Kabza (aşağı) ile tepeden (yukarı) yavaşça oluşturur (build)
/// Prefab içindeki TÜM renderer'lara ve her renderer'ın TÜM materyallerine uygulanır.
/// Sadece newtutorial sahnesinde aktif.
/// </summary>
public class WeaponDissolveEffect : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Despawn animasyon süresi (saniye)")]
    public float despawnDuration = 0.4f;
    [Tooltip("Spawn/build animasyon süresi (saniye)")]
    public float spawnDuration = 0.5f;
    [Header("Dissolve Kenarı")]
    [Tooltip("Kenar ışıltı rengi")]
    public Color edgeGlowColor = new Color(1f, 0.9f, 0.6f, 0.8f);
    [Tooltip("Kenar genişliği (0-0.2)")]
    [Range(0.01f, 0.2f)]
    public float edgeGlowWidth = 0.05f;

    private static readonly int DissolveProgress = Shader.PropertyToID("_DissolveProgress");
    private static readonly int HeightMin = Shader.PropertyToID("_HeightMin");
    private static readonly int HeightMax = Shader.PropertyToID("_HeightMax");
    private static readonly int BuildMode = Shader.PropertyToID("_BuildMode");
    private static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
    private static readonly int EdgeWidth = Shader.PropertyToID("_EdgeWidth");

    private Renderer[] _renderers;
    private Material[][] _originalMaterialsArrays;
    private Material[][] _dissolveMaterialsArrays;
    private bool _initialized;
    private Shader _dissolveShader;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().name != "newtutorial") return;
        CacheRenderers();
    }

    private void CacheRenderers()
    {
        var allRenderers = GetComponentsInChildren<Renderer>(true);
        if (allRenderers == null || allRenderers.Length == 0) return;
        var rendererList = new System.Collections.Generic.List<Renderer>();
        var origList = new System.Collections.Generic.List<Material[]>();
        var dissList = new System.Collections.Generic.List<Material[]>();

        // Fallback materyali şablon olarak kullan - Shader.Find bazen null/pembe döner
        var fallbackMat = Resources.Load<Material>("WeaponDissolveFallback");
        if (fallbackMat == null || fallbackMat.shader == null)
        {
            _dissolveShader = Shader.Find("Custom/WeaponDissolve");
            if (_dissolveShader == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("[WeaponDissolveEffect] Custom/WeaponDissolve shader ve Resources/WeaponDissolveFallback.mat bulunamadı.");
#endif
                return;
            }
        }

        foreach (var r in allRenderers)
        {
            if (r == null || !(r is MeshRenderer || r is SkinnedMeshRenderer)) continue;
            var origMats = r.sharedMaterials;
            if (origMats == null || origMats.Length == 0) continue;

            var dissMats = new Material[origMats.Length];
            bool hasValid = false;
            for (int m = 0; m < origMats.Length; m++)
            {
                var om = origMats[m];
                if (om == null) continue;

                Material dm;
                if (fallbackMat != null && fallbackMat.shader != null)
                {
                    dm = new Material(fallbackMat); // Şablondan kopyala - pembe önlenir
                }
                else
                {
                    dm = new Material(_dissolveShader);
                }

                // Texture: URP _BaseMap veya _MainTex (pembe önlemek için her zaman texture ata)
                var tex = om.HasProperty("_BaseMap") ? om.GetTexture("_BaseMap") : (om.HasProperty("_MainTex") ? om.GetTexture("_MainTex") : om.mainTexture);
                if (tex == null) tex = Texture2D.whiteTexture;
                dm.SetTexture("_MainTex", tex);
                // UV scale/offset - URP _BaseMap veya _MainTex'ten kopyala (shader TRANSFORM_TEX için gerekli)
                if (om.HasProperty("_BaseMap"))
                {
                    dm.SetTextureScale("_MainTex", om.GetTextureScale("_BaseMap"));
                    dm.SetTextureOffset("_MainTex", om.GetTextureOffset("_BaseMap"));
                }
                else if (om.HasProperty("_MainTex"))
                {
                    dm.SetTextureScale("_MainTex", om.GetTextureScale("_MainTex"));
                    dm.SetTextureOffset("_MainTex", om.GetTextureOffset("_MainTex"));
                }
                else
                {
                    dm.SetTextureScale("_MainTex", new Vector2(1f, 1f));
                    dm.SetTextureOffset("_MainTex", Vector2.zero);
                }

                var col = Color.white;
                if (om.HasProperty("_BaseColor")) col = om.GetColor("_BaseColor");
                else if (om.HasProperty("_Color")) col = om.GetColor("_Color");
                dm.SetColor("_Color", col);

                dm.SetFloat(BuildMode, 0f);
                dm.SetColor(EdgeColor, edgeGlowColor);
                dm.SetFloat(EdgeWidth, edgeGlowWidth);

                dissMats[m] = dm;
                hasValid = true;
            }
            if (!hasValid) continue;

            rendererList.Add(r);
            origList.Add(origMats);
            dissList.Add(dissMats);
        }

        if (rendererList.Count == 0) return;
        _renderers = rendererList.ToArray();
        _originalMaterialsArrays = origList.ToArray();
        _dissolveMaterialsArrays = dissList.ToArray();
        _initialized = true;
    }

    private (float min, float max) GetHeightRangeForRenderer(Renderer r)
    {
        var b = r.localBounds;
        float minY = b.min.y - 0.01f;
        float maxY = b.max.y + 0.01f;
        return (minY, maxY);
    }

    /// <summary>Yukarıdan aşağıya siler (despawn). Silah dissolve bitince hemen gizlenir.</summary>
    public IEnumerator PlayDespawn(float? durationOverride = null)
    {
        if (!_initialized) CacheRenderers();
        if (!_initialized) yield break;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null || _dissolveMaterialsArrays[i] == null) continue;
            var (minY, maxY) = GetHeightRangeForRenderer(_renderers[i]);
            for (int m = 0; m < _dissolveMaterialsArrays[i].Length; m++)
            {
                var dm = _dissolveMaterialsArrays[i][m];
                if (dm == null) continue;
                dm.SetFloat(HeightMin, minY);
                dm.SetFloat(HeightMax, maxY);
                dm.SetFloat(BuildMode, 0f);
            }
            _renderers[i].sharedMaterials = _dissolveMaterialsArrays[i];
        }

        float dur = durationOverride ?? despawnDuration;
        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            float raw = Mathf.Clamp01(t / dur);
            float prog = 1f - Mathf.SmoothStep(0f, 1f, raw);
            SetDissolveProgress(prog);
            yield return null;
        }
        SetDissolveProgress(0f);
        // RestoreOriginalMaterials burada çağrılmaz - silah tekrar görünür olur. Gizlendikten sonra EnsureVisible ile restore edilir.
    }

    /// <summary>Aşağıdan yukarıya oluşturur (spawn/build). Süre override edilebilir.</summary>
    public IEnumerator PlaySpawn(float? durationOverride = null)
    {
        if (!_initialized) CacheRenderers();
        if (!_initialized) yield break;

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null || _dissolveMaterialsArrays[i] == null) continue;
            var (minY, maxY) = GetHeightRangeForRenderer(_renderers[i]);
            for (int m = 0; m < _dissolveMaterialsArrays[i].Length; m++)
            {
                var dm = _dissolveMaterialsArrays[i][m];
                if (dm == null) continue;
                dm.SetFloat(HeightMin, minY);
                dm.SetFloat(HeightMax, maxY);
                dm.SetFloat(BuildMode, 1f);
            }
            _renderers[i].sharedMaterials = _dissolveMaterialsArrays[i];
        }
        SetDissolveProgress(-0.01f);

        float dur = durationOverride ?? spawnDuration;
        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            float raw = Mathf.Clamp01(t / dur);
            float prog = Mathf.SmoothStep(0f, 1f, raw);
            SetDissolveProgress(prog);
            yield return null;
        }
        SetDissolveProgress(1f);
        RestoreOriginalMaterials();
    }

    private void SetDissolveProgress(float progress)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_dissolveMaterialsArrays[i] == null) continue;
            foreach (var mat in _dissolveMaterialsArrays[i])
                if (mat != null) mat.SetFloat(DissolveProgress, progress);
        }
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null && _originalMaterialsArrays[i] != null)
                _renderers[i].sharedMaterials = _originalMaterialsArrays[i];
        }
    }

    /// <summary>Silah manuel geçişte (HUD) aktifleştiğinde dissolve materyalleri kaldırıp orijinale döner. newtutorial'da silah görünürlüğü için.</summary>
    public void EnsureVisible()
    {
        if (SceneManager.GetActiveScene().name != "newtutorial") return;
        if (!_initialized) CacheRenderers();
        if (!_initialized) return;
        RestoreOriginalMaterials();
    }

    private void OnDestroy()
    {
        if (_dissolveMaterialsArrays == null) return;
        foreach (var arr in _dissolveMaterialsArrays)
        {
            if (arr == null) continue;
            foreach (var m in arr)
                if (m != null) Destroy(m);
        }
    }
}
