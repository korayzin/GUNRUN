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
    public GameObject leaderboardHeaderPrefab;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI myScoreText;
    public Button refreshButton;
    
    [Header("Settings")]
    public int maxEntries = 50;
    
    [Header("Debug Buttons (Optional)")]
    public Button addTestPlayersButton;
    public Button clearTestPlayersButton;
    
    private void Start()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(LoadLeaderboard);
        }
        
        // Debug butonları
        if (addTestPlayersButton != null)
        {
            addTestPlayersButton.onClick.AddListener(AddTestPlayers);
        }
        
        if (clearTestPlayersButton != null)
        {
            clearTestPlayersButton.onClick.AddListener(ClearTestPlayers);
        }
        
        // Eksik referansları otomatik bul
        AutoFindReferences();
        
        // UI scene'inde otomatik açılması için
        // Kısa bir delay ile aç (UI'ların tam yüklenmesi için)
        StartCoroutine(ShowLeaderboardOnStart());
    }
    
    private System.Collections.IEnumerator ShowLeaderboardOnStart()
    {
        // UI'ların tam yüklenmesi için kısa bir bekleme
        yield return new WaitForSeconds(0.1f);
        
        // Leaderboard'u göster
        ShowLeaderboard();
        Debug.Log("✅ Leaderboard otomatik olarak açıldı (Start metodundan)");
    }
    
    // TEST FONKSIYONU: Test oyuncuları ekle
    public void AddTestPlayers()
    {
        if (FirebaseLeaderboardManager.Instance != null)
        {
            Debug.Log("🧪 10 test oyuncusu ekleniyor...");
            FirebaseLeaderboardManager.Instance.AddTestPlayers(10);
            
            // 3 saniye sonra leaderboard'u yenile
            StartCoroutine(RefreshAfterDelay(3f));
        }
    }
    
    // TEST FONKSIYONU: Test oyuncuları temizle
    public void ClearTestPlayers()
    {
        if (FirebaseLeaderboardManager.Instance != null)
        {
            Debug.Log("🧹 Test oyuncuları temizleniyor...");
            FirebaseLeaderboardManager.Instance.ClearTestPlayers();
            
            // 2 saniye sonra leaderboard'u yenile
            StartCoroutine(RefreshAfterDelay(2f));
        }
    }
    
    private System.Collections.IEnumerator RefreshAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadLeaderboard();
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
        Debug.Log("🎯 ShowLeaderboard() çağrıldı");
        
        // Önce referansları tekrar kontrol et
        AutoFindReferences();

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            Debug.Log($"✅ Leaderboard panel aktif edildi: {leaderboardPanel.name}");

            // Canvas kontrolü
            Canvas canvas = leaderboardPanel.GetComponentInParent<Canvas>();
            if (canvas != null && !canvas.enabled)
            {
                canvas.enabled = true;
                Debug.Log("✅ Canvas enabled");
            }

            // Kısa bir delay sonra load et (UI'ın aktif olması için)
            Debug.Log("⏳ LoadLeaderboard delay başlıyor...");
            StartCoroutine(LoadLeaderboardDelayed());
        }
        else
        {
            // Hata durumunda basit bir mesaj göster
            Debug.LogError("❌ LEADERBOARD PANEL BULUNAMADI! Unity'de LeaderboardUI kurulumunu kontrol et.");
            Debug.LogError("❌ AutoFindReferences çalıştırıldı ama panel hala null!");
        }
    }
    
    private System.Collections.IEnumerator LoadLeaderboardDelayed()
    {
        Debug.Log("⏳ LoadLeaderboardDelayed - 0.5 saniye bekleniyor...");
        yield return new WaitForSecondsRealtime(0.5f);
        Debug.Log("✅ Delay bitti, şimdi LoadLeaderboard() çağrılıyor...");
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
        Debug.Log("📊📊📊 LoadLeaderboard() ÇAĞRILDI! 📊📊📊");
        Debug.Log($"📊 leaderboardContent: {(leaderboardContent != null ? leaderboardContent.name : "NULL")}");
        Debug.Log($"📊 leaderboardPanel: {(leaderboardPanel != null ? leaderboardPanel.name : "NULL")}");
        
        // Instance kontrolü
        if (FirebaseLeaderboardManager.Instance == null)
        {
            Debug.LogError("❌ FirebaseLeaderboardManager Instance bulunamadı! Scene'de FirebaseLeaderboardManager GameObject'i var mı kontrol et.");
            if (loadingText != null)
            {
                loadingText.text = "Firebase bağlantısı yok!";
                loadingText.gameObject.SetActive(true);
            }
            return;
        }
        
        Debug.Log("✅ FirebaseLeaderboardManager Instance bulundu!");
        
        if (loadingText != null)
        {
            loadingText.text = "Firebase'den oyuncular yükleniyor...";
            loadingText.gameObject.SetActive(true);
            Debug.Log("✅ Loading text ayarlandı");
        }
        
        // Mevcut entry'leri temizle
        ClearLeaderboard();
        Debug.Log("🧹 Eski leaderboard entry'leri temizlendi");
        
        // Leaderboard'u çek
        Debug.Log($"📡 Firebase'den leaderboard çekiliyor... (maxEntries: {maxEntries})");
        FirebaseLeaderboardManager.Instance.GetLeaderboard((entries) =>
        {
            Debug.Log($"📊 CALLBACK! Firebase'den veri geldi: {entries?.Count ?? 0} oyuncu bulundu");
            
            if (entries == null)
            {
                Debug.LogWarning("⚠️ Entries NULL geldi!");
            }
            else if (entries.Count == 0)
            {
                Debug.LogWarning("⚠️ Entries boş liste (Count = 0)!");
            }
            else
            {
                Debug.Log($"✅ {entries.Count} oyuncu verisi başarıyla alındı!");
                for (int i = 0; i < Mathf.Min(3, entries.Count); i++)
                {
                    Debug.Log($"   #{i+1}: {entries[i].playerName} - {entries[i].maxScore}");
                }
            }
            
            DisplayLeaderboard(entries);
            
            if (loadingText != null)
            {
                loadingText.gameObject.SetActive(false);
                Debug.Log("✅ Loading text gizlendi");
            }
        }, maxEntries);
        
        // Kendi max score'unu göster
        Debug.Log("📡 Kendi max score çekiliyor...");
        FirebaseLeaderboardManager.Instance.GetMyMaxScore((myScore) =>
        {
            Debug.Log($"✅ CALLBACK! Kendi max score: {myScore}");
            
            if (myScoreText != null)
            {
                myScoreText.text = $"🏆 YOUR HIGH SCORE: {myScore}";
                myScoreText.gameObject.SetActive(true);
                Debug.Log($"✅ MyScoreText güncellendi: {myScoreText.gameObject.name}");
            }
            else
            {
                // Otomatik bulmaya çalış
                Debug.LogWarning("⚠️ myScoreText NULL! Otomatik aranıyor...");
                FindMyScoreText();
                if (myScoreText != null)
                {
                    myScoreText.text = $"🏆 YOUR HIGH SCORE: {myScore}";
                    myScoreText.gameObject.SetActive(true);
                    Debug.Log($"✅ MyScoreText otomatik bulundu ve güncellendi: {myScoreText.gameObject.name}");
                }
                else
                {
                    Debug.LogError("❌ myScoreText bulunamadı!");
                }
            }
        });
        
        Debug.Log("📊 LoadLeaderboard() fonksiyonu tamamlandı (callback'ler beklemede)");
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
            // Veri yoksa bilgilendirici mesaj göster
            if (loadingText != null)
            {
                loadingText.text = "📊 Henüz leaderboard'da başka oyuncu yok!\nOyunu oynayın ve ilk olmaya çalışın! 🏆";
                loadingText.gameObject.SetActive(true);
            }
            Debug.LogWarning("⚠️ Firebase'den leaderboard verisi gelmedi veya boş!");
            return;
        }

        Debug.Log($"📊 {entries.Count} oyuncu leaderboard'da gösteriliyor:");
        
        // Content Layout Group ayarlarını kontrol et ve ayarla
        SetupContentLayoutGroup();

        // Başlık ekle
        CreateHeaderEntry();

        // Tüm entry'leri göster
        for (int i = 0; i < entries.Count; i++)
        {
            LeaderboardEntry entry = entries[i];
            Debug.Log($"   {i + 1}. {entry.playerName} - {entry.maxScore} puan");
            CreateLeaderboardEntry(i + 1, entry);
        }
        
        Debug.Log($"✅ Leaderboard başarıyla gösterildi! Toplam {entries.Count} oyuncu");
    }
    
    private void SetupContentLayoutGroup()
    {
        if (leaderboardContent == null) return;
        
        // Vertical Layout Group ekle veya güncelle
        UnityEngine.UI.VerticalLayoutGroup layoutGroup = leaderboardContent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = leaderboardContent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            Debug.Log("✅ VerticalLayoutGroup eklendi");
        }
        
        layoutGroup.spacing = 5;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        
        // Content Size Fitter ekle veya güncelle
        UnityEngine.UI.ContentSizeFitter sizeFitter = leaderboardContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = leaderboardContent.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            Debug.Log("✅ ContentSizeFitter eklendi");
        }
        
        sizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;
    }

    private void CreateHeaderEntry()
    {
        GameObject headerObj;
        
        if (leaderboardHeaderPrefab != null)
        {
            // Prefab varsa kullan
            headerObj = Instantiate(leaderboardHeaderPrefab, leaderboardContent, false);
            headerObj.name = "LeaderboardHeader";
            Debug.Log($"✅ Header prefab kullanılarak oluşturuldu");
        }
        else
        {
            // Prefab yoksa otomatik header oluştur
            headerObj = CreateDefaultHeader();
            Debug.Log($"⚠️ Otomatik header oluşturuldu (prefab yok)");
        }
    }
    
    private GameObject CreateDefaultHeader()
    {
        GameObject headerObj = new GameObject("LeaderboardHeader");
        headerObj.transform.SetParent(leaderboardContent, false);

        // RectTransform ayarla
        RectTransform rectTransform = headerObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, 50);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        // Arka plan ekle
        UnityEngine.UI.Image background = headerObj.AddComponent<UnityEngine.UI.Image>();
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Horizontal Layout Group ekle
        UnityEngine.UI.HorizontalLayoutGroup layoutGroup = headerObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layoutGroup.spacing = 15;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = true;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;

        // Rank Header
        GameObject rankObj = new GameObject("RankHeader");
        rankObj.transform.SetParent(headerObj.transform, false);
        TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
        rankText.text = "SIRA";
        rankText.fontSize = 18;
        rankText.fontStyle = FontStyles.Bold;
        rankText.color = new Color(0.9f, 0.9f, 0.9f);
        rankText.alignment = TextAlignmentOptions.Center;
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.sizeDelta = new Vector2(70, 0);

        // Name Header
        GameObject nameObj = new GameObject("NameHeader");
        nameObj.transform.SetParent(headerObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "OYUNCU ADI";
        nameText.fontSize = 18;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = new Color(0.9f, 0.9f, 0.9f);
        nameText.alignment = TextAlignmentOptions.Left;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(200, 0);

        // Score Header
        GameObject scoreObj = new GameObject("ScoreHeader");
        scoreObj.transform.SetParent(headerObj.transform, false);
        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "PUAN";
        scoreText.fontSize = 18;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.color = new Color(0.9f, 0.9f, 0.9f);
        scoreText.alignment = TextAlignmentOptions.Right;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.sizeDelta = new Vector2(100, 0);

        // Layout Element ekle
        UnityEngine.UI.LayoutElement layoutElement = headerObj.AddComponent<UnityEngine.UI.LayoutElement>();
        layoutElement.minHeight = 50;
        layoutElement.preferredHeight = 50;
        
        return headerObj;
    }
    
    private void CreateLeaderboardEntry(int rank, LeaderboardEntry entry)
    {
        GameObject entryObj;
        
        if (leaderboardEntryPrefab != null)
        {
            // Prefab varsa kullan
            entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContent, false);
            Debug.Log($"✅ Prefab kullanılarak entry oluşturuldu: {rank}. {entry.playerName}");
        }
        else
        {
            // Prefab yoksa otomatik entry oluştur
            entryObj = CreateDefaultEntry(rank, entry);
            Debug.Log($"⚠️ Otomatik entry oluşturuldu (prefab yok): {rank}. {entry.playerName}");
        }
        
        // Entry component'ini bul veya ekle
        LeaderboardEntryUI entryUI = entryObj.GetComponent<LeaderboardEntryUI>();
        if (entryUI == null)
        {
            entryUI = entryObj.AddComponent<LeaderboardEntryUI>();
            Debug.Log($"   LeaderboardEntryUI component eklendi");
        }
        
        // Entry'yi set et
        entryUI.SetEntry(rank, entry);
    }
    
    private GameObject CreateDefaultEntry(int rank, LeaderboardEntry entry)
    {
        // Ana GameObject
        GameObject entryObj = new GameObject($"Entry_{rank}_{entry.playerName}");
        entryObj.transform.SetParent(leaderboardContent, false);

        // RectTransform ekle (UI için gerekli)
        RectTransform rectTransform = entryObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, 45); // Yükseklik
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        // Arka plan rengi ekle (alternatif satırlar için)
        UnityEngine.UI.Image background = entryObj.AddComponent<UnityEngine.UI.Image>();
        background.color = (rank % 2 == 0) ? new Color(0.2f, 0.2f, 0.2f, 0.3f) : new Color(0.15f, 0.15f, 0.15f, 0.3f);

        // Horizontal Layout Group ekle
        UnityEngine.UI.HorizontalLayoutGroup layoutGroup = entryObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        layoutGroup.spacing = 15;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = true;
        layoutGroup.padding = new RectOffset(10, 10, 5, 5);
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;

        // Özel renk için ilk 3 sırayı vurgula
        Color textColor = Color.white;
        string rankPrefix = "";
        if (rank == 1)
        {
            textColor = new Color(1f, 0.84f, 0f); // Altın
            rankPrefix = "🥇 ";
        }
        else if (rank == 2)
        {
            textColor = new Color(0.75f, 0.75f, 0.75f); // Gümüş
            rankPrefix = "🥈 ";
        }
        else if (rank == 3)
        {
            textColor = new Color(0.8f, 0.5f, 0.2f); // Bronz
            rankPrefix = "🥉 ";
        }

        // Rank Text
        GameObject rankObj = new GameObject("RankText");
        rankObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
        rankText.text = $"{rankPrefix}{rank}.";
        rankText.fontSize = 20;
        rankText.fontStyle = FontStyles.Bold;
        rankText.color = textColor;
        rankText.alignment = TextAlignmentOptions.Center;
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.sizeDelta = new Vector2(70, 0);

        // Name Text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = entry.playerName;
        nameText.fontSize = 18;
        nameText.color = textColor;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.fontStyle = FontStyles.Normal;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(200, 0);

        // Score Text
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = $"{entry.maxScore:N0}"; // Binlik ayırıcılarla
        scoreText.fontSize = 20;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.color = textColor;
        scoreText.alignment = TextAlignmentOptions.Right;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.sizeDelta = new Vector2(100, 0);

        // Layout Element ekle (scroll için önemli)
        UnityEngine.UI.LayoutElement layoutElement = entryObj.AddComponent<UnityEngine.UI.LayoutElement>();
        layoutElement.minHeight = 45;
        layoutElement.preferredHeight = 45;

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
