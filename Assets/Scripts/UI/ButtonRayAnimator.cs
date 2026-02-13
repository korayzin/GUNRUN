using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Ray/hover ve tıklama için buton animasyonu.
/// - Hover: Hafif büyüme + harf aralığı açılması
/// - Tıklama: Harf aralığı daha çok açılıp eski haline dönme
/// Hem mouse/touch (IPointerEvents) hem VR ray (HandRayUIInteractor) ile çalışır.
/// Butona ekleyip kullan. BubbleButtonAnimator ile aynı butonda kullanma.
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonRayAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale")]
    [Tooltip("Hover'da büyüme çarpanı")]
    [Range(1.01f, 1.2f)]
    public float hoverScaleMultiplier = 1.06f;

    [Header("Harf Aralığı (Letter Spacing)")]
    [Tooltip("Normal harf aralığı")]
    public float normalLetterSpacing = 0f;
    
    [Tooltip("Hover'da harf aralığı artışı")]
    [Range(0f, 20f)]
    public float hoverLetterSpacing = 4f;
    
    [Tooltip("Tıklamada harf aralığı artışı (peak)")]
    [Range(0f, 40f)]
    public float clickLetterSpacing = 12f;

    [Header("Animasyon Süreleri")]
    [Tooltip("Hover giriş/çıkış süresi")]
    [Range(0.05f, 0.3f)]
    public float hoverDuration = 0.12f;
    
    [Tooltip("Tıklama animasyonu süresi")]
    [Range(0.05f, 0.25f)]
    public float clickDuration = 0.15f;

    [Header("VR Haptic (Opsiyonel)")]
    [Tooltip("Hover'da hafif haptic")]
    public bool hapticOnHover = true;
    
    [Tooltip("Tıklamada haptic")]
    public bool hapticOnClick = true;
    
    [Tooltip("Haptic şiddeti (0-1)")]
    [Range(0.1f, 1f)]
    public float hapticStrength = 0.3f;

    [Header("Referanslar")]
    [Tooltip("Boşsa otomatik bulunur - buton içindeki TMP_Text")]
    public TMP_Text targetText;

    [Header("Ses (Opsiyonel)")]
    [Tooltip("Tıklamada çalacak ses")]
    public AudioClip clickSound;

    private RectTransform _rectTransform;
    private Button _button;
    private Vector3 _originalScale;
    private float _originalLetterSpacing;
    private Coroutine _scaleCoroutine;
    private Coroutine _spacingCoroutine;
    private Coroutine _clickSpacingCoroutine;
    private bool _isHovered;
    private bool _isPressed;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _button = GetComponent<Button>();
        _originalScale = _rectTransform.localScale;

        if (targetText == null)
            targetText = GetComponentInChildren<TMP_Text>(true);

        if (targetText != null)
            _originalLetterSpacing = targetText.characterSpacing;
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void OnDisable()
    {
        StopAllAnimations();
        ResetState();
    }

    private void LateUpdate()
    {
        // Disabled olduğunda hover efektini sıfırla
        if (_isHovered && !_button.interactable)
        {
            _isHovered = false;
            StopAllAnimations();
            ResetState();
        }
    }

    private void ResetState()
    {
        _rectTransform.localScale = _originalScale;
        if (targetText != null)
            targetText.characterSpacing = _originalLetterSpacing;
        _isHovered = false;
        _isPressed = false;
    }

    private void StopAllAnimations()
    {
        if (_scaleCoroutine != null) { StopCoroutine(_scaleCoroutine); _scaleCoroutine = null; }
        if (_spacingCoroutine != null) { StopCoroutine(_spacingCoroutine); _spacingCoroutine = null; }
        if (_clickSpacingCoroutine != null) { StopCoroutine(_clickSpacingCoroutine); _clickSpacingCoroutine = null; }
    }

    #region Pointer Events (Mouse / Touch)

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        OnHoverEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnHoverExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        OnPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnReleased();
    }

    #endregion

    #region Public API (HandRayUIInteractor için)

    /// <summary>
    /// Ray üzerine geldiğinde çağrılır.
    /// </summary>
    public void OnHoverEnter()
    {
        if (!_button.interactable) return;
        _isHovered = true;

        StopAllAnimations();

        AnimateScale(_originalScale * hoverScaleMultiplier, hoverDuration);
        AnimateLetterSpacing(_originalLetterSpacing + hoverLetterSpacing, hoverDuration);

        if (hapticOnHover)
            TriggerHaptic(0.08f, hapticStrength * 0.5f);
    }

    /// <summary>
    /// Ray üzerinden çıktığında çağrılır.
    /// </summary>
    public void OnHoverExit()
    {
        _isHovered = false;

        if (!_isPressed)
        {
            StopAllAnimations();
            AnimateScale(_originalScale, hoverDuration);
            AnimateLetterSpacing(_originalLetterSpacing, hoverDuration);
        }
    }

    /// <summary>
    /// Tıklandığında çağrılır (trigger basıldığında).
    /// </summary>
    public void OnPressed()
    {
        if (!_button.interactable) return;
        _isPressed = true;

        if (clickSound != null)
        {
            var src = GetComponent<AudioSource>();
            if (src == null) src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 0f; // 2D UI sesi
            src.PlayOneShot(clickSound);
        }

        if (_clickSpacingCoroutine != null)
        {
            StopCoroutine(_clickSpacingCoroutine);
            _clickSpacingCoroutine = null;
        }

        _clickSpacingCoroutine = StartCoroutine(ClickLetterSpacingSequence());
        if (hapticOnClick)
            TriggerHaptic(0.05f, hapticStrength);
    }

    /// <summary>
    /// Tıklama bittiğinde çağrılır (trigger bırakıldığında).
    /// </summary>
    public void OnReleased()
    {
        _isPressed = false;
    }

    #endregion

    #region Animations

    private void AnimateScale(Vector3 target, float duration)
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleCoroutine(target, duration));
    }

    private IEnumerator ScaleCoroutine(Vector3 target, float duration)
    {
        Vector3 start = _rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutQuad(Mathf.Clamp01(elapsed / duration));
            _rectTransform.localScale = Vector3.LerpUnclamped(start, target, t);
            yield return null;
        }

        _rectTransform.localScale = target;
        _scaleCoroutine = null;
    }

    private void AnimateLetterSpacing(float target, float duration)
    {
        if (targetText == null) return;
        if (_spacingCoroutine != null) StopCoroutine(_spacingCoroutine);
        _spacingCoroutine = StartCoroutine(LetterSpacingCoroutine(target, duration));
    }

    private IEnumerator LetterSpacingCoroutine(float target, float duration)
    {
        float start = targetText.characterSpacing;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutQuad(Mathf.Clamp01(elapsed / duration));
            targetText.characterSpacing = Mathf.LerpUnclamped(start, target, t);
            yield return null;
        }

        targetText.characterSpacing = target;
        _spacingCoroutine = null;
    }

    /// <summary>
    /// Tıklamada: Harf aralığı açılır, sonra eski haline döner.
    /// </summary>
    private IEnumerator ClickLetterSpacingSequence()
    {
        if (targetText == null) { _clickSpacingCoroutine = null; yield break; }

        float baseSpacing = _isHovered ? _originalLetterSpacing + hoverLetterSpacing : _originalLetterSpacing;
        float peakSpacing = baseSpacing + clickLetterSpacing;

        // Açılma
        float elapsed = 0f;
        float half = clickDuration * 0.4f;
        float start = targetText.characterSpacing;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutQuad(Mathf.Clamp01(elapsed / half));
            targetText.characterSpacing = Mathf.LerpUnclamped(start, peakSpacing, t);
            yield return null;
        }

        targetText.characterSpacing = peakSpacing;

        // Geri dönüş
        elapsed = 0f;
        float secondHalf = clickDuration * 0.6f;

        while (elapsed < secondHalf)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutBack(Mathf.Clamp01(elapsed / secondHalf));
            targetText.characterSpacing = Mathf.LerpUnclamped(peakSpacing, baseSpacing, t);
            yield return null;
        }

        targetText.characterSpacing = baseSpacing;
        _clickSpacingCoroutine = null;
        _isPressed = false;
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    #endregion

    #region Haptic

    private void TriggerHaptic(float duration, float amplitude)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(HapticCoroutine(duration, amplitude));
#endif
    }

    private IEnumerator HapticCoroutine(float duration, float amplitude)
    {
        var controller = OVRInput.Controller.LTouch | OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(0.5f, Mathf.Clamp01(amplitude), controller);
        yield return new WaitForSecondsRealtime(duration);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }

    #endregion
}
