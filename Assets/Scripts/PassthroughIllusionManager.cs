using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// First Encounters–style passthrough illusion: spawns a giant inner sphere (virtual background)
/// at (0,0,0) so that when destructible wall segments are destroyed, the virtual environment
/// is revealed behind the passthrough mesh. The sphere has no colliders and uses an unlit
/// shader with Cull Front so the interior is visible from inside the room.
/// </summary>
public class PassthroughIllusionManager : MonoBehaviour
{
    [Header("Virtual background sphere")]
    [Tooltip("Optional 360° / skybox-style texture (e.g. space). If null, a solid color is used as placeholder.")]
    [SerializeField] private Texture2D backgroundTexture;
    [Tooltip("Placeholder color when no texture is assigned")]
    [SerializeField] private Color placeholderColor = new Color(0.02f, 0.02f, 0.06f, 1f);
    [Tooltip("Sphere scale (default 100 so room sits inside)")]
    [SerializeField] private Vector3 sphereScale = new Vector3(100f, 100f, 100f);

    private GameObject _sphere;
    private Material _sphereMaterial;

    private void Start()
    {
        CreateBackgroundSphere();
    }

    private void OnDestroy()
    {
        if (_sphereMaterial != null)
            Destroy(_sphereMaterial);
        if (_sphere != null)
            Destroy(_sphere);
    }

    private void CreateBackgroundSphere()
    {
        _sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _sphere.name = "PassthroughIllusionBackground";
        _sphere.transform.SetParent(transform, false);
        _sphere.transform.localPosition = Vector3.zero;
        _sphere.transform.localRotation = Quaternion.identity;
        _sphere.transform.localScale = sphereScale;

        var collider = _sphere.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        if (unlitShader == null)
        {
            Debug.LogError("[PassthroughIllusionManager] URP Unlit / Unlit/Color shader not found.");
            return;
        }

        _sphereMaterial = new Material(unlitShader);
        _sphereMaterial.name = "PassthroughIllusionBackgroundMat";
        _sphereMaterial.SetInt("_Cull", (int)CullMode.Front);

        if (backgroundTexture != null)
        {
            _sphereMaterial.mainTexture = backgroundTexture;
            _sphereMaterial.color = Color.white;
            if (_sphereMaterial.HasProperty("_BaseMap"))
                _sphereMaterial.SetTexture("_BaseMap", backgroundTexture);
            if (_sphereMaterial.HasProperty("_BaseColor"))
                _sphereMaterial.SetColor("_BaseColor", Color.white);
        }
        else
        {
            _sphereMaterial.color = placeholderColor;
            if (_sphereMaterial.HasProperty("_BaseColor"))
                _sphereMaterial.SetColor("_BaseColor", placeholderColor);
        }

        _sphereMaterial.renderQueue = (int)RenderQueue.Geometry - 1;

        var renderer = _sphere.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = _sphereMaterial;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
