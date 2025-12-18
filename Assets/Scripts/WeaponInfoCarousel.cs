using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponInfoSwitcher : MonoBehaviour
{
    public List<GameObject> weaponPanels; 
    private int currentIndex = 0; 

    private void Start()
    {
        UpdateWeaponDisplay(); 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextWeapon();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousWeapon();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bullet")) return;

        if (this.CompareTag("RightButton")) NextWeapon();
        else if (this.CompareTag("LeftButton")) PreviousWeapon();

        Destroy(other.gameObject); 
    }

    private void NextWeapon()
    {
        if (currentIndex < weaponPanels.Count - 1)
        {
            currentIndex++;
            UpdateWeaponDisplay();
        }
    }

    private void PreviousWeapon()
    {
        if (currentIndex >= 0)
        {
            currentIndex--;
            UpdateWeaponDisplay();
        }
    }

    private void UpdateWeaponDisplay()
    {
        for (int i = 0; i < weaponPanels.Count; i++)
        {
            weaponPanels[i].SetActive(i == currentIndex); 
        }
    }
}
