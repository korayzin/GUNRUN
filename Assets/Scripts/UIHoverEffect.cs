using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHoverEffect : MonoBehaviour
{
    public Transform gunBarrel; 
    public float maxDistance = 50f; 
    public LayerMask buttonLayer; 

    private Image lastHoveredImage;
    private Color originalColor;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(gunBarrel.position, gunBarrel.forward, out hit, maxDistance, buttonLayer))
        {
            Debug.Log($"Ray temas etti: {hit.collider.gameObject.name}");

            Image image = hit.collider.GetComponentInChildren<Image>();
            if (image != null)
            {
                Debug.Log("UI Image bulundu!");

                if (lastHoveredImage != image)
                {
                    ResetLastImage();
                    lastHoveredImage = image;
                    originalColor = image.color;
                    image.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); 
                }
            }
            else
            {
                Debug.Log("Çarpýlan nesnede UI Image yok!");
            }
        }
        else
        {
            Debug.Log("Ray hiçbir nesneye çarpmadý!");
            ResetLastImage();
        }
    }

    void ResetLastImage()
    {
        if (lastHoveredImage != null)
        {
            lastHoveredImage.color = originalColor;
            lastHoveredImage = null;
        }
    }
}