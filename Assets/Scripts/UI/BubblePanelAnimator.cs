using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Bubble temalı panel animasyonu - Dalgalanma, nefes alma ve wobbly efektleri
/// Panel'e organik, canlı bir görünüm verir
/// </summary>
public class BubblePanelAnimator : MonoBehaviour
{
    [Header("Dalgalanma Efekti")]
    [Tooltip("Dalgalanma efekti aktif mi?")]
    public bool enableWobble = true;
    
    [Tooltip("Dalgalanma genliği (scale)")]
    [Range(0.005f, 0.05f)]
    public float wobbleAmountX = 0.015f;
    
    [Tooltip("Dalgalanma genliği (scale Y)")]
    [Range(0.005f, 0.05f)]
    public float wobbleAmountY = 0.012f;
    
    [Tooltip("Dalgalanma hızı")]
    [Range(0.5f, 3f)]
    public float wobbleSpeed = 1.2f;
    
    [Tooltip("X ve Y dalgalanması arasındaki faz farkı")]
    [Range(0f, 2f)]
    public float phaseOffset = 0.7f;

    [Header("Rotation Dalgalanması")]
    [Tooltip("Hafif dönme efekti aktif mi?")]
    public bool enableRotationWobble = true;
    
    [Tooltip("Maksimum dönme açısı (derece)")]
    [Range(0.1f, 3f)]
    public float rotationAmount = 0.8f;
    
    [Tooltip("Dönme hızı")]
    [Range(0.3f, 2f)]
    public float rotationSpeed = 0.8f;

    [Header("Position Sway (Sallanma)")]
    [Tooltip("Hafif pozisyon kayması aktif mi?")]
    public bool enablePositionSway = false;
    
    [Tooltip("Pozisyon kayma miktarı (piksel)")]
    [Range(1f, 10f)]
    public float swayAmount = 3f;
    
    [Tooltip("Sallanma hızı")]
    [Range(0.3f, 2f)]
    public float swaySpeed = 0.6f;

    [Header("Giriş Animasyonu")]
    [Tooltip("Panel açılırken animasyon yap")]
    public bool enableEntranceAnimation = true;
    
    [Tooltip("Giriş animasyonu süresi")]
    public float entranceDuration = 0.5f;
    
    [Tooltip("Giriş animasyonu tipi")]
    public EntranceType entranceType = EntranceType.BubblePop;

    [Header("Pulse Efekti (Opsiyonel)")]
    [Tooltip("Periyodik pulse efekti")]
    public bool enablePulse = false;
    
    [Tooltip("Pulse aralığı (saniye)")]
    public float pulseInterval = 3f;
    
    [Tooltip("Pulse büyüklüğü")]
    [Range(1.01f, 1.1f)]
    public float pulseScale = 1.03f;
    
    [Tooltip("Pulse süresi")]
    public float pulseDuration = 0.4f;

    public enum EntranceType
    {
        None,
        BubblePop,      // Küçükten büyüğe bounce ile
        FadeIn,         // Opaklık ile
        SlideUp,        // Aşağıdan yukarı
        Expand          // Merkezden genişle
    }

    // Private değişkenler
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    private float wobbleTimeX;
    private float wobbleTimeY;
    private float rotationTime;
    private float swayTime;
    
    private bool isAnimating = false;
    private bool entranceComplete = false;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        // CanvasGroup yoksa ve fade animasyonu gerekiyorsa ekle
        if (canvasGroup == null && entranceType == EntranceType.FadeIn)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        originalScale = rectTransform.localScale;
        originalPosition = rectTransform.anchoredPosition;
        originalRotation = rectTransform.localRotation;
        
        // Rastgele başlangıç fazları (her panel farklı görünsün)
        wobbleTimeX = Random.Range(0f, Mathf.PI * 2f);
        wobbleTimeY = Random.Range(0f, Mathf.PI * 2f);
        rotationTime = Random.Range(0f, Mathf.PI * 2f);
        swayTime = Random.Range(0f, Mathf.PI * 2f);
    }

    private void OnEnable()
    {
        // Giriş animasyonu
        if (enableEntranceAnimation && entranceType != EntranceType.None)
        {
            StartCoroutine(PlayEntranceAnimation());
        }
        else
        {
            entranceComplete = true;
        }
        
        // Pulse efektini başlat
        if (enablePulse)
        {
            pulseCoroutine = StartCoroutine(PulseLoop());
        }
    }

    private void OnDisable()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        
        // Değerleri sıfırla
        rectTransform.localScale = originalScale;
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localRotation = originalRotation;
        
        entranceComplete = false;
    }

    private void Update()
    {
        if (!entranceComplete || isAnimating) return;
        
        ApplyWobbleEffects();
    }

    private void ApplyWobbleEffects()
    {
        Vector3 targetScale = originalScale;
        Vector3 targetPosition = originalPosition;
        Quaternion targetRotation = originalRotation;
        
        // Scale dalgalanması
        if (enableWobble)
        {
            wobbleTimeX += Time.unscaledDeltaTime * wobbleSpeed;
            wobbleTimeY += Time.unscaledDeltaTime * wobbleSpeed;
            
            float scaleOffsetX = Mathf.Sin(wobbleTimeX) * wobbleAmountX;
            float scaleOffsetY = Mathf.Sin(wobbleTimeY + phaseOffset * Mathf.PI) * wobbleAmountY;
            
            targetScale = new Vector3(
                originalScale.x * (1f + scaleOffsetX),
                originalScale.y * (1f + scaleOffsetY),
                originalScale.z
            );
        }
        
        // Rotation dalgalanması
        if (enableRotationWobble)
        {
            rotationTime += Time.unscaledDeltaTime * rotationSpeed;
            float rotationOffset = Mathf.Sin(rotationTime) * rotationAmount;
            targetRotation = originalRotation * Quaternion.Euler(0f, 0f, rotationOffset);
        }
        
        // Position sallanması
        if (enablePositionSway)
        {
            swayTime += Time.unscaledDeltaTime * swaySpeed;
            float swayOffsetX = Mathf.Sin(swayTime) * swayAmount;
            float swayOffsetY = Mathf.Sin(swayTime * 1.3f + 0.5f) * swayAmount * 0.5f;
            targetPosition = originalPosition + new Vector3(swayOffsetX, swayOffsetY, 0f);
        }
        
        // Değerleri uygula
        rectTransform.localScale = targetScale;
        rectTransform.localRotation = targetRotation;
        rectTransform.anchoredPosition = targetPosition;
    }

    #region Giriş Animasyonları

    private IEnumerator PlayEntranceAnimation()
    {
        isAnimating = true;
        
        switch (entranceType)
        {
            case EntranceType.BubblePop:
                yield return StartCoroutine(BubblePopEntrance());
                break;
            case EntranceType.FadeIn:
                yield return StartCoroutine(FadeInEntrance());
                break;
            case EntranceType.SlideUp:
                yield return StartCoroutine(SlideUpEntrance());
                break;
            case EntranceType.Expand:
                yield return StartCoroutine(ExpandEntrance());
                break;
        }
        
        isAnimating = false;
        entranceComplete = true;
    }

    private IEnumerator BubblePopEntrance()
    {
        // Başlangıçta küçük
        rectTransform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        
        // Overshoot ile büyü
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);
            float easedT = EaseOutElastic(t);
            
            rectTransform.localScale = Vector3.LerpUnclamped(Vector3.zero, originalScale, easedT);
            
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
    }

    private IEnumerator FadeInEntrance()
    {
        if (canvasGroup == null) yield break;
        
        canvasGroup.alpha = 0f;
        rectTransform.localScale = originalScale * 0.9f;
        
        float elapsed = 0f;
        
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);
            float easedT = EaseOutQuad(t);
            
            canvasGroup.alpha = easedT;
            rectTransform.localScale = Vector3.Lerp(originalScale * 0.9f, originalScale, easedT);
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        rectTransform.localScale = originalScale;
    }

    private IEnumerator SlideUpEntrance()
    {
        Vector3 startPos = originalPosition + new Vector3(0f, -100f, 0f);
        rectTransform.anchoredPosition = startPos;
        rectTransform.localScale = originalScale * 0.8f;
        
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);
            float easedT = EaseOutBack(t);
            
            rectTransform.anchoredPosition = Vector3.LerpUnclamped(startPos, originalPosition, easedT);
            rectTransform.localScale = Vector3.LerpUnclamped(originalScale * 0.8f, originalScale, easedT);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = EaseOutQuad(t);
            }
            
            yield return null;
        }
        
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = originalScale;
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    private IEnumerator ExpandEntrance()
    {
        rectTransform.localScale = new Vector3(originalScale.x, 0f, originalScale.z);
        
        float elapsed = 0f;
        
        while (elapsed < entranceDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / entranceDuration);
            float easedT = EaseOutElastic(t);
            
            rectTransform.localScale = new Vector3(
                originalScale.x,
                Mathf.LerpUnclamped(0f, originalScale.y, easedT),
                originalScale.z
            );
            
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
    }

    #endregion

    #region Pulse Efekti

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(pulseInterval);
            
            if (!isAnimating && entranceComplete)
            {
                yield return StartCoroutine(SinglePulse());
            }
        }
    }

    private IEnumerator SinglePulse()
    {
        Vector3 startScale = rectTransform.localScale;
        Vector3 pulseTargetScale = originalScale * pulseScale;
        
        float elapsed = 0f;
        float halfDuration = pulseDuration * 0.5f;
        
        // Büyü
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            
            rectTransform.localScale = Vector3.Lerp(startScale, pulseTargetScale, EaseOutQuad(t));
            
            yield return null;
        }
        
        // Küçül
        elapsed = 0f;
        startScale = rectTransform.localScale;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            
            rectTransform.localScale = Vector3.Lerp(startScale, originalScale, EaseOutQuad(t));
            
            yield return null;
        }
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

    #endregion

    #region Public Metodlar

    /// <summary>
    /// Dalgalanma efektini aç/kapa
    /// </summary>
    public void SetWobbleEnabled(bool enabled)
    {
        enableWobble = enabled;
        if (!enabled)
        {
            rectTransform.localScale = originalScale;
        }
    }

    /// <summary>
    /// Panel'e "hit" efekti ver (tıklama veya etkileşim için)
    /// </summary>
    public void TriggerHitEffect()
    {
        StartCoroutine(HitEffectCoroutine());
    }

    private IEnumerator HitEffectCoroutine()
    {
        Vector3 startScale = rectTransform.localScale;
        Vector3 squeezeScale = originalScale * 0.95f;
        
        // Squeeze
        float elapsed = 0f;
        while (elapsed < 0.08f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.08f);
            rectTransform.localScale = Vector3.Lerp(startScale, squeezeScale, t);
            yield return null;
        }
        
        // Bounce back
        elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.2f);
            rectTransform.localScale = Vector3.LerpUnclamped(squeezeScale, originalScale, EaseOutElastic(t));
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
    }

    /// <summary>
    /// Giriş animasyonunu tekrar oynat
    /// </summary>
    public void ReplayEntranceAnimation()
    {
        if (enableEntranceAnimation && entranceType != EntranceType.None)
        {
            entranceComplete = false;
            StartCoroutine(PlayEntranceAnimation());
        }
    }

    /// <summary>
    /// Çıkış animasyonu oynat ve callback çağır
    /// </summary>
    public void PlayExitAnimation(System.Action onComplete = null)
    {
        StartCoroutine(ExitAnimationCoroutine(onComplete));
    }

    private IEnumerator ExitAnimationCoroutine(System.Action onComplete)
    {
        isAnimating = true;
        
        Vector3 startScale = rectTransform.localScale;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        
        float elapsed = 0f;
        float duration = entranceDuration * 0.7f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = EaseInBack(t);
            
            rectTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, easedT);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            }
            
            yield return null;
        }
        
        isAnimating = false;
        onComplete?.Invoke();
    }

    private float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }

    #endregion
}
