using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Atadığın panelleri oyun başladıktan 9 saniye sonra (portalların açılmasından 1.5 sn sonra)
/// animasyonla gösterir, 5 saniye sonra animasyonla kaldırır.
/// Boş bir GameObject'e ekleyip Inspector'dan panelleri sürükleyebilirsin.
/// </summary>
public class WayManager : MonoBehaviour
{
    [Header("Way Panelleri")]
    [Tooltip("Gösterilecek paneller (UI veya 3D). Boş bırakırsan hiçbir şey olmaz.")]
    public List<GameObject> panels = new List<GameObject>();

    [Header("Zamanlama")]
    [Tooltip("Oyun başladıktan kaç saniye sonra paneller gösterilsin (portallar ~7.5+1.5 sn)")]
    public float showDelay = 9f;
    [Tooltip("Paneller ekranda kaç saniye kalsın")]
    public float visibleDuration = 5f;

    [Header("Animasyon")]
    [Tooltip("Panel giriş animasyonu süresi")]
    public float showAnimationDuration = 0.6f;
    [Tooltip("Panel çıkış animasyonu süresi")]
    public float hideAnimationDuration = 0.5f;
    [Tooltip("Girişte hafif overshoot (zıplama)")]
    [Range(1f, 1.3f)]
    public float showOvershoot = 1.08f;

    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();

    private void Start()
    {
        if (panels == null || panels.Count == 0)
            return;

        originalScales.Clear();
        foreach (GameObject p in panels)
        {
            if (p == null) continue;
            Vector3 sc = GetCurrentScale(p);
            if (sc == Vector3.zero) sc = Vector3.one;
            originalScales[p] = sc;
            p.SetActive(true);
            SetPanelState(p, false);
        }

        StartCoroutine(WaySequence());
    }

    private IEnumerator WaySequence()
    {
        yield return new WaitForSeconds(showDelay);

        // Giriş animasyonu
        foreach (GameObject p in panels)
        {
            if (p != null && p.activeInHierarchy)
                StartCoroutine(AnimateShow(p));
        }
        yield return new WaitForSeconds(showAnimationDuration);

        yield return new WaitForSeconds(visibleDuration);

        // Çıkış animasyonu
        List<Coroutine> hideRoutines = new List<Coroutine>();
        foreach (GameObject p in panels)
        {
            if (p != null && p.activeInHierarchy)
                hideRoutines.Add(StartCoroutine(AnimateHide(p)));
        }
        foreach (var c in hideRoutines)
            yield return c; // Tüm çıkış animasyonları bitsin
    }

    private Vector3 GetCurrentScale(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        Transform t = rect != null ? rect : panel.transform;
        return t.localScale;
    }

    private void SetPanelState(GameObject panel, bool visible)
    {
        if (panel == null) return;

        RectTransform rect = panel.GetComponent<RectTransform>();
        Transform trans = rect != null ? rect : panel.transform;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        if (visible)
        {
            trans.localScale = originalScales.TryGetValue(panel, out Vector3 sc) ? sc : Vector3.one;
            if (cg != null) cg.alpha = 1f;
        }
        else
        {
            trans.localScale = Vector3.zero;
            if (cg != null) cg.alpha = 0f;
        }
    }

    private bool TryGetOriginalScale(GameObject panel, out Vector3 scale)
    {
        if (originalScales != null && originalScales.TryGetValue(panel, out scale))
            return true;
        scale = GetCurrentScale(panel);
        if (scale == Vector3.zero) scale = Vector3.one;
        return false;
    }

    private IEnumerator AnimateShow(GameObject panel)
    {
        if (panel == null) yield break;

        TryGetOriginalScale(panel, out Vector3 targetScale);
        RectTransform rect = panel.GetComponent<RectTransform>();
        Transform trans = rect != null ? rect : panel.transform;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        if (cg != null) cg.alpha = 0f;
        trans.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < showAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / showAnimationDuration);
            float easedT = EaseOutBack(t, showOvershoot);

            trans.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, easedT);
            if (cg != null) cg.alpha = Mathf.Lerp(0f, 1f, EaseOutQuad(t));

            yield return null;
        }

        trans.localScale = targetScale;
        if (cg != null) cg.alpha = 1f;
    }

    private IEnumerator AnimateHide(GameObject panel)
    {
        if (panel == null) yield break;

        TryGetOriginalScale(panel, out Vector3 startScale);
        RectTransform rect = panel.GetComponent<RectTransform>();
        Transform trans = rect != null ? rect : panel.transform;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        float startAlpha = cg != null ? cg.alpha : 1f;

        float elapsed = 0f;
        while (elapsed < hideAnimationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / hideAnimationDuration);
            float easedT = EaseInBack(t);

            trans.localScale = Vector3.Lerp(startScale, Vector3.zero, easedT);
            if (cg != null) cg.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        trans.localScale = Vector3.zero;
        if (cg != null) cg.alpha = 0f;
    }

    private static float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private static float EaseOutBack(float t, float overshoot = 1.70158f)
    {
        float c1 = overshoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private static float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
}
