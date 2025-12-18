using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUiManager : MonoBehaviour
{
    public GameObject[] weaponInfoPanels;
    private int currentWeaponIndex = 0;

    public void NextWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % weaponInfoPanels.Length;
        UpdateWeaponUI();
    }

    public void PreviousWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex - 1 + weaponInfoPanels.Length) % weaponInfoPanels.Length;
        UpdateWeaponUI();
    }

    private void UpdateWeaponUI()
    {
        for (int i = 0; i < weaponInfoPanels.Length; i++)
        {
            weaponInfoPanels[i].SetActive(i == currentWeaponIndex);
        }
    }
}
