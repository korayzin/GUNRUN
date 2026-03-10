using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    private static bool IsTutorialScene()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Tutorial"
            || FindObjectOfType<TutorialIntroController>() != null;
    }
    public static GameManager Instance;
    public GameObject restartCanvas;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI secondaryScoreText;
    public TextMeshProUGUI timerText;
    public GameObject timeAndScorePanel;
    [Header("Background Music")]
    public AudioSource backgroundMusic;
    [Tooltip("Portal/stage sesi çalarken müziğin kısılacağı oran (0-1, AudioSource volume'un bu kadarı kalır)")]
    [Range(0f, 1f)]
    public float backgroundMusicDuckVolume = 0.25f;
    [Tooltip("Müziğin kısılma süresi (saniye)")]
    public float backgroundMusicDuckFadeDuration = 0.25f;
    
    private AdvancedPortalSpawner portalSpawner;
    private Coroutine _duckCoroutine;
    
    [Header("Portal Sounds")]
    [Tooltip("Portallar açıldığında çalacak ses (her portal için bir kez)")]
    public AudioClip portalOpenSound;

    [Header("Stage Sounds")]
    [Tooltip("Stage 1: İlk düşman spawn olduğunda çalacak ses")]
    public AudioClip stage1FirstEnemySpawnSound;
    [Tooltip("Stage 2: Stage 2'ye geçildiğinde çalacak ses")]
    public AudioClip stage2TransitionSound;
    [Tooltip("Stage 3: Stage 3'e geçildiğinde çalacak ses")]
    public AudioClip stage3TransitionSound;
    [Tooltip("Stage/portal sesleri için AudioSource. ÖNEMLİ: backgroundMusic ile AYNI olmamalı! Boş veya aynıysa PlayClipAtPoint kullanılır.")]
    public AudioSource stageSoundsAudioSource;
    [Tooltip("Portal ve stage seslerinin çalma seviyesi (1 = normal, 5 = 5x güçlü)")]
    [Range(0.5f, 5f)]
    public float stagePortalSoundVolume = 2f;
    
    [Header("Hand Ray UI Interactor")]
    [Tooltip("Retry menüsünde el ray etkileşimi için - otomatik bulunur eğer atanmazsa")]
    public HandRayUIInteractor handRayInteractor;

    private float gameTimer;
    public float gameDuration = 150f;

    [Header("Tutorial Timer")]
    [Tooltip("Tutorial'da 9. diyalogdan sonra süre (sn) - 360'dan geriye sayar")]
    public float tutorialTimerDuration = 360f;
    [Tooltip("23. diyalog sonrası serbest oyun süresi (sn) - geriye sayar")]
    public float tutorialFinalPhaseDuration = 45f;
    private bool _tutorialTimerActive = false;

    public int score = 0;
    private bool isGameOver = false;

    /// <summary>Retry ekranı aktif mi? Bu durumda sadece HandRayUIInteractor ile Retry/Main Menu butonlarına tıklanabilir, ateş vb. kapalı.</summary>
    public static bool IsRetryScreenActive => Instance != null && Instance.isGameOver;

    /// <summary>Tutorial 9. diyalogdan sonra süreyi başlat. Pause'da otomatik durur (Time.deltaTime=0).
    /// NOT: Score and Time Panel 23. diyalog sonrasına kadar kapalı kalır (freetime verildikten sonra açılır).</summary>
    public void StartTutorialTimer()
    {
        _tutorialTimerActive = true;
        gameTimer = tutorialTimerDuration;
        // Panel 23. diyalog sonrası StartTutorialFinalPhaseTimer ile açılacak
        UpdateTimerUI();
    }

    /// <summary>23. diyalog sonrası 45 sn geri sayım başlat. Tutorial final phase.</summary>
    public void StartTutorialFinalPhaseTimer()
    {
        _tutorialTimerActive = true;
        gameTimer = tutorialFinalPhaseDuration;
        if (timeAndScorePanel != null)
            timeAndScorePanel.SetActive(true);
        UpdateTimerUI();
    }

    /// <summary>Tutorial retry: süreyi yenile. Death/energy/time retry sonrası çağrılır.</summary>
    public void RefillTutorialTimer()
    {
        if (_tutorialTimerActive)
        {
            gameTimer = tutorialTimerDuration;
            UpdateTimerUI();
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        DestructibleMeshExperience.allowTriggerToBreakWalls = false; // Sadece mermi duvar kırsın

        // Tutorial sahnesinde normal oyun akışı çalışmasın
        if (IsTutorialScene())
        {
            Time.timeScale = 0f;
            return;
        }

        if (Time.timeScale != 1f)
        {
            Debug.LogWarning($"⚠️ Awake'te Time.timeScale = {Time.timeScale}, 1'e ayarlanıyor...");
            Time.timeScale = 1f;
        }
    }

    private void Start()
    {
        if (IsTutorialScene())
        {
            portalSpawner = FindObjectOfType<AdvancedPortalSpawner>();
            if (timeAndScorePanel == null)
                timeAndScorePanel = GameObject.Find("Score and Time Panel") ?? GameObject.Find("UIAnchor") ?? GameObject.Find("InGameUI") ?? GameObject.Find("GameUI");
            // Score and Time Panel 23. diyalog sonrası (freetime) açılacak; başlangıçta kapalı
            if (timeAndScorePanel != null)
                timeAndScorePanel.SetActive(false);
            UpdateScoreUI();
            return;
        }

        Time.timeScale = 1f;
        Debug.Log($"🕐 Time.timeScale başlangıçta {Time.timeScale} olarak ayarlandı");
        
        // Oyun durumlarını sıfırla
        isGameOver = false;
        score = 0;
        
        gameTimer = gameDuration;
        UpdateTimerUI();

        if (backgroundMusic != null)
        {
            if (MusicManager.Instance != null)
                MusicManager.Instance.RegisterMusicSource(backgroundMusic);
            backgroundMusic.Play();
        }

        portalSpawner = FindObjectOfType<AdvancedPortalSpawner>();

        UpdateScoreUI();

        // UI referanslarını kontrol et
        Debug.Log("🎮 GameManager başlatıldı:");
        Debug.Log($"   - timerText: {(timerText != null ? "✅ Atanmış" : "❌ Atanmamış")}");
        Debug.Log($"   - restartCanvas: {(restartCanvas != null ? "✅ Atanmış" : "❌ Atanmamış")}");
        Debug.Log($"   - timeAndScorePanel: {(timeAndScorePanel != null ? "✅ Atanmış" : "❌ Atanmamış")}");
        
        // HandRayUIInteractor'ı bul (atanmamışsa)
        if (handRayInteractor == null)
        {
            handRayInteractor = FindObjectOfType<HandRayUIInteractor>();
            if (handRayInteractor != null)
            {
                Debug.Log("🔍 HandRayUIInteractor otomatik bulundu");
            }
        }

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
        if (IsTutorialScene())
        {
            if (_tutorialTimerActive && !isGameOver)
            {
                gameTimer -= Time.deltaTime;
                UpdateTimerUI();
                if (gameTimer <= 0)
                {
                    gameTimer = 0;
                    GameOver(null);
                }
            }
            return;
        }

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
        if (IsTutorialScene()) return;
        score += damage;
        UpdateScoreUI();
        // Skor modunda stage geçişi ve sesler skora göre anında tetiklensin
        if (GameBalanceManager.Instance != null && GameBalanceManager.Instance.UseScoreForStage && portalSpawner != null)
            portalSpawner.RefreshStage();
    }

    /// <summary>
    /// Portal açılma sesini çalar. Inspector'dan portalOpenSound atayın.
    /// Ana müzik bu sırada yavaşça kısılır.
    /// NOT: stageSoundsAudioSource, backgroundMusic ile AYNI olmamalı - yoksa duck sırasında SFX de kısılır.
    /// </summary>
    public void PlayPortalOpenSound()
    {
        if (portalOpenSound == null) return;
        DuckBackgroundForSFX(portalOpenSound);
        PlayStagePortalSFX(portalOpenSound);
    }

    /// <summary>
    /// Stage değişim anlarında ses çalar. stage: 1 = ilk düşman spawn, 2 = stage2 geçişi, 3 = stage3 geçişi.
    /// Inspector'dan stage1FirstEnemySpawnSound, stage2TransitionSound, stage3TransitionSound atayın.
    /// Ana müzik bu sırada yavaşça kısılır.
    /// </summary>
    public void PlayStageSound(int stage)
    {
        AudioClip clip = stage switch
        {
            1 => stage1FirstEnemySpawnSound,
            2 => stage2TransitionSound,
            3 => stage3TransitionSound,
            _ => null
        };
        if (clip == null) return;

        DuckBackgroundForSFX(clip);
        PlayStagePortalSFX(clip);
    }

    /// <summary>
    /// Portal/stage SFX çalar. backgroundMusic ile aynı AudioSource kullanılmaz (duck çakışmasını önler).
    /// </summary>
    private void PlayStagePortalSFX(AudioClip clip)
    {
        bool useDedicatedSource = stageSoundsAudioSource != null && stageSoundsAudioSource != backgroundMusic;
        if (useDedicatedSource)
        {
            stageSoundsAudioSource.PlayOneShot(clip, stagePortalSoundVolume);
        }
        else
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            AudioSource.PlayClipAtPoint(clip, pos, Mathf.Min(1f, stagePortalSoundVolume));
        }
    }

    /// <summary>
    /// Portal veya stage sesi çalarken ana müziği yavaşça kısar, ses bitince geri yükseltir.
    /// </summary>
    private void DuckBackgroundForSFX(AudioClip clip)
    {
        if (backgroundMusic == null || !backgroundMusic.isPlaying) return;
        if (_duckCoroutine != null)
            StopCoroutine(_duckCoroutine);
        _duckCoroutine = StartCoroutine(DuckBackgroundCoroutine(clip));
    }

    private System.Collections.IEnumerator DuckBackgroundCoroutine(AudioClip clip)
    {
        float normalVolume = backgroundMusic.volume; // Inspector'daki değer
        float targetVolume = normalVolume * backgroundMusicDuckVolume;
        float duration = Mathf.Max(0.05f, backgroundMusicDuckFadeDuration);
        float clipLength = clip != null ? clip.length : 0.5f;
        float waitTime = Mathf.Max(clipLength * 0.8f, 0.3f);

        // Kıs
        float elapsed = 0f;
        float startVol = backgroundMusic.volume;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            backgroundMusic.volume = Mathf.Lerp(startVol, targetVolume, t);
            yield return null;
        }
        backgroundMusic.volume = targetVolume;

        yield return new WaitForSecondsRealtime(waitTime);

        // Geri yükselt (Inspector'daki orijinal değere)
        elapsed = 0f;
        startVol = backgroundMusic.volume;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            backgroundMusic.volume = Mathf.Lerp(startVol, normalVolume, t);
            yield return null;
        }
        backgroundMusic.volume = normalVolume;
        _duckCoroutine = null;
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
        string displayText = IsTutorialScene() ? "∞" : score.ToString();
        if (scoreText != null)
        {
            scoreText.text = displayText;
        }
        if (secondaryScoreText != null)
        {
            secondaryScoreText.text = displayText;
        }
    }

    public void GameOver(Collider hitCollider)
    {
        if (isGameOver) return;

        // Tutorial: süre bitti (23. diyalog sonrası 45 sn) - 24. diyalog göster
        if (IsTutorialScene() && TutorialIntroController.TutorialActive && hitCollider == null && TutorialIntroController.TutorialCompleteFreehand)
        {
            _tutorialTimerActive = false; // Tekrar GameOver tetiklenmesin
            var ctrl = FindObjectOfType<TutorialIntroController>();
            if (ctrl != null)
            {
                ctrl.HandleTutorialFinalPhaseComplete();
                return;
            }
        }

        // Tutorial retry: ölünce normal game over yerine retry diyaloğu
        if (IsTutorialScene() && TutorialIntroController.TutorialActive)
        {
            var ctrl = FindObjectOfType<TutorialIntroController>();
            if (ctrl != null)
            {
                ctrl.HandleTutorialDeath(hitCollider);
                return;
            }
        }

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
            
            // Hand Ray UI Interactor'ı etkinleştir (Retry sonrası ikinci Game Over'da da çalışması için)
            if (handRayInteractor == null)
                handRayInteractor = FindObjectOfType<HandRayUIInteractor>();
            if (handRayInteractor != null)
            {
                // Canvas referansını ayarla (eğer atanmamışsa)
                Canvas canvasComponent = restartCanvas.GetComponent<Canvas>();
                if (canvasComponent != null)
                {
                    handRayInteractor.SetTargetCanvas(canvasComponent);
                }
                handRayInteractor.EnableRay();
                Debug.Log("🎯 Hand Ray UI Interactor etkinleştirildi");
            }
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
        
        // ÖNCELİKLE Time.timeScale'i sıfırla (sahne yüklenmeden önce)
        Time.timeScale = 1f;
        
        // Hand Ray UI Interactor'ı devre dışı bırak
        if (handRayInteractor != null)
        {
            handRayInteractor.DisableRay();
        }
        
        // Tüm durumları sıfırla
        isGameOver = false;
        score = 0;
        gameTimer = gameDuration;
        
        // Tüm coroutine'leri durdur
        StopAllCoroutines();
        
        Debug.Log($"✅ Time.timeScale = {Time.timeScale}, Sahne yükleniyor...");
        
        // Sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    /// <summary>
    /// Ana menüye dön
    /// </summary>
    public void GoToMainMenu()
    {
        Debug.Log("🏠 Ana menüye dönülüyor...");
        
        // Time.timeScale'i sıfırla
        Time.timeScale = 1f;
        
        // Hand Ray UI Interactor'ı devre dışı bırak
        if (handRayInteractor != null)
        {
            handRayInteractor.DisableRay();
        }
        
        isGameOver = false;
        StopAllCoroutines();
        
        // Ana menü sahnesini yükle (Deneme sahnesi)
        SceneManager.LoadScene("Deneme");
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
            
            // "puan text" = sadece "score" label'ı, puan değil - sabit "score" yaz
            if (text.gameObject.name.Equals("puan text", System.StringComparison.OrdinalIgnoreCase))
            {
                text.text = "score";
                continue;
            }
            // Diğer "puan" içeren label'ları atla (güncelleme)
            if (textName.Contains("puan"))
            {
                continue;
            }
            
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
