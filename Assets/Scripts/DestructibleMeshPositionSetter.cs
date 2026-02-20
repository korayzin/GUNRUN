using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// MRUK tarafından spawn edilen DestructibleMesh'in pozisyonunu ayarlar.
/// Mesh, MRUK room.GlobalMeshAnchor altında spawn olduğu için sahnedeki Destructible GameObject'i
/// değiştirmek etkili olmaz. Bu script mesh'i reparent ederek veya oyuncu pozisyonuna göre konumlandırarak çözer.
/// Retry sonrası duvarın yukarıda spawn olmasını önlemek için pozisyon birkaç frame sonra uygulanır (tracking stabilizasyonu).
/// </summary>
public class DestructibleMeshPositionSetter : MonoBehaviour
{
    public enum PositionMode
    {
        /// <summary>Sabit pozisyon kullan (targetPosition)</summary>
        FixedPosition,
        /// <summary>Oyuncu (OVRCameraRig CenterEye) pozisyonuna göre konumlandır - room oyuncunun etrafında olur</summary>
        UsePlayerPosition,
        /// <summary>Tracking space (zemin) referansı - retry sonrası daha tutarlı yükseklik</summary>
        UseTrackingSpacePosition,
        /// <summary>Bu GameObject'in world pozisyonunu kullan - Destructible'ı sahnede taşıyarak kontrol edebilirsin</summary>
        UseThisTransformPosition
    }

    [SerializeField] private DestructibleGlobalMeshSpawner destructibleGlobalMeshSpawner;
    [Tooltip("PositionMode: FixedPosition kullanıldığında hedef pozisyon")]
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private bool useLocalPosition = true;
    [Tooltip("Mesh'i bu GameObject'in child'ı yap - böylece Destructible'ı sahnede taşıyarak room pozisyonunu kontrol edebilirsin")]
    [SerializeField] private bool reparentToThis = true;
    [Tooltip("PositionMode: UsePlayerPosition kullanıldığında oyuncudan offset (örn. zemin hizası için)")]
    [SerializeField] private Vector3 playerOffset = Vector3.zero;
    [SerializeField] private PositionMode positionMode = PositionMode.UsePlayerPosition;
    [Tooltip("Retry sonrası duvar yüksekliği tutarlılığı için pozisyon uygulama gecikmesi (frame). 0 = anında.")]
    [SerializeField] private int positionApplyDelayFrames = 3;

    private OVRCameraRig _cameraRig;

    private void Awake()
    {
        _cameraRig = FindObjectOfType<OVRCameraRig>();
    }

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
        if (destructibleMeshComponent == null) return;

        // Önce reparent ve ilk pozisyonu uygula
        ApplyPositionToMesh(destructibleMeshComponent);

        // Retry sonrası tracking farklı olabilir; birkaç frame sonra pozisyonu tekrar uygula
        if (positionApplyDelayFrames > 0)
        {
            StartCoroutine(ReapplyPositionAfterDelay(destructibleMeshComponent));
        }
        else
        {
            ApplyPositionToMesh(destructibleMeshComponent);
        }
    }

    private IEnumerator ReapplyPositionAfterDelay(DestructibleMeshComponent destructibleMeshComponent)
    {
        for (int i = 0; i < positionApplyDelayFrames; i++)
        {
            yield return null;
        }

        if (destructibleMeshComponent != null)
        {
            ApplyPositionToMesh(destructibleMeshComponent);
        }
    }

    private void ApplyPositionToMesh(DestructibleMeshComponent destructibleMeshComponent)
    {
        if (destructibleMeshComponent == null) return;

        Transform meshTransform = destructibleMeshComponent.transform;

        // 1. Reparent: Mesh'i bu GameObject'in child'ı yap - böylece sahnedeki Destructible pozisyonu room'u kontrol eder
        if (reparentToThis)
        {
            meshTransform.SetParent(transform, true); // worldPositionStays = true
        }

        // 2. Hedef pozisyonu hesapla (world space)
        Vector3 targetWorld = GetTargetPosition();

        // 3. Pozisyonu uygula
        if (reparentToThis && useLocalPosition)
        {
            meshTransform.localPosition = transform.InverseTransformPoint(targetWorld);
        }
        else
        {
            meshTransform.position = targetWorld;
        }

        Debug.Log($"[DestructibleMeshPositionSetter] Room pozisyonu ayarlandı: mode={positionMode}, target={targetWorld}");
    }

    private Vector3 GetTargetPosition()
    {
        switch (positionMode)
        {
            case PositionMode.UsePlayerPosition:
                if (_cameraRig != null && _cameraRig.centerEyeAnchor != null)
                {
                    return _cameraRig.centerEyeAnchor.position + playerOffset;
                }
                Debug.LogWarning("[DestructibleMeshPositionSetter] OVRCameraRig bulunamadı, origin kullanılıyor.");
                return playerOffset;

            case PositionMode.UseTrackingSpacePosition:
                if (_cameraRig != null)
                {
                    Transform trackingSpace = _cameraRig.transform.Find("TrackingSpace");
                    Vector3 basePos = trackingSpace != null ? trackingSpace.position : _cameraRig.transform.position;
                    return basePos + playerOffset;
                }
                Debug.LogWarning("[DestructibleMeshPositionSetter] OVRCameraRig bulunamadı, origin kullanılıyor.");
                return playerOffset;

            case PositionMode.UseThisTransformPosition:
                return transform.position + targetPosition;

            case PositionMode.FixedPosition:
            default:
                return targetPosition;
        }
    }
}

