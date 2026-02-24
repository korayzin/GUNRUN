using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Oyun başladıktan 3 saniye sonra oyuncu hâlâ ateş etmediyse, destructible duvarların üzerinde
/// "kırılabilir" ipucu gösterir (sprite decal). Passthrough ile uyumlu: duvar materyaline dokunulmaz (opacity nabzı yok).
/// </summary>
public class DestructibleMeshHint : MonoBehaviour
{
    [Header("Referans")]
    [SerializeField] private DestructibleGlobalMeshSpawner destructibleGlobalMeshSpawner;

    [Header("Ne zaman çıksın")]
    [Tooltip("Oyun başladıktan bu kadar saniye sonra, oyuncu ateş etmediyse ipucu gösterilir")]
    [SerializeField] private float showHintAfterSeconds = 3f;

    [Header("İpucu - Sprite (duvar üstünde)")]
    [Tooltip("Duvar üzerinde gösterilecek sprite. Atamazsan renkli quad kullanılır.")]
    [SerializeField] private Sprite hintSprite;
    [Tooltip("Segment başına ipucu boyutu (metre). Sprite kare değilse X/Y oranını korumak için scale tek değer.")]
    [SerializeField] private float hintSize = 0.28f;
    [Tooltip("Sprite atanmadığında kullanılacak renk")]
    [SerializeField] private Color hintColorFallback = new Color(1f, 0.45f, 0.1f, 0.5f);
    [Tooltip("Duvara göre hafif dışarı taşma")]
    [SerializeField] private float surfaceOffset = 0.002f;
    [Range(0.1f, 1f)]
    [SerializeField] private float hintDensity = 0.4f;

    [Header("İpucu animasyon")]
    [SerializeField] private bool pulseScale = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinScale = 0.9f;
    [SerializeField] private float pulseMaxScale = 1.1f;

    [Header("Kaldırma")]
    [Tooltip("İlk duvar kırıldığında ipuçları ve duvar nabzı kaldırılır")]
    [SerializeField] private bool hideAfterFirstDestruction = true;

    private readonly List<GameObject> _segments = new List<GameObject>();
    private readonly List<HintDecal> _hintDecals = new List<HintDecal>();
    private DestructibleMeshComponent _currentDestructible;
    private float _meshCreatedTime = -1f;
    private bool _hintsShown;
    private bool _firstDestructionDone;
    private Vector3 _roomCenter;

    /// <summary>TutorialIntroController vb. duvar kırımını dinleyebilir.</summary>
    public static event Action OnWallDestroyed;

    public static void NotifyWallDestroyed()
    {
        OnWallDestroyed?.Invoke();
        var hint = FindObjectOfType<DestructibleMeshHint>();
        if (hint != null)
            hint.OnFirstWallDestroyed();
    }

    private void OnEnable()
    {
        if (destructibleGlobalMeshSpawner != null)
            destructibleGlobalMeshSpawner.OnDestructibleMeshCreated.AddListener(OnDestructibleMeshCreated);
    }

    private void OnDisable()
    {
        if (destructibleGlobalMeshSpawner != null)
            destructibleGlobalMeshSpawner.OnDestructibleMeshCreated.RemoveListener(OnDestructibleMeshCreated);
    }

    private void OnDestructibleMeshCreated(DestructibleMeshComponent destructibleMeshComponent)
    {
        if (destructibleMeshComponent == null) return;

        ClearAll();
        _currentDestructible = destructibleMeshComponent;
        _meshCreatedTime = Time.time;
        _hintsShown = false;
        _segments.Clear();
        destructibleMeshComponent.GetDestructibleMeshSegments(_segments);
        _roomCenter = destructibleMeshComponent.transform.position;

        for (int i = 0; i < _segments.Count; i++)
        {
            GameObject segment = _segments[i];
            if (segment == destructibleMeshComponent.ReservedSegment)
                continue;
            if (segment.GetComponent<MeshCollider>() == null)
                segment.AddComponent<MeshCollider>();
        }
    }

    private void Update()
    {
        if (_firstDestructionDone) return;

        // 3 saniye doldu mu, oyuncu ateş etmedi mi -> ipucu göster
        if (!_hintsShown && _meshCreatedTime >= 0f && Time.time - _meshCreatedTime >= showHintAfterSeconds)
        {
            if (!WeaponManager.HasPlayerFiredSinceLevelLoad)
            {
                ShowHints();
                _hintsShown = true;
            }
            else
            {
                _hintsShown = true; // Ateş ettiyse bir daha kontrol etme
            }
        }

        if (_firstDestructionDone) return;

        // İpucu nabız (sadece decal scale; duvar materyaline dokunulmaz - passthrough uyumlu)
        if (pulseScale && _hintDecals.Count > 0)
        {
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, pulse);
            for (int i = _hintDecals.Count - 1; i >= 0; i--)
            {
                var h = _hintDecals[i];
                if (h.transform == null)
                {
                    if (h.material != null) Destroy(h.material);
                    _hintDecals.RemoveAt(i);
                    continue;
                }
                float s = h.baseScale * scale;
                h.transform.localScale = new Vector3(s, s, s);
            }
        }
    }

    private void ShowHints()
    {
        if (_currentDestructible == null || _segments.Count == 0) return;

        for (int i = 0; i < _segments.Count; i++)
        {
            GameObject segment = _segments[i];
            if (segment == _currentDestructible.ReservedSegment) continue;
            if (hintDensity < 1f && (i % Mathf.Max(1, Mathf.RoundToInt(1f / hintDensity)) != 0))
                continue;
            AddHintToSegment(segment);
        }
    }

    private void AddHintToSegment(GameObject segment)
    {
        MeshRenderer segRenderer = segment.GetComponent<MeshRenderer>();
        MeshFilter segFilter = segment.GetComponent<MeshFilter>();
        if (segRenderer == null || segFilter == null || segFilter.sharedMesh == null) return;

        Bounds bounds = segRenderer.bounds;
        Vector3 center = bounds.center;
        Vector3 normal = (center - _roomCenter).normalized;
        if (normal.sqrMagnitude < 0.01f) normal = segRenderer.transform.forward;

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        var col = quad.GetComponent<Collider>();
        if (col != null) Destroy(col);
        quad.name = "DestructibleHint";
        quad.transform.SetParent(segment.transform, true);
        quad.transform.position = center + normal * surfaceOffset;
        quad.transform.rotation = Quaternion.LookRotation(normal);

        float size = hintSize;
        if (hintSprite != null && hintSprite.rect.width > 0 && hintSprite.rect.height > 0)
        {
            float aspect = (float)hintSprite.rect.width / hintSprite.rect.height;
            quad.transform.localScale = aspect >= 1f ? new Vector3(size * aspect, size, 1f) : new Vector3(size, size / aspect, 1f);
        }
        else
        {
            quad.transform.localScale = Vector3.one * size;
        }

        Material mat;
        if (hintSprite != null && hintSprite.texture != null)
        {
            mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = hintSprite.texture;
            Rect r = hintSprite.textureRect;
            float w = hintSprite.texture.width;
            float h = hintSprite.texture.height;
            if (w > 0 && h > 0)
            {
                mat.mainTextureOffset = new Vector2(r.xMin / w, r.yMin / h);
                mat.mainTextureScale = new Vector2(r.width / w, r.height / h);
            }
            mat.color = Color.white;
        }
        else
        {
            mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = hintColorFallback;
        }

        var renderer = quad.GetComponent<MeshRenderer>();
        if (renderer != null)
            renderer.material = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        _hintDecals.Add(new HintDecal
        {
            transform = quad.transform,
            renderer = renderer,
            baseScale = size,
            material = mat
        });
    }

    private void OnFirstWallDestroyed()
    {
        _firstDestructionDone = true;
        if (!hideAfterFirstDestruction) return;

        foreach (var h in _hintDecals)
        {
            if (h.transform != null && h.material != null)
                StartCoroutine(FadeOutAndDestroy(h));
        }
        _hintDecals.Clear();
    }

    private IEnumerator FadeOutAndDestroy(HintDecal h)
    {
        if (h.transform == null || h.material == null) yield break;
        float duration = 0.5f;
        float t = 0f;
        Color start = h.material.color;
        while (t < duration && h.material != null)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(1f - t / duration);
            h.material.color = new Color(start.r, start.g, start.b, start.a * a);
            yield return null;
        }
        if (h.material != null) Destroy(h.material);
        if (h.transform != null) Destroy(h.transform.gameObject);
    }

    private void ClearAll()
    {
        foreach (var h in _hintDecals)
        {
            if (h.material != null) Destroy(h.material);
            if (h.transform != null) Destroy(h.transform.gameObject);
        }
        _hintDecals.Clear();
    }

    private struct HintDecal
    {
        public Transform transform;
        public MeshRenderer renderer;
        public float baseScale;
        public Material material;
    }
}
