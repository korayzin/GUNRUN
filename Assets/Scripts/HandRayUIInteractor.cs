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
    [Tooltip("Ekran koordinatı offset (piksel) - gerekirse ince ayar için")]
    public Vector2 screenOffset = Vector2.zero;
    
    [Tooltip("Ray rengi (normal)")]
    public Color rayColor = new Color(0f, 1f, 1f, 0.8f); // Cyan
    
    [Tooltip("Ray rengi (hover durumunda)")]
    public Color rayHoverColor = new Color(0f, 1f, 0f, 1f); // Yeşil
    
    [Tooltip("Ray kalınlığı")]
    public float rayWidth = 0.01f;

    [Header("Hover Efekti")]
    [Tooltip("Hover durumunda buton scale çarpanı")]
    public float hoverScaleMultiplier = 1.1f;
    
    [Tooltip("Hover çıkışı için gereken ardışık 'isabet yok' frame sayısı (VR jitter önleme)")]
    [Range(1, 8)]
    public int hoverExitDelayFrames = 3;

    [Header("Başlangıç")]
    [Tooltip("UI menüsü için ray baştan açık olsun")]
    public bool startWithRayEnabled = false;

    // Private değişkenler
    private LineRenderer lineRenderer;
    private GraphicRaycaster graphicRaycaster;
    private EventSystem eventSystem;
    private PointerEventData pointerEventData;
    private List<RaycastResult> raycastResults = new List<RaycastResult>();
    
    private Transform handAnchor;
    private Button currentHoveredButton;
    private Slider currentHoveredSlider;
    private Vector3 originalButtonScale;
    private bool isRayEnabled = false;
    private int hoverMissFrameCount;
    private Vector2 currentScreenPoint;
    private bool isDraggingSlider;
    private Slider draggedSlider;

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
            if (targetCanvas.renderMode == RenderMode.WorldSpace && targetCanvas.worldCamera == null && Camera.main != null)
                targetCanvas.worldCamera = Camera.main;
            graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        // Başlangıçta ray durumu
        if (startWithRayEnabled)
        {
            EnableRay();
        }
        else
        {
            DisableRay();
        }
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
        Vector3 handForward = handAnchor.forward;

        // Ray canvas düzleminde kesilsin, panelin içinden geçip arkaya gitmesin (titreme önlenir)
        Vector3 endPos = GetRayCanvasIntersection(startPos, handForward);

        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);
    }

    /// <summary>
    /// El ray'inin canvas düzlemiyle kesişim noktasını döndürür.
    /// </summary>
    private Vector3 GetRayCanvasIntersection(Vector3 rayOrigin, Vector3 rayDir)
    {
        if (targetCanvas == null) return rayOrigin + rayDir * rayLength;

        Transform canvasTransform = targetCanvas.transform;
        Vector3 planePoint = canvasTransform.position;
        Vector3 planeNormal = canvasTransform.forward;

        float denom = Vector3.Dot(planeNormal, rayDir);
        if (Mathf.Abs(denom) < 0.0001f)
            return rayOrigin + rayDir * rayLength;

        float t = Vector3.Dot(planePoint - rayOrigin, planeNormal) / denom;
        if (t < 0) return rayOrigin + rayDir * rayLength;

        return rayOrigin + rayDir * Mathf.Min(t, rayLength);
    }

    private void PerformUIRaycast()
    {
        if (eventSystem == null) return;

        Vector3 handPos = handAnchor.position;
        Vector3 handForward = handAnchor.forward;
        Vector3 worldPoint = GetRayCanvasIntersection(handPos, handForward);

        // GraphicRaycaster canvas.worldCamera kullanır - aynı kamerayı kullanmalıyız
        Camera raycastCam = targetCanvas != null && targetCanvas.worldCamera != null
            ? targetCanvas.worldCamera : Camera.main;
        if (raycastCam == null) return;

        Vector3 sp = raycastCam.WorldToScreenPoint(worldPoint);
        Vector2 screenPoint = new Vector2(sp.x + screenOffset.x, sp.y + screenOffset.y);
        currentScreenPoint = screenPoint;

        if (pointerEventData == null) pointerEventData = new PointerEventData(eventSystem);
        pointerEventData.position = screenPoint;
        raycastResults.Clear();
        graphicRaycaster.Raycast(pointerEventData, raycastResults);

        Button hitButton = null;
        Slider hitSlider = null;
        if (raycastResults.Count > 0)
        {
            var first = raycastResults[0].gameObject;
            hitButton = first.GetComponent<Button>() ?? first.GetComponentInParent<Button>();
            hitSlider = first.GetComponent<Slider>() ?? first.GetComponentInParent<Slider>();
        }
        if (hitButton == null) hitButton = FindButtonViaRectTransform(screenPoint, raycastCam);
        if (hitSlider == null) hitSlider = FindSliderViaRectTransform(screenPoint, raycastCam);

        if (isDraggingSlider)
        {
            hoverMissFrameCount = 0;
            return;
        }

        if (hitSlider != null && hitSlider.interactable && hitSlider.gameObject.activeInHierarchy)
        {
            hoverMissFrameCount = 0;
            if (hitSlider != currentHoveredSlider)
            {
                ClearHoverEffect();
                currentHoveredSlider = hitSlider;
                lineRenderer.startColor = rayHoverColor;
                lineRenderer.endColor = rayHoverColor;
            }
        }
        else if (hitButton != null)
        {
            hoverMissFrameCount = 0;
            if (hitButton != currentHoveredButton)
            {
                ClearHoverEffect();
                currentHoveredButton = hitButton;
                var mapHover = currentHoveredButton.GetComponent<MapSelectionButtonHover>();
                if (mapHover != null)
                    mapHover.OnHoverEnter();
                else
                {
                    var animator = currentHoveredButton.GetComponent<ButtonRayAnimator>();
                    if (animator != null)
                        animator.OnHoverEnter();
                    else
                    {
                        originalButtonScale = currentHoveredButton.transform.localScale;
                        currentHoveredButton.transform.localScale = originalButtonScale * hoverScaleMultiplier;
                    }
                }
                lineRenderer.startColor = rayHoverColor;
                lineRenderer.endColor = rayHoverColor;
            }
        }
        else if (currentHoveredButton != null || currentHoveredSlider != null)
        {
            hoverMissFrameCount++;
            if (hoverMissFrameCount >= hoverExitDelayFrames && !isDraggingSlider)
            {
                ClearHoverEffect();
                hoverMissFrameCount = 0;
            }
        }
    }

    /// <summary>
    /// GraphicRaycaster başarısız olursa RectTransform ile doğrudan buton kontrolü.
    /// </summary>
    private Button FindButtonViaRectTransform(Vector2 screenPoint, Camera cam)
    {
        if (targetCanvas == null || cam == null) return null;

        foreach (var btn in targetCanvas.GetComponentsInChildren<Button>(true))
        {
            if (!btn.interactable || !btn.gameObject.activeInHierarchy) continue;

            RectTransform rect = btn.GetComponent<RectTransform>();
            if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam))
                return btn;
        }
        return null;
    }

    private Slider FindSliderViaRectTransform(Vector2 screenPoint, Camera cam)
    {
        if (targetCanvas == null || cam == null) return null;

        foreach (var s in targetCanvas.GetComponentsInChildren<Slider>(true))
        {
            if (!s.interactable || !s.gameObject.activeInHierarchy) continue;

            RectTransform rect = s.GetComponent<RectTransform>();
            if (rect != null && RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam))
                return s;
        }
        return null;
    }

    /// <summary>
    /// Ray pozisyonuna göre slider değerini doğrudan günceller. VR için ExecuteEvents yerine daha güvenilir.
    /// </summary>
    private void UpdateSliderValueFromRay(Slider slider)
    {
        if (slider == null) return;

        Canvas rootCanvas = slider.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (rootCanvas != null)
        {
            if (rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                cam = null;
            else
                cam = rootCanvas.worldCamera != null ? rootCanvas.worldCamera : Camera.main;
        }
        if (cam == null && rootCanvas == null) cam = Camera.main;

        RectTransform trackRect = null;
        if (slider.fillRect != null && slider.fillRect.parent != null)
            trackRect = slider.fillRect.parent as RectTransform;
        if (trackRect == null)
            trackRect = slider.GetComponent<RectTransform>();
        if (trackRect == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(trackRect, currentScreenPoint, cam, out localPoint);

        Rect r = trackRect.rect;
        float normalized;

        switch (slider.direction)
        {
            case Slider.Direction.LeftToRight:
            case Slider.Direction.RightToLeft:
                if (Mathf.Abs(r.width) < 0.001f) return;
                normalized = (slider.direction == Slider.Direction.LeftToRight)
                    ? (localPoint.x - r.xMin) / r.width
                    : (r.xMax - localPoint.x) / r.width;
                break;
            case Slider.Direction.BottomToTop:
            case Slider.Direction.TopToBottom:
                if (Mathf.Abs(r.height) < 0.001f) return;
                normalized = (slider.direction == Slider.Direction.BottomToTop)
                    ? (localPoint.y - r.yMin) / r.height
                    : (r.yMax - localPoint.y) / r.height;
                break;
            default:
                if (Mathf.Abs(r.width) < 0.001f) return;
                normalized = (localPoint.x - r.xMin) / r.width;
                break;
        }

        float value = Mathf.Lerp(slider.minValue, slider.maxValue, Mathf.Clamp01(normalized));
        if (slider.wholeNumbers)
            value = Mathf.Round(value);
        slider.value = value;
    }

    private void HandleInput()
    {
        bool triggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                           OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
        bool triggerUp = OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger) ||
                         OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger);

        if (isDraggingSlider)
        {
            if (triggerUp || draggedSlider == null)
            {
                if (triggerUp && draggedSlider != null && draggedSlider.gameObject.name == "SoundSlider" && MusicManager.Instance != null)
                    MusicManager.Instance.PlaySfxPreviewOnRelease();
                isDraggingSlider = false;
                draggedSlider = null;
            }
            else
            {
                UpdateSliderValueFromRay(draggedSlider);
            }
            return;
        }

        if (triggerDown && currentHoveredSlider != null)
        {
            draggedSlider = currentHoveredSlider;
            isDraggingSlider = true;
            UpdateSliderValueFromRay(draggedSlider);
            return;
        }

        if (triggerDown && currentHoveredButton != null)
        {
            Debug.Log($"[HandRayUIInteractor] Buton tıklandı: {currentHoveredButton.gameObject.name}");
            
            if (currentHoveredButton.GetComponent<MapSelectionButtonHover>() == null)
            {
                var animator = currentHoveredButton.GetComponent<ButtonRayAnimator>();
                if (animator != null)
                    animator.OnPressed();
            }
            
            currentHoveredButton.onClick.Invoke();
        }
    }

    private void ClearHoverEffect()
    {
        hoverMissFrameCount = 0;
        if (currentHoveredButton != null)
        {
            var mapHover = currentHoveredButton.GetComponent<MapSelectionButtonHover>();
            if (mapHover != null)
                mapHover.OnHoverExit();
            else
            {
                var animator = currentHoveredButton.GetComponent<ButtonRayAnimator>();
                if (animator != null)
                    animator.OnHoverExit();
                else
                    currentHoveredButton.transform.localScale = originalButtonScale;
            }
            currentHoveredButton = null;
        }
        if (currentHoveredSlider != null && !isDraggingSlider)
        {
            currentHoveredSlider = null;
        }
        
        if (lineRenderer != null)
        {
            lineRenderer.startColor = rayColor;
            lineRenderer.endColor = rayColor;
        }
    }

    /// <summary>
    /// Ray'i etkinleştir (Game Over durumunda çağrılır)
    /// Retry sonrası ikinci kez açıldığında referanslar yenilenir - ray'in çalışması için kritik.
    /// </summary>
    public void EnableRay()
    {
        // Retry sonrası sahne yeniden yüklendiğinde referanslar stale olabilir - hepsini yenile
        RefreshReferences();
        
        isRayEnabled = true;
        
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }

        // Canvas referansını güncelle (dinamik olarak atanmış olabilir)
        if (targetCanvas != null && graphicRaycaster == null)
        {
            graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
                graphicRaycaster = targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        Debug.Log("[HandRayUIInteractor] Ray etkinleştirildi");
    }
    
    /// <summary>
    /// Tüm referansları yeniden alır. Retry sonrası ikinci Game Over'da ray çalışması için gerekli.
    /// </summary>
    private void RefreshReferences()
    {
        if (ovrCameraRig == null)
            ovrCameraRig = FindObjectOfType<OVRCameraRig>();
        
        if (ovrCameraRig != null)
        {
            handAnchor = useLeftHand 
                ? ovrCameraRig.leftHandOnControllerAnchor 
                : ovrCameraRig.rightHandOnControllerAnchor;
        }

        if (eventSystem == null)
        {
            eventSystem = EventSystem.current;
            if (eventSystem == null)
                eventSystem = FindObjectOfType<EventSystem>();
        }

        if (targetCanvas != null)
        {
            if (targetCanvas.renderMode == RenderMode.WorldSpace && targetCanvas.worldCamera == null && Camera.main != null)
                targetCanvas.worldCamera = Camera.main;
            if (graphicRaycaster == null)
            {
                graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();
                if (graphicRaycaster == null)
                    graphicRaycaster = targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
    }

    /// <summary>
    /// Ray'i devre dışı bırak (Oyun yeniden başladığında çağrılır)
    /// </summary>
    public void DisableRay()
    {
        isRayEnabled = false;
        isDraggingSlider = false;
        draggedSlider = null;
        
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
