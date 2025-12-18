using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject restartCanvas;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI secondaryScoreText;
    public TextMeshProUGUI timerText;
    public GameObject timeAndScorePanel;
    public AudioSource backgroundMusic;
    private AdvancedPortalSpawner portalSpawner;

    private float gameTimer;
    public float gameDuration = 150f;

    public int score = 0;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        gameTimer = gameDuration;
        UpdateTimerUI();

        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }

        portalSpawner = FindObjectOfType<AdvancedPortalSpawner>();

        UpdateScoreUI();
    }

    private void Update()
    {
        if (!isGameOver)
        {
            gameTimer -= Time.deltaTime;
            UpdateTimerUI();

            if (gameTimer <= 0)
            {
                gameTimer = 0;
                GameOver(null);
            }
        }
    }

    public void AddScore(int damage)
    {
        score += damage;
        UpdateScoreUI();
    }

    private void UpdateTimerUI()
    {
        timerText.text = Mathf.Ceil(gameTimer).ToString();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
        if (secondaryScoreText != null)
        {
            secondaryScoreText.text = score.ToString();
        }
    }

    public void GameOver(Collider hitCollider)
    {
        if (isGameOver) return;
        isGameOver = true;

        SaveBestScores();

        if (portalSpawner != null)
        {
            portalSpawner.StopSpawning();
        }

        EnemyBehavior[] enemies = FindObjectsOfType<EnemyBehavior>();
        foreach (EnemyBehavior enemy in enemies)
        {
            enemy.gameObject.SetActive(false); 
        }

        if (timeAndScorePanel != null)
        {
            timeAndScorePanel.SetActive(false);
        }

        restartCanvas.SetActive(true);

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        if (hitCollider != null)
        {
            Debug.Log("Game Over! Oyuncu " + hitCollider.name + " tarafından vuruldu.");
        }
        else
        {
            Debug.Log("Game Over! Süre doldu.");
        }
    }

    public void RestartGame()
    {
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SaveBestScores()
    {
        List<ScoreEntry> bestScores = GetBestScores();

        bestScores.Add(new ScoreEntry { score = score, date = DateTime.Now.ToString("dd/MM/yyyy") });
        bestScores = bestScores.OrderByDescending(s => s.score).Take(5).ToList();

        string scoresJson = JsonUtility.ToJson(new ScoreList { scores = bestScores });
        PlayerPrefs.SetString("BestScores", scoresJson);
        PlayerPrefs.Save();

        Debug.Log("Yeni Best Score Listesi Kaydedildi: " + string.Join(", ", bestScores.Select(s => s.score + " (" + s.date + ")")));
    }

    public List<ScoreEntry> GetBestScores()
    {
        string json = PlayerPrefs.GetString("BestScores", "");
        if (!string.IsNullOrEmpty(json))
        {
            return JsonUtility.FromJson<ScoreList>(json).scores;
        }
        return new List<ScoreEntry>(); 
    }
}

[System.Serializable]
public class ScoreEntry
{
    public int score;
    public string date;
}

[System.Serializable]
public class ScoreList
{
    public List<ScoreEntry> scores;
}
