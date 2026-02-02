using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Bubble temalı buton animasyonu - Hover, Click ve Idle (nefes alma) efektleri
/// Herhangi bir UI Button'a eklenebilir
/// </summary>
[RequireComponent(typeof(Button))]
public class BubbleButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Ayarları")]
    [Tooltip("Normal boyut")]
    public float normalScale = 1f;
    
    [Tooltip("Hover durumunda boyut")]
    public float hoverScale = 1.08f;
    
    [Tooltip("Tıklama anında küçülme")]
    public float pressedScale = 0.92f;
    
    [Tooltip("Tıklama sonrası bounce büyüklüğü")]
    public float bounceScale = 1.15f;

    [Header("Animasyon Süreleri")]
    [Tooltip("Hover animasyon süresi")]
    public float hoverDuration = 0.15f;
    
    [Tooltip("Tıklama squeeze süresi")]
    public float pressDuration = 0.08f;
    
    [Tooltip("Bounce animasyon süresi")]
    public float bounceDuration = 0.3f;

    [Header("Idle Nefes Alma Efekti")]
    [Tooltip("Nefes alma efekti aktif mi?")]
    public bool enableBreathing = true;
    
    [Tooltip("Nefes alma genliği (scale değişimi)")]
    [Range(0.01f, 0.1f)]
    public float breathingAmount = 0.03f;
    
    [Tooltip("Nefes alma hızı")]
    [Range(0.5f, 3f)]
    public float breathingSpeed = 1.2f;

    [Header("Renk Efektleri")]
    [Tooltip("Hover'da parlaklık artışı")]
    public bool enableColorEffect = true;
    
    [Tooltip("Hover renk çarpanı")]
    public float hoverBrightness = 1.1f;

    // Private değişkenler
    private RectTransform rectTransform;
    private Button button;
    private Image buttonImage;
    private Color originalColor;
    private Vector3 originalScale;
    
    private Coroutine currentScaleCoroutine;
    private Coroutine breathingCoroutine;
    
    private bool isHovered = false;
    private bool isPressed = false;
    private bool isAnimating = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        
        originalScale = rectTransform.localScale;
    }

    private void OnEnable()
    {
        // Başlangıç scale'ini ayarla
        rectTransform.localScale = originalScale * normalScale;
        
        // Nefes alma efektini başlat
        if (enableBreathing)
        {
            breathingCoroutine = StartCoroutine(BreathingAnimation());
        }
    }

    private void OnDisable()
    {
        // Tüm coroutine'leri durdur
        if (currentScaleCoroutine != null)
        {
            StopCoroutine(currentScaleCoroutine);
        }
        if (breathingCoroutine != null)
        {
            StopCoroutine(breathingCoroutine);
        }
        
        // Scale'i sıfırla
        rectTransform.localScale = originalScale * normalScale;
        
        // Rengi sıfırla
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }

    #region Pointer Events

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovered = true;
        
        // Nefes alma efektini durdur
        if (breathingCoroutine != null)
        {
            StopCoroutine(breathingCoroutine);
            breathingCoroutine = null;
        }
        
        // Hover animasyonu
        AnimateScale(hoverScale, hoverDuration, EaseOutBack);
        
        // Renk efekti
        if (enableColorEffect && buttonImage != null)
        {
            Color targetColor = originalColor * hoverBrightness;
            targetColor.a = originalColor.a;
            StartCoroutine(AnimateColor(targetColor, hoverDuration));
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        
        if (!isPressed)
        {
            // Normal boyuta dön
            AnimateScale(normalScale, hoverDuration, EaseOutQuad);
            
            // Rengi sıfırla
            if (enableColorEffect && buttonImage != null)
            {
                StartCoroutine(AnimateColor(originalColor, hoverDuration));
            }
            
            // Nefes alma efektini tekrar başlat
            if (enableBreathing)
            {
                breathingCoroutine = StartCoroutine(BreathingAnimation());
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = true;
        
        // Squeeze animasyonu (hızlı küçülme)
        AnimateScale(pressedScale, pressDuration, EaseOutQuad);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = false;
        
        // Bounce animasyonu başlat
        StartCoroutine(BounceAnimation());
    }

    #endregion

    #region Animasyonlar

    private void AnimateScale(float targetScale, float duration, System.Func<float, float> easeFunction)
    {
        if (currentScaleCoroutine != null)
        {
            StopCoroutine(currentScaleCoroutine);
        }
        
        currentScaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale, duration, easeFunction));
    }

    private IEnumerator ScaleCoroutine(float targetScale, float duration, System.Func<float, float> easeFunction)
    {
        isAnimating = true;
        
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = originalScale * targetScale;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easeFunction(t);
            
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, easedT);
            
            yield return null;
        }
        
        rectTransform.localScale = endScale;
        isAnimating = false;
    }

    private IEnumerator BounceAnimation()
    {
        isAnimating = true;
        
        Vector3 startScale = rectTransform.localScale;
        Vector3 bounceScaleVec = originalScale * bounceScale;
        Vector3 finalScale = originalScale * (isHovered ? hoverScale : normalScale);
        
        // İlk aşama: Bounce yukarı
        float elapsed = 0f;
        float halfDuration = bounceDuration * 0.4f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            float easedT = EaseOutQuad(t);
            
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, bounceScaleVec, easedT);
            
            yield return null;
        }
        
        // İkinci aşama: Geri dön (elastic efekti ile)
        elapsed = 0f;
        float secondHalf = bounceDuration * 0.6f;
        startScale = rectTransform.localScale;
        
        while (elapsed < secondHalf)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / secondHalf);
            float easedT = EaseOutElastic(t);
            
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, finalScale, easedT);
            
            yield return null;
        }
        
        rectTransform.localScale = finalScale;
        isAnimating = false;
        
        // Hover'da değilse nefes alma efektini başlat
        if (!isHovered && enableBreathing)
        {
            breathingCoroutine = StartCoroutine(BreathingAnimation());
        }
    }

    private IEnumerator BreathingAnimation()
    {
        while (true)
        {
            // Hover veya press durumunda nefes alma efektini durdur
            if (isHovered || isPressed || isAnimating)
            {
                yield return null;
                continue;
            }
            
            // Sinüs dalgası ile yumuşak nefes alma
            float breathValue = Mathf.Sin(Time.unscaledTime * breathingSpeed * Mathf.PI) * breathingAmount;
            float targetScale = normalScale + breathValue;
            
            rectTransform.localScale = originalScale * targetScale;
            
            yield return null;
        }
    }

    private IEnumerator AnimateColor(Color targetColor, float duration)
    {
        Color startColor = buttonImage.color;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            buttonImage.color = Color.Lerp(startColor, targetColor, EaseOutQuad(t));
            
            yield return null;
        }
        
        buttonImage.color = targetColor;
    }

    #endregion

    #region Easing Fonksiyonları

    private float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }

    private float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private float EaseOutElastic(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        
        float c4 = (2f * Mathf.PI) / 3f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }

    #endregion

    #region Public Metodlar

    /// <summary>
    /// Programatik olarak click animasyonu tetikle
    /// </summary>
    public void TriggerClickAnimation()
    {
        StartCoroutine(BounceAnimation());
    }

    /// <summary>
    /// Nefes alma efektini aç/kapa
    /// </summary>
    public void SetBreathingEnabled(bool enabled)
    {
        enableBreathing = enabled;
        
        if (enabled && breathingCoroutine == null && !isHovered && !isPressed)
        {
            breathingCoroutine = StartCoroutine(BreathingAnimation());
        }
        else if (!enabled && breathingCoroutine != null)
        {
            StopCoroutine(breathingCoroutine);
            breathingCoroutine = null;
            rectTransform.localScale = originalScale * normalScale;
        }
    }

    /// <summary>
    /// Buton rengini güncelle (runtime'da değişiklik için)
    /// </summary>
    public void UpdateOriginalColor(Color newColor)
    {
        originalColor = newColor;
        if (!isHovered && buttonImage != null)
        {
            buttonImage.color = newColor;
        }
    }

    #endregion
}
