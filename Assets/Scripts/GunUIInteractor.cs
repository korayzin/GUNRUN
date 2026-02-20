using UnityEngine;
using UnityEngine.EventSystems;

public class GunUIInteractor : MonoBehaviour
{
    public Transform shootPosition;
    public LayerMask uiLayer; // Set this to "UI" in the Inspector
    public WeaponSelector weaponSelector;


    private void Start()
    {
        
    }

    void Update()
    {
        if (GameManager.IsRetryScreenActive) return; // Retry ekranında sadece HandRayUIInteractor ile butonlara tıklanabilir
        Debug.Log("adsfafsd");
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
        {
            Debug.Log("XD");
        }

        bool raycastHit = Physics.Raycast(shootPosition.transform.position, shootPosition.transform.forward, out RaycastHit hit, 1000f, uiLayer);
        if (raycastHit && (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)))
        {
            Debug.LogError(hit.transform.gameObject.name);
            if (hit.transform.gameObject.name == "Button_Canik")
            {
                weaponSelector.SelectWeapon("Glock");
            }
            else if (hit.transform.gameObject.name == "Button_Silencer")
            {
                weaponSelector.SelectWeapon("Pyt");
            }
            else if (hit.transform.gameObject.name == "Button_Shotgun")
            {
                weaponSelector.SelectWeapon("Shotty");
            }
            else if (hit.transform.gameObject.name == "Button_Minimach")
            {
                weaponSelector.SelectWeapon("Minimach");
            }
        }
        else if (raycastHit)
        {
            //Debug.LogError(hit.transform.gameObject.name);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(shootPosition.transform.position, shootPosition.transform.position + shootPosition.transform.forward * 100f);
    }
}
