using UnityEngine;
using TMPro;

/// <summary>
/// Silah değişimi için kalan kill sayısını gösterir.
/// Tüm silahlar (1→2, 2→3, ... 8→9) için geçerli; sadece 9. silahtayken metin gizlenir.
/// Sayı kısmı her zaman beyaz renkte gösterilir.
/// </summary>
public class WeaponSwitchCountdownUI : MonoBehaviour
{
    [Tooltip("Güncellenecek Text (TextMeshPro - UGUI). Metni Inspector'dan sen yazabilirsin; {0} kalan kill sayısıyla değiştirilir (beyaz renkte).")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Örnek: \"silah değişimi için son {0} kill.\" — {0} = kalan kill sayısı (kodda beyaz yapılır)")]
    public string textFormat = "silah değişimi için son {0} kill.";

    /// <summary> Sayı kısmı için zorla kullanılan renk (TMP rich text). </summary>
    private const string NumberColorTag = "<color=#FFFFFF>";

    private void Update()
    {
        if (countdownText == null) return;

        WeaponManager wm = WeaponManager.Instance;
        if (wm == null)
        {
            countdownText.gameObject.SetActive(false);
            return;
        }

        // 9 silah tamamlandıysa countdown gösterme; joystick ile serbest geçiş var
        if (wm.AllWeaponsUnlocked)
        {
            countdownText.gameObject.SetActive(false);
            return;
        }

        int currentWeapon = wm.GetCurrentWeaponIndex();
        int killCount = wm.GetEnemyKillCount();
        int threshold = GetThresholdForNextWeapon(wm, currentWeapon);

        // 9. silahtayken sonraki yok, gizle
        if (threshold < 0)
        {
            countdownText.gameObject.SetActive(false);
            return;
        }

        int remaining = Mathf.Max(0, threshold - killCount);
        countdownText.gameObject.SetActive(true);
        // Sayıyı beyaz renkte göstermek için rich text ile {0} yerine <color=#FFFFFF>X</color> koy
        string numberPart = NumberColorTag + remaining + "</color>";
        countdownText.text = string.Format(textFormat, numberPart);
    }

    /// <summary> Mevcut silahtan sonrakine geçiş için gereken toplam kill eşiğini döndürür; son silahta -1. </summary>
    private static int GetThresholdForNextWeapon(WeaponManager wm, int currentWeapon)
    {
        switch (currentWeapon)
        {
            case 0: return wm.killsToSecond;
            case 1: return wm.killsToThird;
            case 2: return wm.killsToFourth;
            case 3: return wm.killsToFifth;
            case 4: return wm.killsToSixth;
            case 5: return wm.killsToSeventh;
            case 6: return wm.killsToEighth;
            case 7: return wm.killsToNinth;
            default: return -1; // 8 = 9. silah, sonraki yok
        }
    }
}
