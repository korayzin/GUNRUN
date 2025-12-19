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

        // UI referanslarını kontrol et
        Debug.Log("🎮 GameManager başlatıldı:");
        Debug.Log($"   - timerText: {(timerText != null ? "✅ Atanmış" : "❌ Atanmamış")}");
        Debug.Log($"   - restartCanvas: {(restartCanvas != null ? "✅ Atanmış" : "❌ Atanmamış")}");
        Debug.Log($"   - timeAndScorePanel: {(timeAndScorePanel != null ? "✅ Atanmış" : "❌ Atanmamış")}");

        // Eksik referansları otomatik bulmaya çalış
        if (restartCanvas == null)
        {
            restartCanvas = GameObject.Find("RestartCanvas") ?? GameObject.Find("Restart Canvas") ?? GameObject.FindGameObjectWithTag("Restart");
            if (restartCanvas != null)
            {
                Debug.Log($"🔍 RestartCanvas otomatik bulundu: {restartCanvas.name}");
            }
        }

        if (timeAndScorePanel == null)
        {
            timeAndScorePanel = GameObject.Find("UIAnchor") ?? GameObject.Find("InGameUI") ?? GameObject.Find("GameUI");
            if (timeAndScorePanel != null)
            {
                Debug.Log($"🔍 timeAndScorePanel otomatik bulundu: {timeAndScorePanel.name}");
            }
        }

        if (timerText == null)
        {
            // Süre text objesini bul
            GameObject timerObj = GameObject.Find("Süre text") ?? GameObject.Find("Timer Text") ?? GameObject.Find("Time Text");
            if (timerObj != null)
            {
                timerText = timerObj.GetComponent<TMPro.TextMeshProUGUI>();
                if (timerText != null)
                {
                    Debug.Log($"🔍 timerText otomatik bulundu: {timerObj.name}");
                }
            }
        }
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
        else
        {
            // Game over durumunda da timer UI'yi güncelle (son değeri göster)
            UpdateTimerUI();
        }
    }

    public void AddScore(int damage)
    {
        score += damage;
        UpdateScoreUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            string newText = Mathf.Ceil(gameTimer).ToString();
            timerText.text = newText;
            // Debug.Log($"⏰ Süre güncellendi: {newText}"); // Çok sık log atmaması için comment
        }
        else
        {
            if (Time.frameCount % 300 == 0) // 300 frame'de bir uyar
            {
                Debug.LogWarning("⚠️ timerText atanmamış! Unity Inspector'dan GameManager'a timerText'i atayın!");
            }
        }
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

        Debug.Log($"🎮 GAME OVER! Çarpılan obje: {hitCollider?.name ?? "Bilinmiyor"}");

        // Oyunu durdur
        Time.timeScale = 0f;
        Debug.Log("⏸️ Oyun durduruldu (Time.timeScale = 0)");

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

        if (restartCanvas != null)
        {
            restartCanvas.SetActive(true);
            Debug.Log("✅ Restart canvas gösterildi");
        }
        else
        {
            Debug.LogError("❌ restartCanvas atanmamış! Unity Inspector'dan GameManager'a restartCanvas'i atayın!");
        }

        if (backgroundMusic != null)
        {
            backgroundMusic.Stop();
        }

        Debug.Log("💀 GAME OVER TAMAMLANDI!");

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
        Debug.Log("🔄 Oyun yeniden başlatılıyor...");
        Time.timeScale = 1f; // Oyunu devam ettir
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
