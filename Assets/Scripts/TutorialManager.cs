using UnityEngine;
using TMPro; 

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;  
    public GameObject[] boards;  
    public AudioClip damageSound; 
    public GameObject damageEffectPrefab;  
    public int score = 0;  

    void Start()
    {
        UpdateScore();
    }

    void Update()
    {

    }

    public void BulletHitTarget(GameObject board)
    {
        AudioSource.PlayClipAtPoint(damageSound, board.transform.position);

        ShowDamageEffect(board);

        score++;
        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }

    private void ShowDamageEffect(GameObject board)
    {
        GameObject damageEffect = Instantiate(damageEffectPrefab, board.transform.position, Quaternion.identity);
        damageEffect.transform.SetParent(board.transform);
        Destroy(damageEffect, 2f);  
    }
}
