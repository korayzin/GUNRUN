using UnityEngine;
using TMPro;

/// <summary>
/// SLOWED yazısının kameraya bakmasını sağlar (FloatingText gibi).
/// </summary>
public class SlowedTextController : MonoBehaviour
{
    private TextMeshPro textMesh;
    
    private void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
    }
    
    private void Update()
    {
        // Kamera'ya bak (FloatingText gibi)
        if (Camera.main != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - transform.position;
            directionToCamera.y = 0; // Sadece Y ekseni etrafında döndür
            if (directionToCamera != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }
    }
}
