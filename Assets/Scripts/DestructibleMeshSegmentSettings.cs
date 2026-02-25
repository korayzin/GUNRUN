using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Meta.XR.MRUtilityKit;
/// <summary>
/// Destructible mesh segment sayısını sadece bu scriptten yönetir.
/// "Net segment sayısı" modunda girdiğin sayı kadar segment kalacak şekilde segmentasyon sonucu kırpılır (her seferinde aynı).
/// </summary>
[DefaultExecutionOrder(-200)]
public class DestructibleMeshSegmentSettings : MonoBehaviour
{
    [Tooltip("Açıksa sadece build'de uygula. Kapalıysa her yerde uygulanır.")]
    [SerializeField] private bool applyOnlyOnBuild = false;

    [Header("Mod")]
    [Tooltip("Açıksa 'Hedef segment sayısı' kullanılır (net sayı). Kapalıysa aşağıdaki Max/PerUnit değerleri kullanılır.")]
    [SerializeField] private bool useTargetSegmentCount = true;

    [Tooltip("Hedef segment sayısı. Override açıkken gönderilen nokta sayısı buna göre ayarlanır (≈ bu kadar segment gelir, boşluksuz).")]
    [SerializeField] [Min(32)] private int targetSegmentCount = 1000;
    [Tooltip("Hedef için nokta çarpanı (1 = hedefe yakın segment, boşluksuz; 1.5-2 = biraz fazla nokta).")]
    [SerializeField] [Range(1f, 2f)] private float segmentOvershoot = 1f;
    [Tooltip("Segment başına max üçgen. Aşanlar bölünür. Sayı düşükse (örn. 150) bölme çok olur ve segment sayısı artar; ~1000 segment için 400-500 kullan.")]
    [SerializeField] [Min(0)] private int maxTrianglesPerSegment = 450;
    [Tooltip("Açıksa her segment 1 üçgen olur (segment sayısı mesh üçgen sayısına eşitlenir, çok artar). Boşluksuz + az segment için KAPALI bırak.")]
    [SerializeField] private bool singleTrianglePerSegment = false;

    [Header("Manuel (useTargetSegmentCount kapalıysa)")]
    [SerializeField] [Min(256)] private int maxPointsCount = 1024;
    [SerializeField] [Min(0.5f)] private float pointsPerUnitX = 2f;
    [SerializeField] [Min(0.5f)] private float pointsPerUnitY = 2f;

    [Tooltip("Açıksa spawner'ın Max Point Count / Points Per Unit değerleri bu scriptten yazılır. Kapalıysa spawner'daki Inspector değerlerin korunur (sadece segment callback uygulanır).")]
    [SerializeField] private bool overrideSpawnerPointSettings = true;

    [Tooltip("Boş bırakırsan sahnedeki ilk DestructibleGlobalMeshSpawner kullanılır.")]
    [SerializeField] private DestructibleGlobalMeshSpawner spawner;

    private bool _applied;
    private static readonly List<GameObject> _segmentBuffer = new List<GameObject>();
    private readonly List<DestructibleGlobalMeshSpawner> _spawnersWeSubscribedTo = new List<DestructibleGlobalMeshSpawner>();

    private void GetEffectiveValues(out int maxPoints, out float perUnitX, out float perUnitY)
    {
        if (useTargetSegmentCount)
        {
            float overshoot = Mathf.Clamp(segmentOvershoot, 1f, 2f);
            maxPoints = Mathf.Max(32, Mathf.RoundToInt(targetSegmentCount * overshoot));
            perUnitX = 6f;
            perUnitY = 6f;
        }
        else
        {
            maxPoints = maxPointsCount;
            perUnitX = pointsPerUnitX;
            perUnitY = pointsPerUnitY;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spawner == null)
            spawner = FindObjectOfType<DestructibleGlobalMeshSpawner>();
        if (spawner == null || !overrideSpawnerPointSettings) return;

        GetEffectiveValues(out int maxPoints, out float perUnitX, out float perUnitY);
        spawner.MaxPointsCount = maxPoints;
        spawner.PointsPerUnitX = perUnitX;
        spawner.PointsPerUnitY = perUnitY;
        UnityEditor.EditorUtility.SetDirty(spawner);
    }
#endif

    private void Awake()
    {
        TryApply();
        ApplyCallbackToAllSpawners();
    }

    private void Start()
    {
        if (!_applied)
            TryApply();
        ApplyCallbackToAllSpawners();
        if (MRUK.Instance != null)
            MRUK.Instance.RoomCreatedEvent.AddListener(OnRoomCreated);
    }

    private void OnRoomCreated(MRUKRoom room)
    {
        ApplyCallbackToAllSpawners();
    }

    private void OnDisable()
    {
        if (MRUK.Instance != null)
            MRUK.Instance.RoomCreatedEvent.RemoveListener(OnRoomCreated);
        foreach (var s in _spawnersWeSubscribedTo)
        {
            if (s != null)
                s.OnDestructibleMeshCreated.RemoveListener(RemoveOversizedSegments);
        }
        _spawnersWeSubscribedTo.Clear();
    }

    private void ApplyCallbackToAllSpawners()
    {
        if (!useTargetSegmentCount) return;
        var spawners = FindObjectsOfType<DestructibleGlobalMeshSpawner>(true);
        var target = targetSegmentCount;
        var maxTri = maxTrianglesPerSegment;
        var singleTri = singleTrianglePerSegment;
        foreach (var s in spawners)
        {
            var prev = s.OnSegmentationCompleted;
            s.OnSegmentationCompleted = result =>
            {
                var trimmed = TrimSegmentsToExactCount(result, target, maxTri, singleTri);
                return prev != null ? prev(trimmed) : trimmed;
            };
        }
        if (spawners.Length > 0)
            Debug.Log($"[DestructibleMeshSegmentSettings] Callback {spawners.Length} spawner'a tekrar atandı (RoomCreated).");
    }

    private void RemoveOversizedSegments(DestructibleMeshComponent component)
    {
        if (component == null || maxTrianglesPerSegment <= 0) return;
        _segmentBuffer.Clear();
        component.GetDestructibleMeshSegments(_segmentBuffer);
        int maxIndices = maxTrianglesPerSegment * 3;
        int removed = 0;
        for (int i = _segmentBuffer.Count - 1; i >= 0; i--)
        {
            var go = _segmentBuffer[i];
            if (!go.TryGetComponent<MeshFilter>(out var mf) || mf.sharedMesh == null) continue;
            int indexCount = mf.sharedMesh.triangles?.Length ?? 0;
            if (indexCount <= maxIndices) continue;
            if (go == component.ReservedSegment)
                Object.Destroy(go);
            else
                component.DestroySegment(go);
            removed++;
        }
        if (removed > 0)
            Debug.Log($"[DestructibleMeshSegmentSettings] Max triangle aşan {removed} segment kaldırıldı (max={maxTrianglesPerSegment}). Boşlukları önlemek için Max Triangles altında bölme kullanın.");
    }

    private void TryApply()
    {
        if (applyOnlyOnBuild && Application.isEditor)
            return;
        if (_applied)
            return;

        if (spawner == null)
            spawner = FindObjectOfType<DestructibleGlobalMeshSpawner>();

        var spawnersToApply = spawner != null
            ? new[] { spawner }
            : FindObjectsOfType<DestructibleGlobalMeshSpawner>(true);

        if (spawnersToApply == null || spawnersToApply.Length == 0)
        {
            Debug.LogWarning("[DestructibleMeshSegmentSettings] DestructibleGlobalMeshSpawner bulunamadı.");
            return;
        }

        GetEffectiveValues(out int maxPoints, out float perUnitX, out float perUnitY);

        foreach (var s in spawnersToApply)
        {
            if (overrideSpawnerPointSettings)
            {
                s.MaxPointsCount = maxPoints;
                s.PointsPerUnitX = perUnitX;
                s.PointsPerUnitY = perUnitY;
            }

            if (useTargetSegmentCount)
            {
                var prev = s.OnSegmentationCompleted;
                var target = targetSegmentCount;
                var maxTri = maxTrianglesPerSegment;
                var singleTri = singleTrianglePerSegment;
                s.OnSegmentationCompleted = result =>
                {
                    int inCount = result.segments?.Count ?? 0;
                    var trimmed = TrimSegmentsToExactCount(result, target, maxTri, singleTri);
                    int outCount = trimmed.segments?.Count ?? 0;
                    Debug.Log($"[DestructibleMeshSegmentSettings] OnSegmentationCompleted: {inCount} -> {outCount} segment (maxTri={maxTri}, tekÜçgen={singleTri})");
                    return prev != null ? prev(trimmed) : trimmed;
                };
            }

            if (maxTrianglesPerSegment > 0)
            {
                // Subdivide callback'te büyük parçaları bölüyor; sonradan silme (boşluk oluşmasın).
            }
            else
            {
                s.OnDestructibleMeshCreated.AddListener(RemoveOversizedSegments);
                _spawnersWeSubscribedTo.Add(s);
            }
        }

        _applied = true;
        string msg = useTargetSegmentCount ? $"Hedef segment={targetSegmentCount} (net)" : $"MaxPoints={maxPoints}, PerUnit=({perUnitX}, {perUnitY})";
        if (!overrideSpawnerPointSettings) msg += ", spawner nokta ayarları korundu";
        Debug.Log($"[DestructibleMeshSegmentSettings] Uygulandı: {msg}");
    }

    private static DestructibleMeshComponent.MeshSegmentationResult TrimSegmentsToExactCount(
        DestructibleMeshComponent.MeshSegmentationResult result, int targetCount, int maxTrianglesPerSegment, bool singleTrianglePerSegment)
    {
        if (result.segments == null)
            return result;

        int maxIndices = maxTrianglesPerSegment > 0 ? maxTrianglesPerSegment * 3 : int.MaxValue;
        var expanded = new List<DestructibleMeshComponent.MeshSegment>();
        bool anySubdivided = false;

        foreach (var seg in result.segments)
        {
            int len = seg.indices?.Length ?? 0;
            if (len <= maxIndices)
                expanded.Add(seg);
            else
            {
                var pieces = SubdivideSegment(seg, maxIndices);
                if (pieces.Count > 0)
                {
                    expanded.AddRange(pieces);
                    anySubdivided = true;
                }
                else
                    expanded.Add(seg);
            }
        }

        var reserved = result.reservedSegment;
        int resLen = reserved.indices?.Length ?? 0;
        if (maxTrianglesPerSegment > 0 && resLen > maxIndices)
        {
            var pieces = SubdivideSegment(reserved, maxIndices);
            if (pieces.Count > 0)
            {
                expanded.AddRange(pieces);
                anySubdivided = true;
                reserved = new DestructibleMeshComponent.MeshSegment
                    { positions = new Vector3[0], indices = new int[0], uv = null, tangents = null };
            }
        }

        List<DestructibleMeshComponent.MeshSegment> kept;
        if (maxTrianglesPerSegment > 0 || anySubdivided)
            kept = expanded;
        else
        {
            var list = expanded.OrderBy(s => s.indices?.Length ?? 0).ToList();
            kept = list.Count <= targetCount ? list : list.Take(targetCount).ToList();
        }

        for (int i = kept.Count - 1; i >= 0; i--)
        {
            kept[i] = CleanSegmentTrianglesOnly(kept[i]);
            if (kept[i].indices == null || kept[i].indices.Length == 0)
                kept.RemoveAt(i);
        }
        if (reserved.indices != null && reserved.indices.Length > 0)
            reserved = CleanSegmentTrianglesOnly(reserved);

        if (singleTrianglePerSegment)
        {
            var singleList = new List<DestructibleMeshComponent.MeshSegment>();
            foreach (var seg in kept)
            {
                singleList.AddRange(SplitSegmentIntoSingleTriangles(seg));
            }
            kept = singleList;
            int singleCount = kept.Count;
            if (singleCount > targetCount)
            {
                kept = kept.Take(targetCount).ToList();
                Debug.Log($"[DestructibleMeshSegmentSettings] Tek üçgen sonrası hedefe kırpıldı: {singleCount} -> {targetCount} segment (mesh'te boşluk oluşabilir).");
            }
        }

        return new DestructibleMeshComponent.MeshSegmentationResult
        {
            segments = kept,
            reservedSegment = reserved
        };
    }

    private static List<DestructibleMeshComponent.MeshSegment> SplitSegmentIntoSingleTriangles(
        DestructibleMeshComponent.MeshSegment seg)
    {
        var indices = seg.indices;
        var positions = seg.positions;
        if (indices == null || positions == null || indices.Length < 3)
            return new List<DestructibleMeshComponent.MeshSegment>();
        var outList = new List<DestructibleMeshComponent.MeshSegment>();
        for (int t = 0; t + 2 < indices.Length; t += 3)
        {
            int i0 = indices[t], i1 = indices[t + 1], i2 = indices[t + 2];
            if (i0 == i1 || i1 == i2 || i0 == i2) continue;
            if (i0 < 0 || i0 >= positions.Length || i1 < 0 || i1 >= positions.Length || i2 < 0 || i2 >= positions.Length)
                continue;
            outList.Add(new DestructibleMeshComponent.MeshSegment
            {
                positions = new[] { positions[i0], positions[i1], positions[i2] },
                indices = new[] { 0, 1, 2 },
                uv = seg.uv != null && seg.uv.Length > 0
                    ? new[] { seg.uv[i0], seg.uv[i1], seg.uv[i2] }
                    : null,
                tangents = seg.tangents != null && seg.tangents.Length > 0
                    ? new[] { seg.tangents[i0], seg.tangents[i1], seg.tangents[i2] }
                    : null
            });
        }
        return outList;
    }

    private static DestructibleMeshComponent.MeshSegment CleanSegmentTrianglesOnly(
        DestructibleMeshComponent.MeshSegment seg)
    {
        var indices = seg.indices;
        var positions = seg.positions;
        if (indices == null || positions == null || indices.Length < 3)
            return seg;
        var newIndices = new List<int>();
        for (int i = 0; i + 2 < indices.Length; i += 3)
        {
            int i0 = indices[i], i1 = indices[i + 1], i2 = indices[i + 2];
            if (i0 == i1 || i1 == i2 || i0 == i2) continue;
            if (i0 < 0 || i0 >= positions.Length || i1 < 0 || i1 >= positions.Length || i2 < 0 || i2 >= positions.Length)
                continue;
            Vector3 a = positions[i0], b = positions[i1], c = positions[i2];
            if (Vector3.Cross(b - a, c - a).sqrMagnitude < 1e-10f) continue;
            newIndices.Add(i0);
            newIndices.Add(i1);
            newIndices.Add(i2);
        }
        if (newIndices.Count == 0)
            return new DestructibleMeshComponent.MeshSegment
                { positions = new Vector3[0], indices = new int[0], uv = null, tangents = null };
        var used = new HashSet<int>();
        for (int j = 0; j < newIndices.Count; j++) used.Add(newIndices[j]);
        var oldToNew = new Dictionary<int, int>();
        var newPositions = new List<Vector3>();
        var newUv = seg.uv != null ? new List<Vector2>() : null;
        var newTangents = seg.tangents != null ? new List<Vector4>() : null;
        foreach (int oldIdx in used.OrderBy(x => x))
        {
            oldToNew[oldIdx] = newPositions.Count;
            newPositions.Add(positions[oldIdx]);
            if (newUv != null && oldIdx < seg.uv.Length) newUv.Add(seg.uv[oldIdx]);
            if (newTangents != null && oldIdx < seg.tangents.Length) newTangents.Add(seg.tangents[oldIdx]);
        }
        var remapped = new int[newIndices.Count];
        for (int j = 0; j < newIndices.Count; j++)
            remapped[j] = oldToNew[newIndices[j]];
        return new DestructibleMeshComponent.MeshSegment
        {
            positions = newPositions.ToArray(),
            indices = remapped,
            uv = newUv?.ToArray(),
            tangents = newTangents?.ToArray()
        };
    }

    private static List<DestructibleMeshComponent.MeshSegment> SubdivideSegment(
        DestructibleMeshComponent.MeshSegment seg, int maxIndices)
    {
        var positions = seg.positions;
        var indices = seg.indices;
        if (positions == null || indices == null || indices.Length < 3)
            return new List<DestructibleMeshComponent.MeshSegment> { seg };

        int triCount = indices.Length / 3;
        if (triCount * 3 <= maxIndices)
            return new List<DestructibleMeshComponent.MeshSegment> { seg };

        Vector3 min = positions[0], max = positions[0];
        for (int i = 1; i < positions.Length; i++)
        {
            min = Vector3.Min(min, positions[i]);
            max = Vector3.Max(max, positions[i]);
        }
        Vector3 size = max - min;
        if (size.x < 1e-5f) size.x = 1e-5f;
        if (size.y < 1e-5f) size.y = 1e-5f;
        if (size.z < 1e-5f) size.z = 1e-5f;

        int gridRes = Mathf.Max(2, Mathf.CeilToInt(Mathf.Pow((float)(triCount * 3) / maxIndices, 1f / 3f)));
        int nx = gridRes, ny = gridRes, nz = gridRes;
        var cells = new Dictionary<int, List<int>>();
        for (int t = 0; t < triCount; t++)
        {
            int i0 = indices[t * 3], i1 = indices[t * 3 + 1], i2 = indices[t * 3 + 2];
            Vector3 c = (positions[i0] + positions[i1] + positions[i2]) / 3f;
            int cx = Mathf.Clamp(Mathf.FloorToInt((c.x - min.x) / size.x * nx), 0, nx - 1);
            int cy = Mathf.Clamp(Mathf.FloorToInt((c.y - min.y) / size.y * ny), 0, ny - 1);
            int cz = Mathf.Clamp(Mathf.FloorToInt((c.z - min.z) / size.z * nz), 0, nz - 1);
            int cellId = cx + nx * (cy + ny * cz);
            if (!cells.ContainsKey(cellId)) cells[cellId] = new List<int>();
            cells[cellId].Add(i0);
            cells[cellId].Add(i1);
            cells[cellId].Add(i2);
        }

        var outList = new List<DestructibleMeshComponent.MeshSegment>();
        foreach (var kv in cells)
        {
            var triIndices = kv.Value;
            if (triIndices.Count == 0) continue;
            var piece = BuildPieceFromTriList(seg, positions, triIndices);
            if (piece.indices.Length <= maxIndices)
            {
                outList.Add(piece);
                continue;
            }
            outList.AddRange(SplitSegmentByTriangleOrder(piece, maxIndices));
            continue;
        }
        if (outList.Count > 0)
            return outList;
        return new List<DestructibleMeshComponent.MeshSegment> { seg };
    }

    private static DestructibleMeshComponent.MeshSegment BuildPieceFromTriList(
        DestructibleMeshComponent.MeshSegment seg, Vector3[] positions, List<int> triIndices)
    {
        var used = new HashSet<int>();
        for (int i = 0; i < triIndices.Count; i++) used.Add(triIndices[i]);
        var oldToNew = new Dictionary<int, int>();
        var newPositions = new List<Vector3>();
        var newIndices = new List<int>();
        var newUv = seg.uv != null ? new List<Vector2>() : null;
        var newTangents = seg.tangents != null ? new List<Vector4>() : null;
        foreach (int oldIdx in used.OrderBy(x => x))
        {
            oldToNew[oldIdx] = newPositions.Count;
            newPositions.Add(positions[oldIdx]);
            if (newUv != null && oldIdx < seg.uv.Length) newUv.Add(seg.uv[oldIdx]);
            if (newTangents != null && oldIdx < seg.tangents.Length) newTangents.Add(seg.tangents[oldIdx]);
        }
        for (int i = 0; i < triIndices.Count; i += 3)
        {
            newIndices.Add(oldToNew[triIndices[i]]);
            newIndices.Add(oldToNew[triIndices[i + 1]]);
            newIndices.Add(oldToNew[triIndices[i + 2]]);
        }
        return new DestructibleMeshComponent.MeshSegment
        {
            positions = newPositions.ToArray(),
            indices = newIndices.ToArray(),
            uv = newUv?.ToArray(),
            tangents = newTangents?.ToArray()
        };
    }

    private static List<DestructibleMeshComponent.MeshSegment> SplitSegmentByTriangleOrder(
        DestructibleMeshComponent.MeshSegment seg, int maxIndices)
    {
        var indices = seg.indices;
        var positions = seg.positions;
        if (indices == null || indices.Length < 3) return new List<DestructibleMeshComponent.MeshSegment> { seg };
        int maxTri = maxIndices / 3;
        var outList = new List<DestructibleMeshComponent.MeshSegment>();
        for (int start = 0; start < indices.Length; start += maxTri * 3)
        {
            int count = Mathf.Min(maxTri * 3, indices.Length - start);
            if (count < 3) break;
            var chunk = new int[count];
            System.Array.Copy(indices, start, chunk, 0, count);
            var used = new HashSet<int>();
            for (int i = 0; i < chunk.Length; i++) used.Add(chunk[i]);
            var oldToNew = new Dictionary<int, int>();
            var newPositions = new List<Vector3>();
            var newIndices = new List<int>();
            var newUv = seg.uv != null ? new List<Vector2>() : null;
            var newTangents = seg.tangents != null ? new List<Vector4>() : null;
            foreach (int oldIdx in used.OrderBy(x => x))
            {
                oldToNew[oldIdx] = newPositions.Count;
                newPositions.Add(positions[oldIdx]);
                if (newUv != null && oldIdx < seg.uv.Length) newUv.Add(seg.uv[oldIdx]);
                if (newTangents != null && oldIdx < seg.tangents.Length) newTangents.Add(seg.tangents[oldIdx]);
            }
            for (int i = 0; i < chunk.Length; i += 3)
            {
                newIndices.Add(oldToNew[chunk[i]]);
                newIndices.Add(oldToNew[chunk[i + 1]]);
                newIndices.Add(oldToNew[chunk[i + 2]]);
            }
            outList.Add(new DestructibleMeshComponent.MeshSegment
            {
                positions = newPositions.ToArray(),
                indices = newIndices.ToArray(),
                uv = newUv?.ToArray(),
                tangents = newTangents?.ToArray()
            });
        }
        return outList.Count > 0 ? outList : new List<DestructibleMeshComponent.MeshSegment> { seg };
    }
}
