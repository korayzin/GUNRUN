using UnityEngine;
using Meta.XR.MRUtilityKit;

/// <summary>
/// Destructible mesh segment yoğunluğunu runtime'da override eder.
/// Build ve Editor'da aynı segment sayısı için DestructibleGlobalMeshSpawner'daki
/// deterministic cull kullanılır; bu script sadece "1 mermi = 1 küçük kırık" için
/// nokta/segment sayısını artırır.
/// </summary>
public class DestructibleMeshSegmentSettings : MonoBehaviour
{
    [Tooltip("Segment sayısı üst sınırı. Yüksek = daha küçük parçalar, 1 mermi ≈ 1 segment. (Varsayılan spawner: 512)")]
    [Min(256)]
    public int maxPointsCount = 2048;

    [Tooltip("X başına nokta (duvar genişliği). Yüksek = daha ince grid. (Varsayılan: 32)")]
    [Min(1f)]
    public float pointsPerUnitX = 48f;

    [Tooltip("Y başına nokta (duvar yüksekliği). Yüksek = daha ince grid. (Varsayılan: 32)")]
    [Min(1f)]
    public float pointsPerUnitY = 48f;

    [Tooltip("Boş bırakırsan sahnedeki ilk DestructibleGlobalMeshSpawner kullanılır.")]
    public DestructibleGlobalMeshSpawner spawner;

    private void Awake()
    {
        if (spawner == null)
            spawner = FindObjectOfType<DestructibleGlobalMeshSpawner>();

        if (spawner == null)
        {
            Debug.LogWarning("[DestructibleMeshSegmentSettings] DestructibleGlobalMeshSpawner bulunamadı.");
            return;
        }

        spawner.MaxPointsCount = maxPointsCount;
        spawner.PointsPerUnitX = pointsPerUnitX;
        spawner.PointsPerUnitY = pointsPerUnitY;
    }
}
