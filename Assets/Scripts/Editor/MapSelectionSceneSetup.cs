using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Events;

/// <summary>
/// UI sahnesine harita seçim paneli ve MapSelectionManager kurar.
/// Menü: Tools > Map Selection > Otomatik Kurulum
/// </summary>
public static class MapSelectionSceneSetup
{
    [MenuItem("Tools/Map Selection/Sprite'a göre tıklanabilir alan (seçili butonlara Alpha Hit Test uygula)")]
    public static void ApplyAlphaHitTestToSelection()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Uyarı", "Önce Hierarchy'den bir veya daha fazla buton (veya parent) seç.", "Tamam");
            return;
        }
        int count = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            var images = go.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (var img in images)
            {
                if (img.raycastTarget)
                {
                    img.alphaHitTestMinimumThreshold = 0.5f;
                    count++;
                    EditorUtility.SetDirty(img);
                }
            }
        }
        EditorUtility.DisplayDialog("Tamam", $"Alpha Hit Test Minimum Threshold = 0.5 uygulandı ({count} Image).\n\nKüçük sprite kullanınca tıklanabilir alan sprite şekline göre olur.", "Tamam");
    }

    [MenuItem("Tools/Map Selection/Yeni Harita Butonu Ekle")]
    public static void AddMapButton()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Uyarı", "Play modundan çıkıp tekrar dene.", "Tamam");
            return;
        }

        GameObject panel = GetMapSelectionPanelFromSelection();
        if (panel == null)
        {
            EditorUtility.DisplayDialog("Uyarı",
                "MapSelectionPanel bulunamadı.\n\n" +
                "Hierarchy'de MapSelectionPanel veya içindeki bir objeyi seç, ya da önce Tools > Map Selection > Otomatik Kurulum ile paneli oluştur.",
                "Tamam");
            return;
        }

        MapSelectionButton[] existingMapBtns = panel.GetComponentsInChildren<MapSelectionButton>(true);
        int mapCount = existingMapBtns.Length;
        float newButtonY = 20f - mapCount * 70f;

        GameObject newBtn = CreateButton(panel.transform, "NewMap", new Color(0.2f, 0.5f, 0.8f));
        newBtn.name = "Button_NewMap";
        var text = newBtn.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null) text.text = "NewMap";

        newBtn.AddComponent<MapSelectionButtonHover>().hoverScale = 1.05f;
        MapSelectionButton mapBtnComp = newBtn.AddComponent<MapSelectionButton>();
        mapBtnComp.sceneName = "NewMap";

        RectTransform rect = newBtn.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0, newButtonY);
        rect.sizeDelta = new Vector2(220, 56);

        Button button = newBtn.GetComponent<Button>();
        UnityEventTools.AddVoidPersistentListener(button.onClick,
            (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), mapBtnComp, "OnMapSelected"));

        int totalMapButtons = mapCount + 1;
        Transform back = panel.transform.Find("Button_Geri");
        if (back != null)
        {
            RectTransform backRect = back.GetComponent<RectTransform>();
            if (backRect != null)
                backRect.anchoredPosition = new Vector2(0, -60f - (totalMapButtons - 1) * 70f);
        }

        Undo.RegisterCreatedObjectUndo(newBtn, "Add Map Button");
        Selection.activeGameObject = newBtn;
        EditorUtility.SetDirty(panel);

        EditorUtility.DisplayDialog("Tamam",
            "Yeni harita butonu eklendi.\n\n" +
            "1. Inspector'da Button_NewMap'ı seç.\n" +
            "2. MapSelectionButton > Scene Name'i sahne adına çevir (örn. Efe).\n" +
            "3. İçindeki Text objesinde görünen adı yaz (örn. Efe).\n" +
            "4. Build Settings'e bu sahneyi ekle.",
            "Tamam");
    }

    static GameObject GetMapSelectionPanelFromSelection()
    {
        if (Selection.activeGameObject != null)
        {
            Transform t = Selection.activeGameObject.transform;
            while (t != null)
            {
                if (t.name == "MapSelectionPanel") return t.gameObject;
                t = t.parent;
            }
        }
        var manager = Object.FindObjectOfType<MapSelectionManager>();
        if (manager != null)
        {
            var so = new SerializedObject(manager);
            var prop = so.FindProperty("mapSelectionPanel");
            if (prop != null && prop.objectReferenceValue != null)
                return (GameObject)prop.objectReferenceValue;
        }
        return GameObject.Find("MapSelectionPanel");
    }

    [MenuItem("Tools/Map Selection/Otomatik Kurulum")]
    public static void SetupInScene()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Kurulum", "Oyun çalışırken kurulum yapılamaz. Play modundan çıkın.", "Tamam");
            return;
        }

        Transform parent = FindLocalCanvas();
        if (parent == null)
        {
            EditorUtility.DisplayDialog("Hata", "Sahnedeki UIManager'da localCanvas bulunamadı. UI sahnesi açık mı?", "Tamam");
            return;
        }

        UIManager uiManager = Object.FindObjectOfType<UIManager>();
        CountdownManager countdownManager = Object.FindObjectOfType<CountdownManager>();

        // Mevcut MapSelectionPanel varsa kaldır (tekrar kurulum için)
        MapSelectionManager existing = Object.FindObjectOfType<MapSelectionManager>();
        if (existing != null && existing.gameObject.transform.parent == parent)
        {
            Object.DestroyImmediate(existing.gameObject);
        }
        Transform existingPanel = parent.Find("MapSelectionPanel");
        if (existingPanel != null)
        {
            Object.DestroyImmediate(existingPanel.gameObject);
        }

        // MapSelectionPanel oluştur
        GameObject panel = CreateMapSelectionPanel(parent);
        Undo.RegisterCreatedObjectUndo(panel, "Map Selection Panel");

        // MapSelectionManager objesi (panel ile aynı veya ayrı - panel'in kendisine ekleyebiliriz)
        MapSelectionManager manager = panel.GetComponent<MapSelectionManager>();
        if (manager == null)
            manager = panel.AddComponent<MapSelectionManager>();

        SerializedObject soManager = new SerializedObject(manager);
        soManager.FindProperty("mapSelectionPanel").objectReferenceValue = panel;
        soManager.FindProperty("countdownManager").objectReferenceValue = countdownManager;
        soManager.FindProperty("uiManager").objectReferenceValue = uiManager;
        soManager.ApplyModifiedPropertiesWithoutUndo();

        // UIManager'a panel ve manager ata
        if (uiManager != null)
        {
            SerializedObject soUI = new SerializedObject(uiManager);
            soUI.FindProperty("mapSelectionPanel").objectReferenceValue = panel;
            soUI.FindProperty("mapSelectionManager").objectReferenceValue = manager;
            soUI.ApplyModifiedPropertiesWithoutUndo();
        }

        Selection.activeGameObject = panel;
        EditorUtility.SetDirty(panel);
        if (uiManager != null) EditorUtility.SetDirty(uiManager.gameObject);

        EditorUtility.DisplayDialog(
            "Kurulum Tamamlandı",
            "Harita seçim paneli oluşturuldu.\n\n" +
            "• Play'e basınca önce harita seçim paneli açılacak.\n" +
            "• 'Koray' ile oyun sahnesine, 'Geri' ile ana menüye dönülür.\n" +
            "• Yeni harita eklemek için Inspector'da MapSelectionManager > Maps listesini kullanabilir ve yeni butonları manuel ekleyebilirsin.",
            "Tamam");
    }

    static Transform FindLocalCanvas()
    {
        UIManager uiManager = Object.FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            SerializedObject so = new SerializedObject(uiManager);
            SerializedProperty prop = so.FindProperty("localCanvas");
            if (prop != null && prop.objectReferenceValue != null)
                return (Transform)prop.objectReferenceValue;
        }
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        return canvas != null ? canvas.transform : null;
    }

    static GameObject CreateMapSelectionPanel(Transform parent)
    {
        GameObject panel = new GameObject("MapSelectionPanel");
        panel.transform.SetParent(parent, false);
        panel.SetActive(false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        MapSelectionManager manager = panel.AddComponent<MapSelectionManager>();

        // Başlık
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -24f);
        titleRect.sizeDelta = new Vector2(400, 50);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Harita Seç";
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.raycastTarget = false;

        // Koray butonu
        GameObject korayBtn = CreateButton(panel.transform, "Koray", new Color(0.2f, 0.5f, 0.8f));
        RectTransform korayRect = korayBtn.GetComponent<RectTransform>();
        korayRect.anchorMin = new Vector2(0.5f, 0.5f);
        korayRect.anchorMax = new Vector2(0.5f, 0.5f);
        korayRect.pivot = new Vector2(0.5f, 0.5f);
        korayRect.anchoredPosition = new Vector2(0, 20f);
        korayRect.sizeDelta = new Vector2(220, 56);

        korayBtn.AddComponent<MapSelectionButtonHover>().hoverScale = 1.05f;
        MapSelectionButton mapBtnComp = korayBtn.AddComponent<MapSelectionButton>();
        mapBtnComp.sceneName = "Koray";
        Button korayButton = korayBtn.GetComponent<Button>();
        UnityEventTools.AddVoidPersistentListener(korayButton.onClick,
            (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), mapBtnComp, "OnMapSelected"));

        // Geri butonu (sadece "Geri" yazısı, hover'da hafif büyüme)
        GameObject backBtn = CreateButton(panel.transform, "Geri", new Color(0.4f, 0.35f, 0.35f));
        backBtn.name = "Button_Geri";
        var backText = backBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (backText != null) backText.text = "Geri";
        backBtn.AddComponent<MapSelectionButtonHover>().hoverScale = 1.05f;
        RectTransform backRect = backBtn.GetComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.5f, 0.5f);
        backRect.anchorMax = new Vector2(0.5f, 0.5f);
        backRect.pivot = new Vector2(0.5f, 0.5f);
        backRect.anchoredPosition = new Vector2(0, -60f);
        backRect.sizeDelta = new Vector2(180, 48);

        Button backButton = backBtn.GetComponent<Button>();
        UnityEventTools.AddVoidPersistentListener(backButton.onClick,
            (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), manager, "CloseMapSelection"));

        return panel;
    }

    static GameObject CreateButton(Transform parent, string label, Color color)
    {
        GameObject go = new GameObject("Button_" + label);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 50);

        Image img = go.AddComponent<Image>();
        img.color = color;
        img.alphaHitTestMinimumThreshold = 0.5f;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(go.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return go;
    }
}
