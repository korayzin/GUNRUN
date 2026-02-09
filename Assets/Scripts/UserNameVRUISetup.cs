using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UserName sahnesinde VR kontrolcü ile Canvas etkileşimini kurar.
/// UI sahnesindeki HandRayUIInteractor ile aynı sistemi kullanır; bu sahne için ray baştan açık.
/// Sahneye bu component'i ekleyin; Canvas ve OVRCameraRig otomatik bulunur.
/// </summary>
public class UserNameVRUISetup : MonoBehaviour
{
    [Header("İsteğe bağlı atama")]
    [Tooltip("Boş bırakılırsa sahnedeki 'Canvas' adlı obje aranır")]
    public Canvas targetCanvas;
    [Tooltip("Boş bırakılırsa sahnedeki OVRCameraRig aranır")]
    public OVRCameraRig ovrCameraRig;

    [Header("Hand Ray ayarları")]
    [Tooltip("Sağ el (false) veya sol el (true)")]
    public bool useLeftHand = false;

    private HandRayUIInteractor _handRay;

    private void Awake()
    {
        if (targetCanvas == null)
        {
            var canvasGo = GameObject.Find("Canvas") ?? GameObject.Find("UserNameCanvas");
            if (canvasGo != null)
                targetCanvas = canvasGo.GetComponent<Canvas>();
            if (targetCanvas == null)
                targetCanvas = FindObjectOfType<Canvas>();
        }

        if (targetCanvas == null)
        {
            Debug.LogWarning("[UserNameVRUISetup] Canvas bulunamadı. VR UI etkileşimi çalışmayacak.");
            return;
        }

        // GraphicRaycaster yoksa ekle (World Space canvas için gerekli)
        if (targetCanvas.GetComponent<GraphicRaycaster>() == null)
            targetCanvas.gameObject.AddComponent<GraphicRaycaster>();

        if (ovrCameraRig == null)
            ovrCameraRig = FindObjectOfType<OVRCameraRig>();

        if (ovrCameraRig == null)
        {
            Debug.LogWarning("[UserNameVRUISetup] OVRCameraRig bulunamadı. VR'da çalıştırırken rig gerekli.");
            return;
        }

        _handRay = FindObjectOfType<HandRayUIInteractor>();
        if (_handRay == null)
        {
            GameObject go = new GameObject("UserNameVRUIInteractor");
            _handRay = go.AddComponent<HandRayUIInteractor>();
        }

        _handRay.targetCanvas = targetCanvas;
        _handRay.ovrCameraRig = ovrCameraRig;
        _handRay.useLeftHand = useLeftHand;
    }

    private void Start()
    {
        if (_handRay == null) return;
        // HandRayUIInteractor.Start() bittikten sonra ray'i aç (UI sahnesinde Retry'da açılıyor, burada hep açık)
        _handRay.EnableRay();
    }
}
