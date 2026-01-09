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
        
        // Score'u önce güncelle (restart canvas açılmadan önce)
        UpdateScoreUI();
        
        // Firebase'e max score kaydet
        if (FirebaseLeaderboardManager.Instance != null)
        {
            FirebaseLeaderboardManager.Instance.SaveMaxScore(score, (success) =>
            {
                if (success)
                {
                    Debug.Log($"✅ Firebase'e max score kaydedildi: {score}");
                }
                else
                {
                    Debug.LogWarning("⚠️ Firebase'e score kaydedilemedi veya daha yüksek score zaten var.");
                }
            });
        }

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
            Debug.Log($"✅ Restart canvas gösterildi - Final Score: {score}");
            
            // Restart canvas açıldıktan SONRA score'u güncelle (UI aktif olmalı)
            StartCoroutine(UpdateScoreAfterCanvasActive());
        }
        else
        {
            Debug.LogError("❌ restartCanvas atanmamış! Unity Inspector'dan GameManager'a restartCanvas'i atayın!");
        }
        
        // Leaderboard'u göster (Firebase'den bağımsız)
        Debug.Log("🎯 Game Over'da leaderboard açma çağrısı yapılıyor...");
        StartCoroutine(ShowLeaderboardDelayed());

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
    
    private System.Collections.IEnumerator ShowLeaderboardDelayed()
    {
        Debug.Log("⏳ Leaderboard açma delay başlıyor...");
        // Kısa bir delay (UI'ların aktif olması için)
        yield return new WaitForSecondsRealtime(0.5f);
        Debug.Log("✅ Leaderboard açma delay bitti");

        // LeaderboardUI'yi bul ve göster
        LeaderboardUI leaderboardUI = FindObjectOfType<LeaderboardUI>();
        if (leaderboardUI != null)
        {
            Debug.Log($"✅ LeaderboardUI bulundu: {leaderboardUI.gameObject.name}");
            leaderboardUI.ShowLeaderboard();
            Debug.Log("✅ Leaderboard otomatik açıldı");

            // Ekstra kontrol - panel gerçekten aktif mi?
            if (leaderboardUI.leaderboardPanel != null)
            {
                Debug.Log($"📊 Leaderboard panel aktif durumu: {leaderboardUI.leaderboardPanel.activeSelf}");
                if (!leaderboardUI.leaderboardPanel.activeSelf)
                {
                    leaderboardUI.leaderboardPanel.SetActive(true);
                    Debug.Log("🔄 Leaderboard panel manuel aktif edildi");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ LeaderboardUI bulunamadı! Scene'de LeaderboardUI script'i olan bir GameObject var mı kontrol et.");
            Debug.LogWarning("   Tüm GameObject'ler kontrol ediliyor...");

            // Tüm GameObject'leri kontrol et
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.GetComponent<LeaderboardUI>() != null)
                {
                    Debug.Log($"   🔍 LeaderboardUI bulundu: {obj.name}");
                    obj.GetComponent<LeaderboardUI>().ShowLeaderboard();
                    break;
                }
            }
        }
    }
    
    private System.Collections.IEnumerator UpdateScoreAfterCanvasActive()
    {
        // Canvas aktif olana kadar bekle
        yield return new WaitForEndOfFrame();
        yield return new WaitForSecondsRealtime(0.2f);
        
        // Score'u güncelle
        UpdateScoreUI();
        UpdateRestartCanvasScore();
        
        // Bir kez daha güncelle (bazı UI'lar geç yüklenebilir)
        yield return new WaitForSecondsRealtime(0.3f);
        UpdateScoreUI();
        UpdateRestartCanvasScore();
        
        Debug.Log($"✅ Score güncellendi: {score}");
    }
    
    private void UpdateRestartCanvasScore()
    {
        // Restart canvas içindeki tüm score text'lerini bul ve güncelle
        if (restartCanvas == null)
        {
            Debug.LogWarning("⚠️ RestartCanvas null, score güncellenemedi");
            return;
        }
        
        Debug.Log($"📊 Restart canvas içinde score güncelleniyor... (Score: {score})");
        
        // Önce atanmış text'leri güncelle
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
            Debug.Log($"✅ ScoreText güncellendi: {scoreText.gameObject.name} = {score}");
        }
        else
        {
            Debug.LogWarning("⚠️ scoreText null!");
        }
        
        if (secondaryScoreText != null)
        {
            secondaryScoreText.text = score.ToString();
            Debug.Log($"✅ SecondaryScoreText güncellendi: {secondaryScoreText.gameObject.name} = {score}");
        }
        
        // Restart canvas içinde TÜM text'leri bul ve score içerenleri güncelle
        TextMeshProUGUI[] allTexts = restartCanvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        Debug.Log($"📊 Restart canvas içinde {allTexts.Length} text bulundu");
        
        int updatedCount = 0;
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text == null) continue;
            
            string textName = text.gameObject.name.ToLower();
            string textContent = text.text.ToLower();
            
            // Score içeren text'leri bul
            bool isScoreText = textName.Contains("score") || 
                              textContent.Contains("score") ||
                              text == scoreText || 
                              text == secondaryScoreText;
            
            // Veya sadece sayı içeriyorsa (muhtemelen score)
            int currentValue;
            bool isNumeric = int.TryParse(text.text.Trim(), out currentValue);
            
            if (isScoreText || (isNumeric && currentValue < score))
            {
                text.text = score.ToString();
                updatedCount++;
                Debug.Log($"✅ Score text güncellendi: {text.gameObject.name} = {score} (önceki: {text.text})");
            }
        }
        
        Debug.Log($"📊 Toplam {updatedCount} score text güncellendi");
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
