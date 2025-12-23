using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public LayerMask layerMask;
    public LineRenderer linePrefab;
    public GameObject muzzleVFXPrefab;
    public GameObject impactVFXPrefab;
    public Transform shootingPoint;
    public Animator gunAnimator;

    [Header("OVR Input Settings")]
    public OVRInput.Button shootButton = OVRInput.Button.PrimaryIndexTrigger;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip shootingClip;

    [Header("Bullet Settings")]
    public float maxLineDistance = 50f;
    public float lineShowTime = 0.2f;

    private int shootCount = 0; // Shoot count to track number of shots fired
    public WeaponManager weaponManager; // Reference to WeaponManager

    private void Update()
    {
        if (OVRInput.GetDown(shootButton))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Increment shoot count
        shootCount++;
        
        // Weapon switch is now handled by WeaponManager based on kill count
        // No need to call CheckWeaponSwitch here as it's triggered by EnemyHealth.OnEnemyKilled event

        // Play shooting audio
        if (audioSource && shootingClip)
        {
            audioSource.PlayOneShot(shootingClip);
        }

        // Play shoot animation
        if (gunAnimator)
        {
            gunAnimator.SetTrigger("Shoot");
        }

        // Raycast logic
        Ray ray = new Ray(shootingPoint.position, shootingPoint.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxLineDistance, layerMask))
        {
            // Instantiate impact VFX at hit point
            if (impactVFXPrefab)
            {
                Instantiate(impactVFXPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }

            // Deal damage to enemy
            //Enemy enemy = hit.transform.GetComponent<Enemy>();
            //if (enemy != null)
            //{
              //  enemy.TakeDamage(1); // Adjust damage value as needed
            //}
        }

        // Line Renderer setup
        LineRenderer line = Instantiate(linePrefab);
        line.positionCount = 2;
        line.SetPosition(0, shootingPoint.position);
        line.SetPosition(1, hit.point);

        Destroy(line.gameObject, lineShowTime);
    }
}
