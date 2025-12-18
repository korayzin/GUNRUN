using UnityEngine;
using UnityEngine.UI;

public class BoxCollision : MonoBehaviour
{
    public Image hiddenImage; // UI Image objesi (Inspector'dan atayacaksýn)

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Stick")) // Çubuk objene "Stick" tag'ini ekle
        {
            hiddenImage.gameObject.SetActive(true); // Görünür yap
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Stick"))
        {
            hiddenImage.gameObject.SetActive(false); // Gizle
        }
    }
}
