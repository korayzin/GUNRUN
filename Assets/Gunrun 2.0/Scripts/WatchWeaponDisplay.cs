using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Saat (Watch) üzerinde aktif silaha göre ikon gösterir.
/// WeaponManager.OnWeaponChanged ile senkron çalışır; silah değişince image güncellenir.
/// Prefab: Watch root'una ekle, Weapon Sprites (0=1. silah ... 8=9. silah) atanır.
/// </summary>
public class WatchWeaponDisplay : MonoBehaviour
{
    [Tooltip("Sırayla: 1. silah, 2. silah, ... 9. silah ikonu. Eksik indeks boş bırakılabilir.")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[9];

    [Tooltip("Boş bırakılırsa prefab içindeki Image (Canvas altı) otomatik bulunur.")]
    [SerializeField] private Image displayImage;

    private void Awake()
    {
        if (displayImage == null)
            displayImage = GetComponentInChildren<Image>(true);
    }

    private void OnEnable()
    {
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.OnWeaponChanged += ApplyWeaponIcon;
            ApplyWeaponIcon(WeaponManager.Instance.GetCurrentWeaponIndex());
        }
    }

    private void OnDisable()
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnWeaponChanged -= ApplyWeaponIcon;
    }

    private void ApplyWeaponIcon(int weaponIndex)
    {
        if (displayImage == null || weaponSprites == null) return;
        if (weaponIndex < 0 || weaponIndex >= weaponSprites.Length) return;
        Sprite s = weaponSprites[weaponIndex];
        if (s != null)
            displayImage.sprite = s;
    }
}
