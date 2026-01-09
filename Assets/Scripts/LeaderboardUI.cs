using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardPanel;
    public Transform leaderboardContent;
    public GameObject leaderboardEntryPrefab;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI myScoreText;
    public Button refreshButton;
    
    [Header("Settings")]
    public int maxEntries = 50;
    
    private void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(LoadLeaderboard);
        }
        
        // Eksik referansları otomatik bul
        AutoFindReferences();
    }
    
    private void AutoFindReferences()
    {
        // Leaderboard panel'i otomatik bul
        if (leaderboardPanel == null)
        {
            // Önce scene'deki tüm GameObject'leri ara
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                string name = obj.name.ToLower();
                if (name.Contains("leaderboard") && name.Contains("panel"))
                {
                    leaderboardPanel = obj;
                    Debug.Log($"🔍 Leaderboard panel bulundu: {obj.name}");
                    break;
                }
            }

            // Bulunamazsa standart isimlerle ara
            if (leaderboardPanel == null)
            {
                leaderboardPanel = GameObject.Find("LeaderboardPanel") ??
                                 GameObject.Find("Leaderboard") ??
                                 GameObject.Find("LeaderboardUI") ??
                                 GameObject.FindGameObjectWithTag("Leaderboard");
                if (leaderboardPanel != null)
                {
                    Debug.Log($"🔍 Leaderboard panel otomatik bulundu: {leaderboardPanel.name}");
                }
            }
        }

        // My score text'i otomatik bul
        FindMyScoreText();
        
        // Content'i otomatik bul
        if (leaderboardContent == null && leaderboardPanel != null)
        {
            // Önce direkt child'larda ara
            Transform content = leaderboardPanel.transform.Find("Content");
            
            // Bulunamazsa ScrollView içinde ara
            if (content == null)
            {
                Transform scrollView = leaderboardPanel.transform.Find("ScrollView");
                if (scrollView != null)
                {
                    content = scrollView.Find("Content") ?? scrollView.Find("Viewport/Content");
                }
            }
            
            // Bulunamazsa Viewport içinde ara
            if (content == null)
            {
                Transform viewport = leaderboardPanel.transform.Find("Viewport");
                if (viewport != null)
                {
                    content = viewport.Find("Content");
                }
            }
            
            // Bulunamazsa tüm child'larda "Content" ara
            if (content == null)
            {
                Transform[] allChildren = leaderboardPanel.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name == "Content" && child.GetComponent<RectTransform>() != null)
                    {
                        content = child;
                        break;
                    }
                }
            }

            if (content != null)
            {
                leaderboardContent = content;
                Debug.Log($"🔍 Leaderboard content otomatik bulundu: {content.name}");
            }
            else
            {
                Debug.LogWarning("⚠️ Leaderboard content bulunamadı! ScrollView içinde 'Content' GameObject'i var mı kontrol et.");
            }
        }

        // My score text'i otomatik bul
        FindMyScoreText();
    }

    private void FindMyScoreText()
    {
        if (myScoreText != null) return;

        // Leaderboard panel içinde "score" içeren text'leri ara
        if (leaderboardPanel != null)
        {
            TextMeshProUGUI[] texts = leaderboardPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in texts)
            {
                string name = text.gameObject.name.ToLower();
                string content = text.text.ToLower();

                if (name.Contains("score") || content.Contains("senin") || content.Contains("max"))
                {
                    myScoreText = text;
                    Debug.Log($"🔍 My score text otomatik bulundu: {text.gameObject.name}");
                    break;
                }
            }
        }

        // Bulunamazsa tüm scene'de ara
        if (myScoreText == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("score"))
                {
                    TextMeshProUGUI text = obj.GetComponent<TextMeshProUGUI>();
                    if (text != null)
                    {
                        myScoreText = text;
                        Debug.Log($"🔍 My score text tüm scene'den bulundu: {obj.name}");
                        break;
                    }
                }
            }
        }
    }
    
    public void ShowLeaderboard()
    {
        // Önce referansları tekrar kontrol et
        AutoFindReferences();

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);

            // Canvas kontrolü
            Canvas canvas = leaderboardPanel.GetComponentInParent<Canvas>();
            if (canvas != null && !canvas.enabled)
            {
                canvas.enabled = true;
            }

            // Kısa bir delay sonra load et (UI'ın aktif olması için)
            StartCoroutine(LoadLeaderboardDelayed());
        }
        else
        {
            // Hata durumunda basit bir mesaj göster
            Debug.LogError("❌ LEADERBOARD PANEL BULUNAMADI! Unity'de LeaderboardUI kurulumunu kontrol et.");
        }
    }
    
    private System.Collections.IEnumerator LoadLeaderboardDelayed()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        LoadLeaderboard();
    }
    
    public void HideLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }
    
    public void LoadLeaderboard()
    {
        Debug.Log("📊 Leaderboard yükleniyor...");
        
        // Instance kontrolü
        if (FirebaseLeaderboardManager.Instance == null)
        {
            Debug.LogError("❌ FirebaseLeaderboardManager Instance bulunamadı! Scene'de FirebaseLeaderboardManager GameObject'i var mı kontrol et.");
            if (loadingText != null)
            {
                loadingText.text = "Firebase bağlantısı yok!";
            }
            return;
        }
        
        if (loadingText != null)
        {
            loadingText.text = "Yükleniyor...";
            loadingText.gameObject.SetActive(true);
        }
        
        // Mevcut entry'leri temizle
        ClearLeaderboard();
        
        // Leaderboard'u çek
        FirebaseLeaderboardManager.Instance.GetLeaderboard((entries) =>
        {
            Debug.Log($"📊 Leaderboard çekildi: {entries?.Count ?? 0} oyuncu bulundu");
            DisplayLeaderboard(entries);
            
            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(false);
            }
        }, maxEntries);
        
        // Kendi max score'unu göster
        FirebaseLeaderboardManager.Instance.GetMyMaxScore((myScore) =>
        {
            if (myScoreText != null)
            {
                myScoreText.text = $"🏆 Senin Max Skorun: {myScore}";
                myScoreText.gameObject.SetActive(true);
            }
            else
            {
                // Otomatik bulmaya çalış
                FindMyScoreText();
                if (myScoreText != null)
                {
                    myScoreText.text = $"🏆 Senin Max Skorun: {myScore}";
                    myScoreText.gameObject.SetActive(true);
                }
            }
        });
    }
    
    private void DisplayLeaderboard(List<LeaderboardEntry> entries)
    {
        if (leaderboardContent == null)
        {
            Debug.LogError($"❌ LEADERBOARD CONTENT YOK! Unity'de LeaderboardUI'a Content'i ata!");
            return;
        }

        // Loading text'i gizle
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        if (entries == null || entries.Count == 0)
        {
            // Veri yoksa sadece kendi score'unu göster
            if (loadingText != null)
            {
                loadingText.text = "📊 Genel leaderboard henüz boş!\nAma senin max score'un kaydedildi.";
                loadingText.gameObject.SetActive(true);
            }
            return;
        }

        // Başlık ekle
        CreateHeaderEntry();

        // Tüm entry'leri göster
        for (int i = 0; i < entries.Count; i++)
        {
            LeaderboardEntry entry = entries[i];
            CreateLeaderboardEntry(i + 1, entry);
        }
    }

    private void CreateHeaderEntry()
    {
        GameObject headerObj = new GameObject("LeaderboardHeader");
        headerObj.transform.SetParent(leaderboardContent);

        // RectTransform ayarla
        RectTransform rectTransform = headerObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, 50);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        // Horizontal Layout Group ekle
        UnityEngine.UI.HorizontalLayoutGroup layoutGroup = headerObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = true;

        // Rank Header
        GameObject rankObj = new GameObject("RankHeader");
        rankObj.transform.SetParent(headerObj.transform);
        TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
        rankText.text = "Sıra";
        rankText.fontSize = 20;
        rankText.fontStyle = FontStyles.Bold;
        rankText.alignment = TextAlignmentOptions.Left;
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.sizeDelta = new Vector2(60, 0);

        // Name Header
        GameObject nameObj = new GameObject("NameHeader");
        nameObj.transform.SetParent(headerObj.transform);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Oyuncu";
        nameText.fontSize = 20;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Left;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(150, 0);

        // Score Header
        GameObject scoreObj = new GameObject("ScoreHeader");
        scoreObj.transform.SetParent(headerObj.transform);
        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "Max Skor";
        scoreText.fontSize = 20;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.alignment = TextAlignmentOptions.Right;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.sizeDelta = new Vector2(100, 0);
    }
    
    private void CreateLeaderboardEntry(int rank, LeaderboardEntry entry)
    {
        GameObject entryObj;
        
        if (leaderboardEntryPrefab != null)
        {
            entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent);
        }
        else
        {
            // Prefab yoksa otomatik entry oluştur
            entryObj = CreateDefaultEntry(rank, entry);
        }
        
        // Entry component'ini bul veya ekle
        LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
        if (entryUI == null)
        {
            entryUI = entryObj.AddComponent<LeaderboardEntryUI>();
        }
        
        // Entry'yi set et
        entryUI.SetEntry(rank, entry);
    }
    
    private GameObject CreateDefaultEntry(int rank, LeaderboardEntry entry)
    {
        // Ana GameObject
        GameObject entryObj = new GameObject($"Entry_{rank}");
        entryObj.transform.SetParent(leaderboardContent);

        // RectTransform ekle (UI için gerekli)
        RectTransform rectTransform = entryObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, 40); // Yükseklik
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        // Horizontal Layout Group ekle
        UnityEngine.UI.HorizontalLayoutGroup layoutGroup = entryObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = true;

        // Rank Text
        GameObject rankObj = new GameObject("RankText");
        rankObj.transform.SetParent(entryObj.transform);
        TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
        rankText.text = $"{rank}.";
        rankText.fontSize = 18;
        rankText.alignment = TextAlignmentOptions.Left;
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.sizeDelta = new Vector2(50, 0);

        // Name Text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(entryObj.transform);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = entry.playerName;
        nameText.fontSize = 18;
        nameText.alignment = TextAlignmentOptions.Left;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(180, 0);

        // Score Text
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(entryObj.transform);
        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = entry.maxScore.ToString();
        scoreText.fontSize = 18;
        scoreText.alignment = TextAlignmentOptions.Right;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.sizeDelta = new Vector2(80, 0);

        return entryObj;
    }
    
    private void ClearLeaderboard()
    {
        if (leaderboardContent == null) return;

        // MyScoreText'i sakla (eğer Content içinde ise)
        Transform myScoreTransform = null;
        if (myScoreText != null && myScoreText.transform.parent == leaderboardContent)
        {
            myScoreTransform = myScoreText.transform;
        }

        // Tüm child'ları temizle
        foreach (Transform child in leaderboardContent)
        {
            if (child != myScoreTransform) // MyScoreText'i silme
            {
                Destroy(child.gameObject);
            }
        }
    }
}
