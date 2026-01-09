using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Leaderboard sistemini otomatik kuran Editor tool
/// Context Menu: GameObject > Leaderboard > Auto Setup
/// </summary>
public class LeaderboardAutoSetup : MonoBehaviour
{
    // ============================================================================
    // ÜST MENÜDE GÖRÜNÜR BUTONLAR - TEK TIKLA KURULUM!
    // ============================================================================
    
    [MenuItem("Leaderboard/⚡ TEK TIKLA OTOMATIK KURULUM ⚡", false, 0)]
    static void QuickSetupFromTopMenu()
    {
        bool result = EditorUtility.DisplayDialog(
            "🚀 Otomatik Leaderboard Kurulumu", 
            "Leaderboard sistemi otomatik kurulacak!\n\n" +
            "📍 Kurulum Yeri: InGame panel içi (varsa)\n\n" +
            "Oluşturulacaklar:\n" +
            "✅ LeaderboardPanel (InGame içinde)\n" +
            "✅ ScrollView ve Content (Title, vb.)\n" +
            "✅ LeaderboardUI script (tüm referanslar atanmış)\n" +
            "✅ FirebaseManager\n" +
            "✅ Test butonları\n\n" +
            "🧹 Eski LeaderboardPanel'ler temizlenecek!\n\n" +
            "Devam edilsin mi?",
            "EVET! Kur! 🚀",
            "İptal"
        );
        
        if (result)
        {
            CreateFullLeaderboardSystem();
        }
    }
    
    [MenuItem("Leaderboard/🧹 Eski Leaderboard'ları Temizle", false, 5)]
    static void CleanupOldLeaderboardsMenu()
    {
        bool result = EditorUtility.DisplayDialog(
            "🧹 Eski Leaderboard'ları Temizle", 
            "Tüm eski LeaderboardPanel'ler silinecek!\n\n" +
            "Bu işlem geri alınamaz.\n\n" +
            "Devam edilsin mi?",
            "EVET! Temizle!",
            "İptal"
        );
        
        if (result)
        {
            CleanupOldLeaderboards();
            EditorUtility.DisplayDialog("✅ Başarılı!", "Eski LeaderboardPanel'ler temizlendi!", "Tamam");
        }
    }
    
    [MenuItem("Leaderboard/📊 Mevcut Objeye Ekle (LeaderboardRow için)", false, 1)]
    static void AddToSelectedFromTopMenu()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Uyarı", "Lütfen önce Hierarchy'den bir GameObject seç!", "Tamam");
            return;
        }
        AddLeaderboardUIToSelected();
    }
    
    [MenuItem("Leaderboard/🧪 Test Butonları Ekle", false, 2)]
    static void AddTestButtonsFromTopMenu()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Uyarı", "Lütfen önce LeaderboardPanel'i seç!", "Tamam");
            return;
        }
        AddTestButtons();
    }
    
    [MenuItem("Leaderboard/───────────────────", false, 10)]
    static void Separator1() { }
    
    [MenuItem("Leaderboard/───────────────────", true)]
    static bool Separator1Validate() { return false; }
    
    [MenuItem("Leaderboard/📚 Dokümantasyon/📖 Hızlı Başlangıç", false, 11)]
    static void OpenQuickStart()
    {
        OpenTextFile("Assets/Scripts/README_OTOMATIK_KURULUM.txt");
    }
    
    [MenuItem("Leaderboard/📚 Dokümantasyon/📋 Otomatik Kurulum Rehberi", false, 12)]
    static void OpenAutoSetupGuide()
    {
        OpenTextFile("Assets/Scripts/OTOMATIK_KURULUM_REHBERI.txt");
    }
    
    [MenuItem("Leaderboard/📚 Dokümantasyon/🎯 3 Adımda Kurulum", false, 13)]
    static void OpenThreeStepGuide()
    {
        OpenTextFile("Assets/Scripts/HIZLI_KURULUM_3_ADIM.txt");
    }
    
    static void OpenTextFile(string path)
    {
        if (System.IO.File.Exists(path))
        {
            Application.OpenURL("file://" + System.IO.Path.GetFullPath(path));
        }
        else
        {
            EditorUtility.DisplayDialog("Dosya Bulunamadı", $"Dosya bulunamadı:\n{path}", "Tamam");
        }
    }
    
    [MenuItem("GameObject/Leaderboard/🚀 Otomatik Kurulum (Yeni Panel Oluştur)", false, 0)]
    static void CreateFullLeaderboardSystem()
    {
        Debug.Log("🚀 Leaderboard otomatik kurulum başlıyor...");
        
        // 1. Önce InGame panel'ini bul
        GameObject inGamePanel = FindInGamePanel();
        Transform parentTransform = null;
        
        if (inGamePanel != null)
        {
            parentTransform = inGamePanel.transform;
            Debug.Log($"✅ InGame panel bulundu: {inGamePanel.name}");
        }
        else
        {
            // InGame bulunamazsa Canvas bul veya oluştur
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                Debug.Log("✅ Canvas oluşturuldu");
            }
            parentTransform = canvas.transform;
            Debug.LogWarning("⚠️ InGame panel bulunamadı, Canvas'a ekleniyor");
        }
        
        // 2. Eski LeaderboardPanel'leri temizle
        CleanupOldLeaderboards();
        
        // 3. Yeni LeaderboardPanel oluştur
        GameObject leaderboardPanel = CreateLeaderboardPanel(parentTransform);
        
        // 4. Firebase Manager oluştur
        CreateFirebaseManager();
        
        // 5. Test butonlarını da ekle
        AddTestButtonsInternal(leaderboardPanel);
        
        // 6. Seçimi yeni panel'e ayarla
        Selection.activeGameObject = leaderboardPanel;
        
        Debug.Log("✅✅✅ LEADERBOARD KURULUMU TAMAMLANDI! ✅✅✅");
        Debug.Log($"📊 LeaderboardPanel oluşturuldu: {GetGameObjectPath(leaderboardPanel)}");
        Debug.Log("🎮 Play mode'a geçip test edebilirsin!");
        
        string locationMessage = inGamePanel != null 
            ? $"InGame panel içine kuruldu!" 
            : "Canvas içine kuruldu (InGame bulunamadı)";
        
        EditorUtility.DisplayDialog(
            "🎉 Kurulum Başarılı!", 
            $"Leaderboard sistemi tamamen kuruldu!\n\n" +
            $"📍 Konum: {locationMessage}\n\n" +
            "✅ LeaderboardPanel oluşturuldu\n" +
            "✅ ScrollView ve Content hazır\n" +
            "✅ LeaderboardUI script eklendi\n" +
            "✅ FirebaseManager eklendi\n" +
            "✅ Test butonları eklendi\n\n" +
            "Şimdi yapman gerekenler:\n" +
            "1. Play mode'a geç\n" +
            "2. 'Test Oyuncuları Ekle' butonuna tıkla\n" +
            "3. 2 saniye bekle\n" +
            "4. 'Yenile' butonuna tıkla\n" +
            "5. Oyuncuları gör! 🎉\n\n" +
            "HER ŞEY HAZIR! Tek yapman gereken Play'e basmak!", 
            "Harika! 🚀"
        );
    }
    
    [MenuItem("GameObject/Leaderboard/⚡ Mevcut Objeye LeaderboardUI Ekle (LeaderboardRow için)", false, 1)]
    static void AddLeaderboardUIToSelected()
    {
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen Hierarchy'den bir GameObject seç!", "Tamam");
            return;
        }
        
        Debug.Log($"⚡ {selected.name} objesine LeaderboardUI ekleniyor...");
        
        // LeaderboardUI ekle veya al
        LeaderboardUI leaderboardUI = selected.GetComponent<LeaderboardUI>();
        if (leaderboardUI == null)
        {
            leaderboardUI = selected.AddComponent<LeaderboardUI>();
            Debug.Log("✅ LeaderboardUI component eklendi");
        }
        
        // Leaderboard Panel'i kendine ata
        leaderboardUI.leaderboardPanel = selected;
        
        // ScrollView'ı bul
        ScrollRect scrollRect = selected.GetComponentInChildren<ScrollRect>();
        if (scrollRect != null)
        {
            Transform content = scrollRect.content;
            if (content != null)
            {
                leaderboardUI.leaderboardContent = content;
                
                // VerticalLayoutGroup ekle
                if (content.GetComponent<VerticalLayoutGroup>() == null)
                {
                    VerticalLayoutGroup vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
                    vlg.spacing = 5;
                    vlg.childControlWidth = true;
                    vlg.childControlHeight = false;
                    vlg.childForceExpandWidth = true;
                    vlg.childForceExpandHeight = false;
                    vlg.padding = new RectOffset(10, 10, 10, 10);
                    Debug.Log("✅ VerticalLayoutGroup eklendi");
                }
                
                // ContentSizeFitter ekle
                if (content.GetComponent<ContentSizeFitter>() == null)
                {
                    ContentSizeFitter csf = content.gameObject.AddComponent<ContentSizeFitter>();
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                    Debug.Log("✅ ContentSizeFitter eklendi");
                }
                
                Debug.Log("✅ ScrollView Content otomatik bulundu ve atandı");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ ScrollView bulunamadı! Manuel olarak atamalısın.");
        }
        
        // Text'leri bul ve ata
        TextMeshProUGUI[] texts = selected.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in texts)
        {
            string name = text.gameObject.name.ToLower();
            if ((name.Contains("score") || name.Contains("puan")) && leaderboardUI.myScoreText == null)
            {
                leaderboardUI.myScoreText = text;
                Debug.Log($"✅ My Score Text bulundu: {text.gameObject.name}");
            }
        }
        
        // Firebase Manager oluştur
        CreateFirebaseManager();
        
        EditorUtility.SetDirty(leaderboardUI);
        
        Debug.Log("✅✅✅ KURULUM TAMAMLANDI! ✅✅✅");
        EditorUtility.DisplayDialog(
            "✅ Başarılı!", 
            $"{selected.name} için LeaderboardUI kuruldu!\n\n" +
            "Kontrol et:\n" +
            "✅ LeaderboardUI component eklendi\n" +
            "✅ Leaderboard Panel atandı\n" +
            "✅ Content otomatik bulundu (varsa)\n" +
            "✅ FirebaseManager oluşturuldu\n\n" +
            "Play mode'da test edebilirsin!", 
            "Tamam"
        );
    }
    
    [MenuItem("GameObject/Leaderboard/🔧 Firebase Manager Oluştur", false, 2)]
    static void CreateFirebaseManagerOnly()
    {
        CreateFirebaseManager();
        EditorUtility.DisplayDialog("✅ Başarılı!", "FirebaseManager oluşturuldu!", "Tamam");
    }
    
    [MenuItem("GameObject/Leaderboard/🧪 Test Butonu Ekle", false, 3)]
    static void AddTestButtons()
    {
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen bir GameObject seç (LeaderboardPanel veya Canvas)", "Tamam");
            return;
        }
        
        AddTestButtonsInternal(selected);
        
        EditorUtility.DisplayDialog("✅ Başarılı!", "Test butonları eklendi!\n\nPlay mode'da kullanabilirsin.", "Tamam");
    }
    
    static void AddTestButtonsInternal(GameObject parent)
    {
        Debug.Log("🧪 Test butonları oluşturuluyor...");
        
        // Test Buttons Container
        GameObject buttonContainer = new GameObject("TestButtons");
        buttonContainer.transform.SetParent(parent.transform, false);
        buttonContainer.layer = LayerMask.NameToLayer("UI");
        
        RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 0);
        containerRect.pivot = new Vector2(0.5f, 0);
        containerRect.anchoredPosition = new Vector2(0, 10);
        containerRect.sizeDelta = new Vector2(-20, 60);
        
        HorizontalLayoutGroup hlg = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(10, 10, 10, 10);
        
        // Add Test Players Button
        GameObject addButton = CreateButton(buttonContainer.transform, "➕ Test Oyuncuları Ekle", new Color(0.2f, 0.7f, 0.2f));
        
        // Clear Test Players Button
        GameObject clearButton = CreateButton(buttonContainer.transform, "🗑️ Test Oyuncuları Sil", new Color(0.7f, 0.2f, 0.2f));
        
        // Refresh Button
        GameObject refreshButton = CreateButton(buttonContainer.transform, "🔄 Yenile", new Color(0.2f, 0.4f, 0.8f));
        
        // LeaderboardUI'ye bağla
        LeaderboardUI leaderboardUI = parent.GetComponent<LeaderboardUI>();
        if (leaderboardUI == null)
        {
            leaderboardUI = parent.GetComponentInParent<LeaderboardUI>();
        }
        
        if (leaderboardUI != null)
        {
            leaderboardUI.addTestPlayersButton = addButton.GetComponent<Button>();
            leaderboardUI.clearTestPlayersButton = clearButton.GetComponent<Button>();
            leaderboardUI.refreshButton = refreshButton.GetComponent<Button>();
            EditorUtility.SetDirty(leaderboardUI);
            Debug.Log("✅ Butonlar LeaderboardUI'a bağlandı");
        }
        
        Debug.Log("✅ Test butonları oluşturuldu!");
    }
    
    [MenuItem("GameObject/Leaderboard/📚 Kurulum Rehberini Aç", false, 10)]
    static void OpenSetupGuide()
    {
        string path = "Assets/Scripts/HIZLI_KURULUM_3_ADIM.txt";
        if (System.IO.File.Exists(path))
        {
            Application.OpenURL("file://" + System.IO.Path.GetFullPath(path));
        }
        else
        {
            EditorUtility.DisplayDialog("Dosya Bulunamadı", "HIZLI_KURULUM_3_ADIM.txt dosyası bulunamadı!", "Tamam");
        }
    }
    
    // ============================================================================
    // HELPER METHODS
    // ============================================================================
    
    static GameObject FindInGamePanel()
    {
        // Hierarchy'de InGame panel'ini ara
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            string name = obj.name.ToLower();
            
            // InGame veya InGamePanel isimlerini ara
            if (name == "ingame" || name == "ingamepanel" || name == "in game" || name == "game panel")
            {
                Debug.Log($"🔍 InGame panel bulundu: {obj.name}");
                return obj;
            }
            
            // MainMenuPanel altında InGame ara
            if (name.Contains("mainmenu"))
            {
                Transform inGame = obj.transform.Find("InGame");
                if (inGame != null)
                {
                    Debug.Log($"🔍 MainMenuPanel/InGame bulundu: {inGame.name}");
                    return inGame.gameObject;
                }
            }
        }
        
        // Bulunamazsa path ile ara
        string[] possiblePaths = new string[]
        {
            "Canvas/UIMenu/Panels/MainMenuPanel/InGame",
            "Canvas/MainMenuPanel/InGame",
            "UI/GameMenu/Canvas/UIMenu/Panels/MainMenuPanel/InGame",
            "GameMenu/Canvas/UIMenu/Panels/MainMenuPanel/InGame",
            "Canvas/InGame",
            "UIMenu/Panels/MainMenuPanel/InGame"
        };
        
        foreach (string path in possiblePaths)
        {
            GameObject found = GameObject.Find(path);
            if (found != null)
            {
                Debug.Log($"🔍 InGame panel path ile bulundu: {path}");
                return found;
            }
        }
        
        Debug.LogWarning("⚠️ InGame panel bulunamadı!");
        return null;
    }
    
    static void CleanupOldLeaderboards()
    {
        Debug.Log("🧹 Eski LeaderboardPanel'ler temizleniyor...");
        
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int cleanedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("LeaderboardPanel") || obj.name.Contains("Leaderboard"))
            {
                // LeaderboardUI component'i varsa sil
                if (obj.GetComponent<LeaderboardUI>() != null)
                {
                    Debug.Log($"   🗑️ Siliniyor: {GetGameObjectPath(obj)}");
                    GameObject.DestroyImmediate(obj);
                    cleanedCount++;
                }
            }
        }
        
        if (cleanedCount > 0)
        {
            Debug.Log($"✅ {cleanedCount} eski LeaderboardPanel temizlendi");
        }
        else
        {
            Debug.Log("✅ Temizlenecek eski panel bulunamadı");
        }
    }
    
    static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    static GameObject CreateLeaderboardPanel(Transform parent)
    {
        Debug.Log($"📊 LeaderboardPanel oluşturuluyor... Parent: {parent.name}");
        
        // Ana Panel
        GameObject panel = new GameObject("LeaderboardPanel");
        panel.transform.SetParent(parent, false);
        panel.layer = LayerMask.NameToLayer("UI");
        
        // Varsayılan olarak inactive yap (InGame içindeyse)
        bool isInGame = parent.name.ToLower().Contains("ingame");
        if (isInGame)
        {
            panel.SetActive(false);
            Debug.Log("📊 Panel InGame içinde - varsayılan olarak inactive");
        }
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.9f);
        
        // Title
        GameObject title = new GameObject("Title");
        title.transform.SetParent(panel.transform, false);
        title.layer = LayerMask.NameToLayer("UI");
        
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(-20, 60);
        
        TextMeshProUGUI titleText = title.AddComponent<TextMeshProUGUI>();
        titleText.text = "🏆 LEADERBOARD 🏆";
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        
        // My Score Text
        GameObject myScore = new GameObject("MyScoreText");
        myScore.transform.SetParent(panel.transform, false);
        myScore.layer = LayerMask.NameToLayer("UI");
        
        RectTransform myScoreRect = myScore.AddComponent<RectTransform>();
        myScoreRect.anchorMin = new Vector2(0, 1);
        myScoreRect.anchorMax = new Vector2(1, 1);
        myScoreRect.pivot = new Vector2(0.5f, 1);
        myScoreRect.anchoredPosition = new Vector2(0, -80);
        myScoreRect.sizeDelta = new Vector2(-20, 40);
        
        TextMeshProUGUI myScoreText = myScore.AddComponent<TextMeshProUGUI>();
        myScoreText.text = "🏆 Senin Max Skorun: 0";
        myScoreText.fontSize = 24;
        myScoreText.alignment = TextAlignmentOptions.Center;
        myScoreText.color = new Color(1f, 0.84f, 0f);
        
        // ScrollView
        GameObject scrollView = CreateScrollView(panel.transform);
        
        // Loading Text
        GameObject loading = new GameObject("LoadingText");
        loading.transform.SetParent(panel.transform, false);
        loading.layer = LayerMask.NameToLayer("UI");
        
        RectTransform loadingRect = loading.AddComponent<RectTransform>();
        loadingRect.anchorMin = new Vector2(0.5f, 0.5f);
        loadingRect.anchorMax = new Vector2(0.5f, 0.5f);
        loadingRect.pivot = new Vector2(0.5f, 0.5f);
        loadingRect.anchoredPosition = Vector2.zero;
        loadingRect.sizeDelta = new Vector2(400, 60);
        
        TextMeshProUGUI loadingText = loading.AddComponent<TextMeshProUGUI>();
        loadingText.text = "Yükleniyor...";
        loadingText.fontSize = 28;
        loadingText.alignment = TextAlignmentOptions.Center;
        loadingText.color = Color.yellow;
        loading.SetActive(false);
        
        // LeaderboardUI Component Ekle
        LeaderboardUI leaderboardUI = panel.AddComponent<LeaderboardUI>();
        leaderboardUI.leaderboardPanel = panel;
        leaderboardUI.leaderboardContent = scrollView.transform.Find("Viewport/Content");
        leaderboardUI.loadingText = loadingText;
        leaderboardUI.myScoreText = myScoreText;
        leaderboardUI.maxEntries = 50;
        
        Debug.Log("✅ LeaderboardPanel oluşturuldu ve yapılandırıldı");
        
        return panel;
    }
    
    static GameObject CreateScrollView(Transform parent)
    {
        Debug.Log("📜 ScrollView oluşturuluyor...");
        
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(parent, false);
        scrollView.layer = LayerMask.NameToLayer("UI");
        
        RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(20, 80);
        scrollRect.offsetMax = new Vector2(-20, -130);
        
        Image scrollImage = scrollView.AddComponent<Image>();
        scrollImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        scroll.inertia = true;
        scroll.scrollSensitivity = 1.0f;
        
        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        viewport.layer = LayerMask.NameToLayer("UI");
        
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        scroll.viewport = viewportRect;
        
        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        content.layer = LayerMask.NameToLayer("UI");
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0, 0);
        
        VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        
        ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        
        scroll.content = contentRect;
        
        Debug.Log("✅ ScrollView oluşturuldu");
        
        return scrollView;
    }
    
    static void CreateFirebaseManager()
    {
        // Zaten var mı kontrol et
        FirebaseLeaderboardManager existing = Object.FindObjectOfType<FirebaseLeaderboardManager>();
        if (existing != null)
        {
            Debug.Log("✅ FirebaseManager zaten mevcut");
            return;
        }
        
        GameObject manager = new GameObject("FirebaseManager");
        manager.AddComponent<FirebaseLeaderboardManager>();
        
        Debug.Log("✅ FirebaseManager oluşturuldu");
    }
    
    static GameObject CreateButton(Transform parent, string text, Color color)
    {
        GameObject button = new GameObject(text);
        button.transform.SetParent(parent, false);
        button.layer = LayerMask.NameToLayer("UI");
        
        RectTransform rect = button.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100, 50);
        
        Image image = button.AddComponent<Image>();
        image.color = color;
        
        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = image;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        textObj.layer = LayerMask.NameToLayer("UI");
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        return button;
    }
}
