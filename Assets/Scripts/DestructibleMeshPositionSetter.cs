using UnityEngine;
using Meta.XR.MRUtilityKit;

public class DestructibleMeshPositionSetter : MonoBehaviour
{
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private bool useLocalPosition = true;
    [SerializeField] private DestructibleGlobalMeshSpawner destructibleGlobalMeshSpawner;

    private void OnEnable()
    {
        if (destructibleGlobalMeshSpawner != null)
        {
            destructibleGlobalMeshSpawner.OnDestructibleMeshCreated.AddListener(OnDestructibleMeshCreated);
        }
    }

    private void OnDisable()
    {
        if (destructibleGlobalMeshSpawner != null)
        {
            destructibleGlobalMeshSpawner.OnDestructibleMeshCreated.RemoveListener(OnDestructibleMeshCreated);
        }
    }

    private void OnDestructibleMeshCreated(DestructibleMeshComponent destructibleMeshComponent)
    {
        if (destructibleMeshComponent != null)
        {
            Transform meshTransform = destructibleMeshComponent.transform;
            
            if (useLocalPosition)
            {
                meshTransform.localPosition = targetPosition;
            }
            else
            {
                meshTransform.position = targetPosition;
            }
            
            Debug.Log($"DestructibleMesh pozisyonu ayarlandı: {targetPosition}");
        }
    }
}

