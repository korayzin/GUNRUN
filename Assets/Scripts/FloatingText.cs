using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    [Header("Animation Settings")]
    public float destroyTime = 1.5f;
    public float floatSpeed = 2f;
    public float floatDistance = 1.5f;
    
    [Header("Scale Animation")]
    public float scaleUpDuration = 0.2f;
    public float scaleDownDuration = 0.3f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Fade Animation")]
    public float fadeStartTime = 0.8f;
    public float fadeDuration = 0.7f;
    
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsedTime = 0f;
    private TextMeshPro textMesh3D;
    private TextMeshProUGUI textMeshUI;
    private Color originalColor;
    private Vector3 originalScale;

    private void Start()
    {
        // Hem 3D (TextMeshPro) hem UI (TextMeshProUGUI) versiyonunu ara
        textMesh3D = GetComponent<TextMeshPro>();
        textMeshUI = GetComponent<TextMeshProUGUI>();
        
        if (textMesh3D != null)
        {
            originalColor = textMesh3D.color;
        }
        else if (textMeshUI != null)
        {
            originalColor = textMeshUI.color;
        }
        
        originalScale = transform.localScale;
        startPosition = transform.position;
        targetPosition = startPosition + Vector3.up * floatDistance;
        
        // Baslangicta scale 0
        transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        
        // Scale animasyonu - bounce efekti
        if (elapsedTime < scaleUpDuration)
        {
            float scaleProgress = elapsedTime / scaleUpDuration;
            float scaleValue = scaleCurve.Evaluate(scaleProgress);
            // Bounce efekti için hafif overshoot
            scaleValue = Mathf.Sin(scaleValue * Mathf.PI) * 1.1f;
            scaleValue = Mathf.Clamp01(scaleValue);
            transform.localScale = originalScale * scaleValue;
        }
        else if (elapsedTime < scaleUpDuration + scaleDownDuration)
        {
            // Hafif scale down (normal boyuta dön)
            float scaleProgress = (elapsedTime - scaleUpDuration) / scaleDownDuration;
            float scaleValue = Mathf.Lerp(1.1f, 1f, scaleProgress);
            transform.localScale = originalScale * scaleValue;
        }
        
        // Yukarı doğru smooth hareket
        float moveProgress = Mathf.Clamp01(elapsedTime / destroyTime);
        // Ease out curve kullan
        float easedProgress = 1f - Mathf.Pow(1f - moveProgress, 3f);
        transform.position = Vector3.Lerp(startPosition, targetPosition, easedProgress);
        
        // Hafif yan yana sallanma (tatlı bir efekt)
        float swayAmount = Mathf.Sin(elapsedTime * 3f) * 0.1f;
        transform.position += transform.right * swayAmount;
        
        // Fade out animasyonu
        if (elapsedTime > fadeStartTime)
        {
            float fadeProgress = (elapsedTime - fadeStartTime) / fadeDuration;
            fadeProgress = Mathf.Clamp01(fadeProgress);
            Color currentColor = originalColor;
            currentColor.a = Mathf.Lerp(originalColor.a, 0f, fadeProgress);
            
            if (textMesh3D != null)
                textMesh3D.color = currentColor;
            else if (textMeshUI != null)
                textMeshUI.color = currentColor;
        }
        
        // Kamera'ya bak
        if (Camera.main != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;
            directionToCamera.y = 0; // Sadece Y ekseni etrafında döndür
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
        
        // Destroy
        if (elapsedTime >= destroyTime)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        // Henuz atanmamissa bul
        if (textMesh3D == null && textMeshUI == null)
        {
            textMesh3D = GetComponent<TextMeshPro>();
            textMeshUI = GetComponent<TextMeshProUGUI>();
        }
        
        // Hangisi varsa ona yaz
        if (textMesh3D != null)
        {
            textMesh3D.text = text;
        }
        else if (textMeshUI != null)
        {
            textMeshUI.text = text;
        }
        
        Debug.Log("FloatingText SetText: " + text);
    }
}

