using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponInfoController : MonoBehaviour
{
    [Header("UI Elemanlarý")]
    [SerializeField] private List<RectTransform> weaponImages; 
    [SerializeField] private RectTransform mainPosition; 
    [SerializeField] private Button leftButton; 
    [SerializeField] private Button rightButton; 

    [Header("Animasyon Ayarlarý")]
    [SerializeField] private float moveSpeed = 5f;

    private int currentIndex = 0; 

    private void Start()
    {
        leftButton.onClick.AddListener(MoveLeft);
        rightButton.onClick.AddListener(MoveRight);

        UpdateWeaponPositions();
    }

    private void MoveLeft()
    {
        currentIndex = (currentIndex - 1 + weaponImages.Count) % weaponImages.Count;
        UpdateWeaponPositions();
    }

    private void MoveRight()
    {
        currentIndex = (currentIndex + 1) % weaponImages.Count;
        UpdateWeaponPositions();
    }

    private void UpdateWeaponPositions()
    {
        for (int i = 0; i < weaponImages.Count; i++)
        {
            int relativeIndex = (i - currentIndex + weaponImages.Count) % weaponImages.Count;

            if (relativeIndex == 0)
            {
                StartCoroutine(SmoothMove(weaponImages[i], mainPosition.position));
            }
            else
            {
                Vector3 newPosition = CalculateOffPosition(relativeIndex);
                StartCoroutine(SmoothMove(weaponImages[i], newPosition));
            }
        }
    }

    private Vector3 CalculateOffPosition(int relativeIndex)
    {
        float offset = 200f; 
        Vector3 basePosition = mainPosition.position;

        if (relativeIndex == 1) return basePosition + new Vector3(offset, 0, 0);
        if (relativeIndex == weaponImages.Count - 1) return basePosition - new Vector3(offset, 0, 0); 

        return basePosition;
    }

    private IEnumerator SmoothMove(RectTransform weapon, Vector3 targetPosition)
    {
        while (Vector3.Distance(weapon.position, targetPosition) > 0.01f)
        {
            weapon.position = Vector3.Lerp(weapon.position, targetPosition, Time.deltaTime * moveSpeed);
            yield return null;
        }

        weapon.position = targetPosition; 
    }
}
