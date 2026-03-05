using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Harita seçim paneli butonları için havalı hover animasyonu.
/// Elastic bounce, glow, rotasyon + metin animasyonu — HandRayUI ile kullanılır.
/// </summary>
[RequireComponent(typeof(Button))]
public class MapSelectionButtonHover : MonoBehaviour
{
    [Header("Scale")]
    [Tooltip("Hover'da maksimum büyüme (elastic bounce ile)")]
    [Range(1.02f, 1.2f)]
    public float hoverScale = 1.08f;
    
    [Tooltip("Elastic bounce yoğunluğu (0 = düz, 1.5 = belirgin zıplama)")]
    [Range(0f, 2f)]
    public float bounceOvershoot = 0.4f;

    [Header("Rotasyon")]
    [Tooltip("Hover'da hafif eğim açısı (derece)")]
    [Range(-5f, 5f)]
    public float hoverTiltAngle = 2f;

    [Header("Glow / Renk")]
    [Tooltip("Hover'da Image rengini parlat (ekstra parlaklık)")]
    [Range(0f, 0.3f)]
    public float colorBoost = 0.12f;
    
    [Tooltip("Glow gölgesi genişliği (0 = kapalı, >0 ise Shadow yoksa eklenir)")]
    [Range(0f, 20f)]
    public float glowShadowSize = 6f;

    [Header("Metin (Text)")]
    [Tooltip("Animasyonlu metin — boşsa buton içindeki TMP_Text otomatik bulunur")]
    public TMP_Text targetText;
    [Tooltip("Hover'da harf aralığı artışı (açılma efekti)")]
    [Range(0f, 25f)]
    public float textLetterSpacing = 6f;
    [Tooltip("Hover'da metin parlaklığı (0 = yok, 0.3 = çok parlak)")]
    [Range(0f, 0.4f)]
    public float textColorBoost = 0.08f;
    [Tooltip("Hover'da font boyutu artışı")]
    [Range(0f, 8f)]
    public float textSizeBoost = 2f;

    [Header("Timing")]
    [Tooltip("Hover giriş animasyon süresi")]
    [Range(0.05f, 0.4f)]
    public float enterDuration = 0.18f;
    
    [Tooltip("Hover çıkış animasyon süresi")]
    [Range(0.05f, 0.4f)]
    public float exitDuration = 0.15f;

    private RectTransform _rect;
    private Image _image;
    private Shadow _shadowOrOutline;
    private Vector3 _originalScale;
    private Vector3 _originalEuler;
    private Color _originalColor;
    private float _originalLetterSpacing;
    private float _originalFontSize;
    private Color _originalTextColor;
    private Coroutine _animCoroutine;
    private bool _isHovered;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
        _shadowOrOutline = GetComponent<Shadow>();
        if (targetText == null)
            targetText = GetComponentInChildren<TMP_Text>(true);
        if (targetText != null)
        {
            _originalLetterSpacing = targetText.characterSpacing;
            _originalFontSize = targetText.fontSize;
            _originalTextColor = targetText.color;
        }
        
        if (_rect != null)
        {
            _originalScale = _rect.localScale;
            _originalEuler = _rect.localEulerAngles;
        }
        if (_image != null)
        {
            _originalColor = _image.color;
            if (_shadowOrOutline == null && glowShadowSize > 0)
            {
                var outline = gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 1f, 1f, 0.35f);
                outline.effectDistance = Vector2.zero;
                _shadowOrOutline = outline;
            }
        }
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void OnDisable()
    {
        if (_animCoroutine != null)
        {
            StopCoroutine(_animCoroutine);
            _animCoroutine = null;
        }
        ResetState();
    }

    private void ResetState()
    {
        if (_rect != null)
        {
            _rect.localScale = _originalScale;
            _rect.localEulerAngles = _originalEuler;
        }
        if (_image != null)
            _image.color = _originalColor;
        if (_shadowOrOutline != null)
            _shadowOrOutline.effectDistance = Vector2.zero;
        if (targetText != null)
        {
            targetText.characterSpacing = _originalLetterSpacing;
            targetText.fontSize = _originalFontSize;
            targetText.color = _originalTextColor;
        }
        _isHovered = false;
    }

    public void OnHoverEnter()
    {
        if (_rect == null) return;
        _isHovered = true;

        if (_animCoroutine != null)
            StopCoroutine(_animCoroutine);

        _animCoroutine = StartCoroutine(AnimateToHover(enterDuration, true));
    }

    public void OnHoverExit()
    {
        if (_rect == null) return;
        _isHovered = false;

        if (_animCoroutine != null)
            StopCoroutine(_animCoroutine);

        _animCoroutine = StartCoroutine(AnimateToHover(exitDuration, false));
    }

    private IEnumerator AnimateToHover(float duration, bool toHover)
    {
        Vector3 startScale = _rect.localScale;
        Vector3 startEuler = _rect.localEulerAngles;
        
        Vector3 targetScale = toHover ? _originalScale * hoverScale : _originalScale;
        Vector3 targetEuler = toHover ? _originalEuler + new Vector3(0, 0, hoverTiltAngle) : _originalEuler;
        
        Color startColor = _image != null ? _image.color : _originalColor;
        Color targetColor = toHover 
            ? _originalColor + new Color(colorBoost, colorBoost, colorBoost, 0f) 
            : _originalColor;
        
        Vector2 startShadow = _shadowOrOutline != null ? _shadowOrOutline.effectDistance : Vector2.zero;
        float glowOffset = glowShadowSize / 3f;
        Vector2 targetShadow = toHover && glowShadowSize > 0 
            ? new Vector2(glowOffset, glowOffset) 
            : Vector2.zero;

        // Metin animasyonu
        float startSpacing = targetText != null ? targetText.characterSpacing : _originalLetterSpacing;
        float startFontSize = targetText != null ? targetText.fontSize : _originalFontSize;
        Color startTextColor = targetText != null ? targetText.color : _originalTextColor;
        float targetSpacing = toHover ? _originalLetterSpacing + textLetterSpacing : _originalLetterSpacing;
        float targetFontSize = toHover ? _originalFontSize + textSizeBoost : _originalFontSize;
        Color targetTextColor = toHover 
            ? _originalTextColor + new Color(textColorBoost, textColorBoost, textColorBoost, 0f) 
            : _originalTextColor;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = EaseOutQuad(t);
            
            float scaleT = toHover ? EaseOutBack(t, bounceOvershoot) : EaseOutQuad(t);
            _rect.localScale = Vector3.LerpUnclamped(startScale, targetScale, scaleT);
            _rect.localEulerAngles = Vector3.Lerp(startEuler, targetEuler, smoothT);

            if (_image != null)
                _image.color = Color.Lerp(startColor, targetColor, smoothT);
            if (_shadowOrOutline != null && glowShadowSize > 0)
                _shadowOrOutline.effectDistance = Vector2.Lerp(startShadow, targetShadow, smoothT);

            if (targetText != null)
            {
                float textT = toHover ? EaseOutBack(t, bounceOvershoot * 0.6f) : smoothT;
                targetText.characterSpacing = Mathf.LerpUnclamped(startSpacing, targetSpacing, textT);
                targetText.fontSize = Mathf.Lerp(startFontSize, targetFontSize, textT);
                targetText.color = Color.Lerp(startTextColor, targetTextColor, smoothT);
            }

            yield return null;
        }

        _rect.localScale = targetScale;
        _rect.localEulerAngles = targetEuler;
        if (_image != null)
            _image.color = targetColor;
        if (_shadowOrOutline != null)
            _shadowOrOutline.effectDistance = targetShadow;
        if (targetText != null)
        {
            targetText.characterSpacing = targetSpacing;
            targetText.fontSize = targetFontSize;
            targetText.color = targetTextColor;
        }

        _animCoroutine = null;
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    private static float EaseOutBack(float t, float overshoot)
    {
        float c1 = 1f + overshoot;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + (c1 - 1f) * Mathf.Pow(t - 1f, 2f);
    }
}
