using UnityEngine;

/// <summary>
/// Harita seçim paneli butonları için sadece hafif büyüme hover efekti.
/// Glitch yapan letter-spacing / renk / bounce yok; sadece scale.
/// HandRayUIInteractor bu component varsa ButtonRayAnimator yerine bunu kullanır.
/// </summary>
[RequireComponent(typeof(UnityEngine.UI.Button))]
public class MapSelectionButtonHover : MonoBehaviour
{
    [Tooltip("Hover'da büyüme çarpanı (1 = yok, 1.05 = %5 büyüme)")]
    [Range(1f, 1.15f)]
    public float hoverScale = 1.05f;

    private RectTransform _rect;
    private Vector3 _originalScale;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (_rect != null) _originalScale = _rect.localScale;
    }

    private void OnEnable()
    {
        if (_rect != null) _originalScale = _rect.localScale;
    }

    public void OnHoverEnter()
    {
        if (_rect != null)
            _rect.localScale = _originalScale * hoverScale;
    }

    public void OnHoverExit()
    {
        if (_rect != null)
            _rect.localScale = _originalScale;
    }
}
