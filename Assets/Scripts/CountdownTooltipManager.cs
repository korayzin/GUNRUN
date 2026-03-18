using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Geri sayım sırasında 1 rastgele tooltip gösterir.
/// Tooltip yavaşça fade-in ile görünür.
/// Her açılışta farklı tooltip seçilir.
/// </summary>
public class CountdownTooltipManager : MonoBehaviour
{
    public static CountdownTooltipManager Instance;

    [Tooltip("Tooltip metnini gösterecek TextMeshProUGUI.")]
    public TextMeshProUGUI tooltipText;

    [Tooltip("Olası tooltip metinleri. Her seferinde 1 tanesi rastgele seçilir.")]
    public string[] tooltips = new string[]
    {
        "Hazırlan!",
        "Son saniyeler...",
        "Dikkat!",
        "Neredeyse!",
        "Başla!",
        "Dikkatli ol!",
        "Hemen geliyor!",
        "İşte geliyor!"
    };

    [Tooltip("Tooltip'in görünür olma süresi (saniye)")]
    [Range(0.3f, 1.5f)]
    public float fadeInDuration = 0.8f;

    private Coroutine _tooltipCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (tooltipText == null)
        {
            var t = transform.Find("TooltipText");
            if (t != null)
                tooltipText = t.GetComponent<TextMeshProUGUI>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        StopCountdownTooltips();
    }

    /// <summary>
    /// Geri sayım boyunca 1 rastgele tooltip gösterir. Duration saniye boyunca çalışır.
    /// </summary>
    public void BeginCountdownTooltips(float duration)
    {
        StopCountdownTooltips();
        if (tooltipText != null && tooltips != null && tooltips.Length > 0)
            _tooltipCoroutine = StartCoroutine(CountdownTooltipsCoroutine(duration));
    }

    /// <summary>
    /// Tooltip gösterimini durdurur ve gizler.
    /// </summary>
    public void StopCountdownTooltips()
    {
        if (_tooltipCoroutine != null)
        {
            StopCoroutine(_tooltipCoroutine);
            _tooltipCoroutine = null;
        }
        HideTooltip();
    }

    /// <summary>
    /// Eski API - artık kullanılmıyor. BeginCountdownTooltips kullan.
    /// </summary>
    public void ShowTooltip(int countdownNumber) { }

    /// <summary>
    /// Tooltip'i anında gizler.
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipText != null)
        {
            tooltipText.gameObject.SetActive(false);
            Color c = tooltipText.color;
            c.a = 1f;
            tooltipText.color = c;
        }
    }

    private IEnumerator CountdownTooltipsCoroutine(float totalDuration)
    {
        HideTooltip();

        if (tooltips.Length == 0) yield break;

        int idx = Random.Range(0, tooltips.Length);
        string tip = tooltips[idx];
        if (string.IsNullOrEmpty(tip)) tip = tooltips[0];

        // Ortada göster
        float showAt = Mathf.Max(0.5f, totalDuration * 0.5f);
        yield return new WaitForSeconds(showAt);
        yield return ShowTooltipAnimated(tip);

        _tooltipCoroutine = null;
    }

    private IEnumerator ShowTooltipAnimated(string text)
    {
        if (tooltipText == null) yield break;

        tooltipText.text = text;
        tooltipText.gameObject.SetActive(true);

        Color baseColor = tooltipText.color;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            // EaseOutQuad - yumuşak giriş
            t = 1f - (1f - t) * (1f - t);
            baseColor.a = t;
            tooltipText.color = baseColor;
            yield return null;
        }

        baseColor.a = 1f;
        tooltipText.color = baseColor;
    }
}
