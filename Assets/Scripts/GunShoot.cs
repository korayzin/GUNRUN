using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunShoot : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletPosition;
    [SerializeField] private float shootDelay = 0.2f;
    [Range(0, 3000), SerializeField] private float bulletSpeed;

    [Space, SerializeField] private AudioSource audioSource;
    [SerializeField] private float hapticDuration = 0.1f;

    private float lastShot;


    private void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        if (lastShot > Time.time) return;

        lastShot = Time.time + shootDelay;

        GunShotAudio();
        StartHapticFeedback(hapticDuration);

        var bulletPrefab = Instantiate(bullet, bulletPosition.position, bulletPosition.rotation);
        var bulletRB = bulletPrefab.GetComponent<Rigidbody>();

        var direction = bulletPrefab.transform.TransformDirection(Vector3.forward);
        bulletRB.AddForce(direction * bulletSpeed);
        Destroy(bulletPrefab, 5f);
    }

    private void GunShotAudio()
    {
        var random = UnityEngine.Random.Range(0.8f, 1.2f);
        audioSource.pitch = random;

        audioSource.Play();
    }

    private void StartHapticFeedback(float duration)
    {
        OVRInput.SetControllerVibration(0.8f, 1.0f, OVRInput.Controller.RTouch); 
        StartCoroutine(StopHapticFeedback(duration));
    }

    private IEnumerator StopHapticFeedback(float duration)
    {
        yield return new WaitForSeconds(duration);
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch); 
    }
}
