using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Destructible mesh (MRUK) / passthrough içindeyken UI'ın gizlenmesini önler.
/// Ana kamerayı UI layer'dan çıkarır, sadece UI render eden overlay kamera ekler.
/// Silah içi canvas'lar (circle UI, energy bar vb.) runtime'da oluşturulsa bile görünür.
/// </summary>
[DefaultExecutionOrder(-100)]
public class UICameraStackSetup : MonoBehaviour
{
    private const int UILayer = 5;

    public static UICameraStackSetup Instance { get; private set; }
    public Camera UIOverlayCamera => _uiOverlayCamera;

    [Tooltip("Boş bırakılırsa OVRCameraRig'den otomatik bulunur")]
    [SerializeField] private Transform cameraRigTransform;

    private Camera _mainCamera;
    private Camera _uiOverlayCamera;
    private Transform _centerEyeAnchor;
    private Transform _ovrCameraRigTransform;
    private int _originalCullingMask;
    private readonly HashSet<Canvas> _registeredCanvases = new HashSet<Canvas>();

    private void Awake()
    {
        Instance = this;
        SetupUICamera();
        StartCoroutine(DiscoverCanvasesRoutine());
    }

    private void OnDestroy()
    {
        Instance = null;
        if (_mainCamera != null)
            _mainCamera.cullingMask = _originalCullingMask;
        if (_uiOverlayCamera != null)
            Destroy(_uiOverlayCamera.gameObject);
    }

    /// <summary>
    /// Runtime'da oluşturulan World Space canvas'ı passthrough'ta görünür yap.
    /// Silah script'leri (FifthGunLaser, SixthGunLaser, LastGunFlameSpray vb.) canvas oluşturduktan sonra çağırabilir.
    /// </summary>
    public void RegisterWorldSpaceCanvas(Canvas canvas)
    {
        if (canvas == null || _uiOverlayCamera == null) return;
        if (_registeredCanvases.Contains(canvas)) return;

        ApplyCanvasSetup(canvas);
        _registeredCanvases.Add(canvas);
    }

    private IEnumerator DiscoverCanvasesRoutine()
    {
        if (_uiOverlayCamera == null) yield break;

        float elapsed = 0f;
        while (true)
        {
            yield return new WaitForSeconds(elapsed < 10f ? 0.5f : 2f);
            elapsed += elapsed < 10f ? 0.5f : 2f;

            foreach (var canvas in FindObjectsOfType<Canvas>())
            {
                if (!_registeredCanvases.Contains(canvas) && ShouldApplyCanvasSetup(canvas))
                {
                    ApplyCanvasSetup(canvas);
                    _registeredCanvases.Add(canvas);
                }
            }
        }
    }

    /// <summary>
    /// Sadece silah canvas'ları (ammoText dahil), CenterEyeAnchor içindekiler ve Leaderboard'a setup uygula.
    /// NFT canvas'ları hariç tutulur.
    /// </summary>
    private bool ShouldApplyCanvasSetup(Canvas canvas)
    {
        if (canvas == null) return false;

        // NFT hariç - Nft Area, NFT Gallery vb. altındakiler
        if (IsUnderNftHierarchy(canvas.transform)) return false;

        // OVRCameraRig altındaki canvas'lar: CenterEyeAnchor (HUD) + el tutulan silahlar (ammoText dahil)
        if (_ovrCameraRigTransform != null && IsDescendantOf(canvas.transform, _ovrCameraRigTransform))
            return true;

        // Silah canvas'ı (GunFire ammoText vb.) - prefab instantiate edildiğinde henüz rig altında olmayabilir
        if (IsWeaponCanvas(canvas)) return true;

        // Leaderboard
        if (IsLeaderboardCanvas(canvas)) return true;

        return false;
    }

    private static bool IsWeaponCanvas(Canvas canvas)
    {
        return canvas.GetComponentInParent<GunFire>() != null;
    }

    private static bool IsUnderNftHierarchy(Transform t)
    {
        Transform p = t;
        while (p != null)
        {
            string name = p.name;
            if (name.Contains("NFT") || name.Contains("Nft") || name.Contains("NftFrame"))
                return true;
            p = p.parent;
        }
        return false;
    }

    private static bool IsDescendantOf(Transform t, Transform ancestor)
    {
        while (t != null)
        {
            if (t == ancestor) return true;
            t = t.parent;
        }
        return false;
    }

    private static bool IsLeaderboardCanvas(Canvas canvas)
    {
        var leaderboardUI = Object.FindObjectOfType<LeaderboardUI>();
        if (leaderboardUI == null || leaderboardUI.leaderboardPanel == null) return false;
        var rootCanvas = leaderboardUI.leaderboardPanel.GetComponentInParent<Canvas>();
        return rootCanvas == canvas;
    }

    private void ApplyCanvasSetup(Canvas canvas)
    {
        if (canvas == null || _uiOverlayCamera == null) return;

        canvas.worldCamera = _uiOverlayCamera;
        SetLayerRecursively(canvas.gameObject, UILayer);
    }

    private static void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private void SetupUICamera()
    {
        // Ana kamerayı bul (CenterEyeAnchor veya Camera.main)
        Transform eyeAnchor = FindCameraRigCenterEye();
        if (eyeAnchor == null)
        {
            Debug.LogWarning("[UICameraStackSetup] OVRCameraRig/CenterEyeAnchor bulunamadı, Camera.main kullanılıyor.");
            _mainCamera = Camera.main;
        }
        else
        {
            _mainCamera = eyeAnchor.GetComponent<Camera>();
            if (_mainCamera == null)
                _mainCamera = eyeAnchor.GetComponentInChildren<Camera>();
        }

        if (_mainCamera == null)
        {
            Debug.LogError("[UICameraStackSetup] Ana kamera bulunamadı!");
            return;
        }

        _centerEyeAnchor = eyeAnchor != null ? eyeAnchor : _mainCamera.transform;

        var ovrRig = FindObjectOfType<OVRCameraRig>();
        _ovrCameraRigTransform = ovrRig != null ? ovrRig.transform : null;

        // Ana kameranın culling mask'ından UI'ı çıkar
        _originalCullingMask = _mainCamera.cullingMask;
        _mainCamera.cullingMask &= ~(1 << UILayer);

        // UI Overlay kamera oluştur
        CreateUIOverlayCamera(eyeAnchor != null ? eyeAnchor : _mainCamera.transform);
    }

    private Transform FindCameraRigCenterEye()
    {
        if (cameraRigTransform != null)
        {
            // TrackingSpace veya OVRCameraRig - CenterEyeAnchor'ı bul
            Transform centerEye = cameraRigTransform.Find("TrackingSpace/CenterEyeAnchor");
            if (centerEye == null)
                centerEye = cameraRigTransform.Find("CenterEyeAnchor");
            if (centerEye == null)
                centerEye = cameraRigTransform;
            return centerEye;
        }

        var ovrRig = FindObjectOfType<OVRCameraRig>();
        if (ovrRig != null && ovrRig.centerEyeAnchor != null)
            return ovrRig.centerEyeAnchor;

        return null;
    }

    private void CreateUIOverlayCamera(Transform parent)
    {
        GameObject uiCamGo = new GameObject("UIOverlayCamera");
        uiCamGo.transform.SetParent(parent);
        uiCamGo.transform.localPosition = Vector3.zero;
        uiCamGo.transform.localRotation = Quaternion.identity;
        uiCamGo.transform.localScale = Vector3.one;

        _uiOverlayCamera = uiCamGo.AddComponent<Camera>();
        _uiOverlayCamera.CopyFrom(_mainCamera);
        _uiOverlayCamera.cullingMask = 1 << UILayer; // Sadece UI
        _uiOverlayCamera.clearFlags = CameraClearFlags.Depth;
        _uiOverlayCamera.depth = _mainCamera.depth + 1;
        _uiOverlayCamera.useOcclusionCulling = false;

        // URP Overlay olarak stack'e ekle
        var mainCamData = _mainCamera.GetUniversalAdditionalCameraData();
        if (mainCamData != null && mainCamData.renderType != CameraRenderType.Overlay)
        {
            var uiCamData = _uiOverlayCamera.GetUniversalAdditionalCameraData();
            if (uiCamData != null)
            {
                uiCamData.renderType = CameraRenderType.Overlay;
                if (!mainCamData.cameraStack.Contains(_uiOverlayCamera))
                    mainCamData.cameraStack.Add(_uiOverlayCamera);
            }
        }
        else
        {
            // URP yoksa normal second camera olarak çalışır
            _uiOverlayCamera.depth = _mainCamera.depth + 1;
        }

        // Sadece silah, CenterEyeAnchor ve Leaderboard canvas'ları kaydet (NFT hariç)
        foreach (var canvas in FindObjectsOfType<Canvas>())
        {
            if (ShouldApplyCanvasSetup(canvas))
            {
                ApplyCanvasSetup(canvas);
                _registeredCanvases.Add(canvas);
            }
        }

        Debug.Log("[UICameraStackSetup] UI Overlay Camera kuruldu. Sadece silah, CenterEyeAnchor ve Leaderboard canvas'ları passthrough'ta görünecek.");
    }
}
