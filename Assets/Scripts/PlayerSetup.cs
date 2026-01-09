using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    void Start()
    {
        SetupPlayer();
    }

    void SetupPlayer()
    {
        // OVRCameraRig'i bul
        OVRCameraRig ovrRig = FindObjectOfType<OVRCameraRig>();
        if (ovrRig != null)
        {
            // Player tag'ini ayarla
            ovrRig.tag = "Player";
            Debug.Log("✅ Player tag'i ayarlandı: OVRCameraRig");

            // Collider kontrolü ve ekleme
            Collider existingCollider = ovrRig.GetComponent<Collider>();
            if (existingCollider == null)
            {
                // CapsuleCollider ekle
                CapsuleCollider capsuleCol = ovrRig.gameObject.AddComponent<CapsuleCollider>();
                capsuleCol.radius = 0.5f;
                capsuleCol.height = 2f;
                capsuleCol.center = new Vector3(0, 1f, 0);
                capsuleCol.isTrigger = false; // Solid collider (enemy'ler trigger edecek)

                Debug.Log("✅ Player CapsuleCollider eklendi (Solid)");
            }
            else
            {
                existingCollider.isTrigger = false; // Solid yap
                Debug.Log("ℹ️ Player mevcut collider solid yapıldı");
            }

            // Rigidbody kontrolü (kinematic olarak)
            Rigidbody rb = ovrRig.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = ovrRig.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;
                Debug.Log("✅ Player Rigidbody eklendi (Kinematic)");
            }
        }
        else
        {
            Debug.LogError("❌ OVRCameraRig bulunamadı! Player setup yapılamadı.");
        }
    }
}

