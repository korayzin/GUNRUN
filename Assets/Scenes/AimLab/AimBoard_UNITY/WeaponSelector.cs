using System.Collections;
using UnityEngine;

public class WeaponSelector : MonoBehaviour
{
    public GameObject GlockR;
    public GameObject GlockL;
    public GameObject Pyt;
    public GameObject Shotty;
    public GameObject Minimach;

    private GameObject activeWeapon;

    void Start()
    {
        activeWeapon = GlockR;
        GlockR.SetActive(true);
        GlockL.SetActive(true);
        Pyt.SetActive(false);
        Shotty.SetActive(false);
        Minimach.SetActive(false);
    }

    public void SelectWeapon(string weaponName)
    {
        StartCoroutine(AddDelay(0.1f, weaponName));
    }

    private IEnumerator AddDelay(float t, string weaponName)
    {
        yield return new WaitForSeconds(t);
        if (activeWeapon != null)
        {
            if (activeWeapon == GlockR || activeWeapon == GlockL)
            {
                GlockR.SetActive(false);
                GlockL.SetActive(false);
            }
            else
            {
                activeWeapon.SetActive(false);
            }
        }

        if (weaponName.Contains("Glock"))
        {
            activeWeapon = GlockR;
            GlockR.SetActive(true);
            GlockL.SetActive(true);
        }
        else if (weaponName.Contains("Pyt"))
        {
            activeWeapon = Pyt;
        }
        else if (weaponName.Contains("Shotty"))
        {
            activeWeapon = Shotty;
        }
        else if (weaponName.Contains("Minimach"))
        {
            activeWeapon = Minimach;
        }

        activeWeapon.SetActive(true);
    }
}
