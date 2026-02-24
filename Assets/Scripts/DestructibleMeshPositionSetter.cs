using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// MRUK tarafından spawn edilen DestructibleMesh'in pozisyonunu ayarlar.
/// Mixed Reality için: AlignToScannedRoom kullanın – destructible mesh taranan gerçek duvarlarla aynı yerde olur.
/// </summary>
public class DestructibleMeshPositionSetter : MonoBehaviour
{
    public enum PositionMode
    {
        /// <summary>Mesh'i MRUK taranan oda ile hizala – gerçek dünya duvarlarıyla çakışır (Mixed Reality önerilen).</summary>
        AlignToScannedRoom,
        /// <summary>Sabit pozisyon kullan (targetPosition)</summary>
        FixedPosition,
        /// <summary>Oyuncu (OVRCameraRig CenterEye) pozisyonuna göre konumlandır</summary>
        UsePlayerPosition,
        /// <summary>Tracking space (zemin) referansı</summary>
        UseTrackingSpacePosition,
        /// <summary>Bu GameObject'in world pozisyonunu kullan</summary>
        UseThisTransformPosition
    }

    [SerializeField] private DestructibleGlobalMeshSpawner destructibleGlobalMeshSpawner;
    [Tooltip("PositionMode: FixedPosition kullanıldığında hedef pozisyon")]
    [SerializeField] private Vector3 targetPosition = Vector3.zero;
    [SerializeField] private bool useLocalPosition = true;
    [Tooltip("Mesh'i bu GameObject'in child'ı yap. AlignToScannedRoom modunda kullanılmaz (mesh oda ile hizalanır).")]
    [SerializeField] private bool reparentToThis = false;
    [Tooltip("PositionMode: UsePlayerPosition kullanıldığında oyuncudan offset (örn. zemin hizası için)")]
    [SerializeField] private Vector3 playerOffset = Vector3.zero;
    [Tooltip("AlignToScannedRoom = destructible mesh gerçek taranan duvarlarla aynı yerde (Mixed Reality önerilen).")]
    [SerializeField] private PositionMode positionMode = PositionMode.AlignToScannedRoom;
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

        if (positionMode == PositionMode.AlignToScannedRoom)
        {
            // Mixed Reality: destructible mesh = taranan gerçek duvarlar. Mesh'i MRUK room'a bağla.
            var room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;
            if (room != null && room.transform != null)
            {
                meshTransform.SetParent(room.transform, true);
                meshTransform.localPosition = Vector3.zero;
                meshTransform.localRotation = Quaternion.identity;
                meshTransform.localScale = Vector3.one;
                Debug.Log("[DestructibleMeshPositionSetter] Destructible mesh taranan oda ile hizalandı (gerçek duvarlar).");
            }
            else
            {
                Debug.LogWarning("[DestructibleMeshPositionSetter] MRUK/room bulunamadı, mesh pozisyonu değiştirilmedi.");
            }
            return;
        }

        // Diğer modlar: isteğe bağlı reparent ve özel pozisyon
        if (reparentToThis)
        {
            meshTransform.SetParent(transform, true);
        }

        Vector3 targetWorld = GetTargetPosition();
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

