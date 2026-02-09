using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UserName sahnesi: Sadece Physics.Raycast ile butonlara tıklanır (fare veya VR tetikleyici).
/// Örnek buton = isim yazar, Devam = onaylar ve sonraki sahneye geçer.
/// </summary>
public class UserNameController : MonoBehaviour
{
    public const string UserNameSetKey = "UserNameSet";
    public const string PlayerNameKey = "PlayerName";
    private const int MaxNameLength = 20;
    private const float RayDistance = 50f;

    [Header("Sonraki sahne")]
    [SerializeField] private string nextSceneName = "UI";

    [Header("İsim girişi")]
    [SerializeField] private TMP_InputField nameInputTMP;
    [SerializeField] private InputField nameInputLegacy;

    [Header("Butonlar")]
    [SerializeField] private Button sampleButton1;
    [SerializeField] private Button sampleButton2;
    [SerializeField] private Button sampleButton3;
    [SerializeField] private Button confirmButton;

    [Header("Örnek isimler (sırayla 1-2-3)")]
    [SerializeField] private string[] sampleNames = new string[] { "Sürat", "Nişancı", "Keskin" };

    [Header("Raycast - Elinden çıkan ray")]
    [Tooltip("Elimden çıkan rayin başlangıç noktası. OVR'da laser/pointer kullanan objeyi buraya sürükle. Boşsa otomatik RightHandAnchor aranır.")]
    [SerializeField] private Transform rayOrigin;
    [Tooltip("Ray atılacak katman (varsayılan = tümü)")]
    [SerializeField] private LayerMask raycastLayers = -1;

    private Camera _mainCam;

    private void Start()
    {
        if (PlayerPrefs.GetInt(UserNameSetKey, 0) == 1)
        {
            LoadNextScene();
            return;
        }

        if (sampleNames == null || sampleNames.Length < 3)
            sampleNames = new string[] { "Sürat", "Nişancı", "Keskin" };

        sampleButton1?.onClick.AddListener(OnSample1Click);
        sampleButton2?.onClick.AddListener(OnSample2Click);
        sampleButton3?.onClick.AddListener(OnSample3Click);
        confirmButton?.onClick.AddListener(OnConfirmClick);

        _mainCam = Camera.main;
        if (rayOrigin == null)
            rayOrigin = FindHandRayOrigin();
        if (rayOrigin == null)
            rayOrigin = _mainCam != null ? _mainCam.transform : transform;

        Canvas can = GetComponentInParent<Canvas>();
        if (can != null && can.renderMode == RenderMode.WorldSpace && _mainCam != null)
            can.worldCamera = _mainCam;
    }

    private Transform FindHandRayOrigin()
    {
        try
        {
            var ovrRig = Object.FindObjectOfType<OVRCameraRig>();
            if (ovrRig == null) return null;
            Transform t = ovrRig.transform.Find("RightControllerAnchor");
            if (t != null) return t;
            t = ovrRig.transform.Find("TrackingSpace/RightHandAnchor");
            if (t != null) return t;
            t = ovrRig.transform.Find("RightHandAnchor");
            if (t != null) return t;
            t = ovrRig.transform.Find("LeftControllerAnchor");
            if (t != null) return t;
            t = ovrRig.transform.Find("TrackingSpace/LeftHandAnchor");
            if (t != null) return t;
            foreach (Transform child in ovrRig.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.Contains("RightHand") || child.name.Contains("RightController"))
                    return child;
            }
        }
        catch (System.Exception) { }
        return null;
    }

    public void OnSample1Click() => SetSampleName(0);
    public void OnSample2Click() => SetSampleName(1);
    public void OnSample3Click() => SetSampleName(2);
    public void OnConfirmClick() => OnConfirm();

    private void Update()
    {
        bool trigger = false;

        if (Input.GetMouseButtonDown(0))
            trigger = true;

        if (!trigger && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            trigger = true;

        if (!trigger)
        {
            try
            {
                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
                    trigger = true;
            }
            catch (System.Exception) { }
        }

        if (!trigger) return;

        Ray ray;
        if (_mainCam != null && Input.GetMouseButtonDown(0))
            ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        else if (rayOrigin != null)
            ray = new Ray(rayOrigin.position, rayOrigin.forward);
        else if (_mainCam != null)
            ray = new Ray(_mainCam.transform.position, _mainCam.transform.forward);
        else
            return;

        if (!Physics.Raycast(ray, out RaycastHit hit, RayDistance, raycastLayers))
            return;

        UserNameButtonHit target = hit.collider.GetComponent<UserNameButtonHit>();
        if (target == null)
            target = hit.collider.GetComponentInParent<UserNameButtonHit>();

        if (target != null)
        {
            switch (target.action)
            {
                case UserNameButtonHit.Action.Sample1: SetSampleName(0); break;
                case UserNameButtonHit.Action.Sample2: SetSampleName(1); break;
                case UserNameButtonHit.Action.Sample3: SetSampleName(2); break;
                case UserNameButtonHit.Action.Confirm:  OnConfirm(); break;
            }
        }
    }

    private void SetSampleName(int index)
    {
        if (sampleNames == null || index < 0 || index >= sampleNames.Length) return;
        string name = sampleNames[index];
        if (nameInputTMP != null)
            nameInputTMP.text = name;
        else if (nameInputLegacy != null)
            nameInputLegacy.text = name;
    }

    private string GetInputText()
    {
        if (nameInputTMP != null && nameInputTMP.gameObject.activeInHierarchy)
            return nameInputTMP.text ?? "";
        if (nameInputLegacy != null && nameInputLegacy.gameObject.activeInHierarchy)
            return nameInputLegacy.text ?? "";
        return "";
    }

    private void OnConfirm()
    {
        string name = GetInputText().Trim();
        if (string.IsNullOrEmpty(name))
            name = sampleNames != null && sampleNames.Length > 0 ? sampleNames[0] : "Player";
        if (name.Length > MaxNameLength)
            name = name.Substring(0, MaxNameLength);

        PlayerPrefs.SetString(PlayerNameKey, name);
        PlayerPrefs.SetInt(UserNameSetKey, 1);
        PlayerPrefs.Save();

        if (FirebaseLeaderboardManager.Instance != null)
            FirebaseLeaderboardManager.Instance.SetPlayerName(name);

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
            nextSceneName = "UI";
        SceneManager.LoadScene(nextSceneName);
    }
}
