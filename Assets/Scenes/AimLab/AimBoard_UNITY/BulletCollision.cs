using UnityEngine;
using UnityEngine.SceneManagement;

public class BulletCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WeaponButton")) 
        {
            string buttonName = other.name; 

            WeaponSelector weaponSelector = FindObjectOfType<WeaponSelector>();
            if (weaponSelector != null)
            {
                weaponSelector.SelectWeapon(buttonName);
            }

            Destroy(gameObject);
        }

        if (other.CompareTag("RetryButton"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (other.CompareTag("BackToMainMenu"))
        {
            SceneManager.LoadScene("UI");
        }
    }
}
