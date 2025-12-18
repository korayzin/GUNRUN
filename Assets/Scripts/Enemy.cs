using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject destructionVFX;  
    public MeshRenderer model;  

    private bool isDestroyed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet") && !isDestroyed)  
        {
            DestroyEnemy();
        }
    }

    private void DestroyEnemy()
    {
        isDestroyed = true;

        Instantiate(destructionVFX, transform.position, Quaternion.identity);

        model.enabled = false;


        Destroy(gameObject, 3f);  

        if (FindObjectOfType<TargetManager>().isWave())
        {
            ScoreKeeper.current.ChangeScore(10);
        }
    }
}
