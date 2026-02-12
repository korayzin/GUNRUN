using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// UserName sahnesi için tek script: Kontrolcüden ray atar, UI butonuna çarparsa tetikleyici ile tıklar.
/// Bu script'i sahneye herhangi bir GameObject'e eklemen yeterli; Canvas ve OVR otomatik bulunur.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class UserNameRayClick : MonoBehaviour
{
    [Header("Boş bırakırsan otomatik bulunur")]
    public Canvas canvas;
    public OVRCameraRig ovrRig;
    [Tooltip("true = sol el, false = sağ el")]
    public bool useLeftHand;

    [Header("Ray")]
    public float rayLength = 5f;
    public float lineWidth = 0.008f;

    private Transform _hand;
    private GraphicRaycaster _raycaster;
    private EventSystem _eventSystem;
    private LineRenderer _line;
    private PointerEventData _pointerData;
    private List<RaycastResult> _results = new List<RaycastResult>();
    private Camera _cam;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.positionCount = 2;
        _line.useWorldSpace = true;
        _line.startWidth = lineWidth;
        _line.endWidth = lineWidth * 0.5f;
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.startColor = new Color(0.2f, 0.8f, 1f, 0.9f);
        _line.endColor = new Color(0.2f, 0.8f, 1f, 0.4f);
    }

    private void Start()
    {
        if (ovrRig == null) ovrRig = FindObjectOfType<OVRCameraRig>();
        if (ovrRig == null)
        {
            Debug.LogError("[UserNameRayClick] OVRCameraRig yok! Sahneye OVR Camera Rig ekle.");
            enabled = false;
            return;
        }

        _hand = useLeftHand ? ovrRig.leftHandOnControllerAnchor : ovrRig.rightHandOnControllerAnchor;
        if (_hand == null)
        {
            Debug.LogError("[UserNameRayClick] Hand anchor bulunamadı.");
            enabled = false;
            return;
        }

        if (canvas == null)
        {
            var go = GameObject.Find("Canvas") ?? GameObject.Find("UserNameCanvas");
            if (go != null) canvas = go.GetComponent<Canvas>();
            if (canvas == null) canvas = FindObjectOfType<Canvas>();
        }
        if (canvas == null)
        {
            Debug.LogError("[UserNameRayClick] Canvas yok!");
            enabled = false;
            return;
        }

        _raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (_raycaster == null)
        {
            _raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
        }

        _eventSystem = EventSystem.current ?? FindObjectOfType<EventSystem>();
        _cam = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        if (_cam == null) _cam = Camera.main;
        if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null && _cam != null)
            canvas.worldCamera = _cam;
    }

    private void Update()
    {
        if (_hand == null || _raycaster == null || _cam == null) return;

        Vector3 origin = _hand.position;
        Vector3 dir = _hand.forward;
        Vector3 end = origin + dir * rayLength;

        _line.SetPosition(0, origin);
        _line.SetPosition(1, end);
        _line.enabled = true;

        Vector2 screenPoint = _cam.WorldToScreenPoint(end);
        if (_pointerData == null) _pointerData = new PointerEventData(_eventSystem);
        _pointerData.position = screenPoint;

        _results.Clear();
        _raycaster.Raycast(_pointerData, _results);

        bool trigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);

        if (_results.Count > 0 && trigger)
        {
            var go = _results[0].gameObject;
            var btn = go.GetComponent<Button>() ?? go.GetComponentInParent<Button>();
            if (btn != null && btn.interactable)
            {
                btn.onClick.Invoke();
            }
        }
    }
}
