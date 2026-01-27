using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// VR el ray etkileşim sistemi - Screen Space Canvas UI butonları için
/// Time.timeScale = 0 durumunda da çalışır (Retry menüsü için)
/// </summary>
public class HandRayUIInteractor : MonoBehaviour
{
    [Header("VR Referansları")]
    [Tooltip("OVRCameraRig referansı - otomatik bulunur eğer atanmazsa")]
    public OVRCameraRig ovrCameraRig;
    
    [Tooltip("Hangi el kullanılacak")]
    public bool useLeftHand = false;

    [Header("Canvas Referansı")]
    [Tooltip("Etkileşim yapılacak Canvas (GraphicRaycaster olmalı)")]
    public Canvas targetCanvas;
    
    [Header("Ray Ayarları")]
    [Tooltip("Ray'in maksimum uzunluğu")]
    public float rayLength = 10f;
    
    [Tooltip("Ray rengi (normal)")]
    public Color rayColor = new Color(0f, 1f, 1f, 0.8f); // Cyan
    
    [Tooltip("Ray rengi (hover durumunda)")]
    public Color rayHoverColor = new Color(0f, 1f, 0f, 1f); // Yeşil
    
    [Tooltip("Ray kalınlığı")]
    public float rayWidth = 0.01f;

    [Header("Hover Efekti")]
    [Tooltip("Hover durumunda buton scale çarpanı")]
    public float hoverScaleMultiplier = 1.1f;

    // Private değişkenler
    private LineRenderer lineRenderer;
    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;
    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    
    private Transform handAnchor;
    private Button currentHoveredButton;
    private Vector3 originalButtonScale;
    private bool isRayEnabled = false;

    private void Awake()
    {
        SetupLineRenderer();
    }

    private void Start()
    {
        // OVRCameraRig'i bul
        if (ovrCameraRig == null)
        {
            ovrCameraRig = FindObjectOfType<OVRCameraRig>();
        }

        if (ovrCameraRig != null)
        {
            handAnchor = useLeftHand 
                ? ovrCameraRig.leftHandOnControllerAnchor 
                : ovrCameraRig.rightHandOnControllerAnchor;
        }
        else
        {
            Debug.LogError("[HandRayUIInteractor] OVRCameraRig bulunamadı!");
        }

        // EventSystem'i bul
        eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            eventSystem = FindObjectOfType<EventSystem>();
        }

        // GraphicRaycaster'ı bul
        if (targetCanvas != null)
        {
            graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                Debug.LogWarning("[HandRayUIInteractor] Target Canvas'ta GraphicRaycaster bulunamadı, ekleniyor...");
                graphicRaycaster = targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // Başlangıçta ray kapalı
        DisableRay();
    }

    private void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth * 0.5f;
        lineRenderer.useWorldSpace = true;
        
        // Basit materyal oluştur
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = rayColor;
        lineRenderer.endColor = rayColor;
        
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (!isRayEnabled) return;
        if (handAnchor == null) return;
        if (graphicRaycaster == null) return;

        UpdateRayVisual();
        PerformUIRaycast();
        HandleInput();
    }

    private void UpdateRayVisual()
    {
        Vector3 startPos = handAnchor.position;
        Vector3 endPos = startPos + handAnchor.forward * rayLength;

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    private void PerformUIRaycast()
    {
        if (eventSystem == null) return;

        // El pozisyonundan ekran koordinatı hesapla
        Vector3 handPos = handAnchor.position;
        Vector3 handForward = handAnchor.forward;
        
        // Ray'in bir noktasını ekran koordinatına çevir
        Vector3 worldPoint = handPos + handForward * rayLength;
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(worldPoint);

        // PointerEventData oluştur
        if (pointerEventData == null)
        {
            pointerEventData = new PointerEventData(eventSystem);
        }
        pointerEventData.position = new Vector2(screenPoint.x, screenPoint.y);

        // Raycast yap
        raycastResults.Clear();
        graphicRaycaster.Raycast(pointerEventData, raycastResults);

        // Sonuçları işle
        if (raycastResults.Count > 0)
        {
            // İlk sonuçta buton var mı kontrol et
            Button hitButton = raycastResults[0].gameObject.GetComponent<Button>();
            
            // Eğer buton yoksa parent'larda ara
            if (hitButton == null)
            {
                hitButton = raycastResults[0].gameObject.GetComponentInParent<Button>();
            }

            if (hitButton != null && hitButton != currentHoveredButton)
            {
                // Önceki hover'ı temizle
                ClearHoverEffect();
                
                // Yeni hover efekti uygula
                currentHoveredButton = hitButton;
                originalButtonScale = currentHoveredButton.transform.localScale;
                currentHoveredButton.transform.localScale = originalButtonScale * hoverScaleMultiplier;
                
                // Ray rengini değiştir
                lineRenderer.startColor = rayHoverColor;
                lineRenderer.endColor = rayHoverColor;
            }
            else if (hitButton == null)
            {
                ClearHoverEffect();
            }
        }
        else
        {
            ClearHoverEffect();
        }
    }

    private void HandleInput()
    {
        // Tetik kontrolü - her iki el için de kontrol et
        bool triggerPressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || 
                              OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);

        if (triggerPressed && currentHoveredButton != null)
        {
            Debug.Log($"[HandRayUIInteractor] Buton tıklandı: {currentHoveredButton.gameObject.name}");
            
            // Butonu tıkla
            currentHoveredButton.onClick.Invoke();
        }
    }

    private void ClearHoverEffect()
    {
        if (currentHoveredButton != null)
        {
            currentHoveredButton.transform.localScale = originalButtonScale;
            currentHoveredButton = null;
        }
        
        // Ray rengini normale döndür
        lineRenderer.startColor = rayColor;
        lineRenderer.endColor = rayColor;
    }

    /// <summary>
    /// Ray'i etkinleştir (Game Over durumunda çağrılır)
    /// </summary>
    public void EnableRay()
    {
        isRayEnabled = true;
        
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }

        // Canvas referansını güncelle (dinamik olarak atanmış olabilir)
        if (targetCanvas != null && graphicRaycaster == null)
        {
            graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();
        }

        Debug.Log("[HandRayUIInteractor] Ray etkinleştirildi");
    }

    /// <summary>
    /// Ray'i devre dışı bırak (Oyun yeniden başladığında çağrılır)
    /// </summary>
    public void DisableRay()
    {
        isRayEnabled = false;
        
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }

        ClearHoverEffect();
        
        Debug.Log("[HandRayUIInteractor] Ray devre dışı bırakıldı");
    }

    /// <summary>
    /// Target Canvas'ı runtime'da ayarla
    /// </summary>
    public void SetTargetCanvas(Canvas canvas)
    {
        targetCanvas = canvas;
        if (canvas != null)
        {
            graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }
}
