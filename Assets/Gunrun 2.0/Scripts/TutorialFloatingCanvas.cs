using UnityEngine;
using TMPro;

/// <summary>
/// Oyuncunun önünde açılan World-Space canvas. Tutorial text gösterimi için.
/// </summary>
public class TutorialFloatingCanvas : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Boşsa OVRCameraRig CenterEyeAnchor veya Camera.main kullanılır")]
    public Transform cameraAnchor;
    [Tooltip("Text gösterilecek alan")]
    public TextMeshProUGUI textUI;

    [Header("Konum")]
    [Tooltip("Kameradan önde mesafe (metre)")]
    public float distanceFromCamera = 1.2f;

    private Canvas _canvas;
    private bool _visible;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        if (_canvas == null) _canvas = GetComponentInChildren<Canvas>();
        if (textUI == null) textUI = GetComponentInChildren<TextMeshProUGUI>(true);
        ResolveCameraAnchor();
    }

    private void LateUpdate()
    {
        if (!_visible || cameraAnchor == null) return;
        PositionInFrontOfCamera();
    }

    private void ResolveCameraAnchor()
    {
        if (cameraAnchor != null) return;
        var ovrRig = FindObjectOfType<OVRCameraRig>();
        if (ovrRig != null)
        {
            var centerEye = ovrRig.transform.Find("TrackingSpace/CenterEyeAnchor");
            if (centerEye == null) centerEye = ovrRig.transform.Find("CenterEyeAnchor");
            if (centerEye != null) cameraAnchor = centerEye;
        }
        if (cameraAnchor == null && Camera.main != null)
            cameraAnchor = Camera.main.transform;
    }

    private void PositionInFrontOfCamera()
    {
        if (cameraAnchor == null) return;
        transform.position = cameraAnchor.position + cameraAnchor.forward * distanceFromCamera;
        transform.rotation = Quaternion.LookRotation(transform.position - cameraAnchor.position);
    }

    /// <summary>Metni göster ve canvas'ı kameranın önünde konumlandır.</summary>
    public void Show(string text)
    {
        _visible = true;
        gameObject.SetActive(true);
        if (textUI != null) textUI.text = text ?? "";
        ResolveCameraAnchor();
        if (cameraAnchor != null)
            PositionInFrontOfCamera();
    }

    /// <summary>Canvas'ı gizle.</summary>
    public void Hide()
    {
        _visible = false;
        gameObject.SetActive(false);
    }
}
