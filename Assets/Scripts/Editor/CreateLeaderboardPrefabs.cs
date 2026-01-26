using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class CreateLeaderboardPrefabs : EditorWindow
{
    [MenuItem("Tools/Leaderboard/Create Default Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<CreateLeaderboardPrefabs>("Create Leaderboard Prefabs");
    }

    private void OnGUI()
    {
        GUILayout.Label("Varsayılan Leaderboard Prefab'ları Oluştur", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Bu araç, kodun otomatik oluşturduğu varsayılan header ve entry tasarımlarını prefab olarak oluşturur. " +
            "Oluşturulan prefab'ları Unity'de düzenleyebilir ve LeaderboardUI script'ine atayabilirsiniz.",
            MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("Header ve Entry Prefab'larını Oluştur", GUILayout.Height(40)))
        {
            CreatePrefabs();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Prefab'lar 'Assets/Prefabs/Leaderboard/' klasörüne kaydedilecek. " +
            "Eğer klasör yoksa otomatik oluşturulacak.",
            MessageType.None);
    }

    private static void CreatePrefabs()
    {
        // Klasör oluştur
        string folderPath = "Assets/Prefabs/Leaderboard";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            // Assets/Prefabs klasörünü kontrol et
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            AssetDatabase.CreateFolder("Assets/Prefabs", "Leaderboard");
        }

        // Canvas oluştur (UI elementleri için gerekli)
        GameObject tempCanvas = new GameObject("TempCanvas");
        Canvas canvas = tempCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        tempCanvas.AddComponent<CanvasScaler>();
        tempCanvas.AddComponent<GraphicRaycaster>();

        // Header Prefab oluştur
        GameObject headerPrefab = CreateHeaderPrefab(tempCanvas.transform);
        string headerPath = $"{folderPath}/LeaderboardHeaderPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(headerPrefab, headerPath);
        Debug.Log($"✅ Header prefab oluşturuldu: {headerPath}");

        // Entry Prefab oluştur
        GameObject entryPrefab = CreateEntryPrefab(tempCanvas.transform);
        string entryPath = $"{folderPath}/LeaderboardEntryPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(entryPrefab, entryPath);
        Debug.Log($"✅ Entry prefab oluşturuldu: {entryPath}");

        // Geçici canvas'ı sil
        DestroyImmediate(tempCanvas);

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Başarılı!",
            $"Prefab'lar başarıyla oluşturuldu!\n\n" +
            $"Header: {headerPath}\n" +
            $"Entry: {entryPath}\n\n" +
            $"Şimdi LeaderboardUI script'ine bu prefab'ları atayabilirsiniz.",
            "Tamam");
    }

    private static GameObject CreateHeaderPrefab(Transform parent)
    {
        GameObject headerObj = new GameObject("LeaderboardHeader");

        // RectTransform ayarla
        RectTransform rectTransform = headerObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 50);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        // Arka plan ekle
        Image background = headerObj.AddComponent<Image>();
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // Horizontal Layout Group ekle
        HorizontalLayoutGroup layoutGroup = headerObj.AddComponent<HorizontalLayoutGroup>();
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
        LayoutElement layoutElement = headerObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 50;
        layoutElement.preferredHeight = 50;

        return headerObj;
    }

    private static GameObject CreateEntryPrefab(Transform parent)
    {
        GameObject entryObj = new GameObject("LeaderboardEntry");

        // RectTransform ekle (UI için gerekli)
        RectTransform rectTransform = entryObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 45);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);

        // Arka plan rengi ekle
        Image background = entryObj.AddComponent<Image>();
        background.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);

        // Horizontal Layout Group ekle
        HorizontalLayoutGroup layoutGroup = entryObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 15;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = true;
        layoutGroup.padding = new RectOffset(10, 10, 5, 5);
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;

        // Rank Text
        GameObject rankObj = new GameObject("RankText");
        rankObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
        rankText.text = "1."; // Örnek metin
        rankText.fontSize = 20;
        rankText.fontStyle = FontStyles.Bold;
        rankText.color = Color.white;
        rankText.alignment = TextAlignmentOptions.Center;
        RectTransform rankRect = rankObj.GetComponent<RectTransform>();
        rankRect.sizeDelta = new Vector2(70, 0);

        // Name Text
        GameObject nameObj = new GameObject("NameText");
        nameObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = "Oyuncu Adı"; // Örnek metin
        nameText.fontSize = 18;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Left;
        nameText.fontStyle = FontStyles.Normal;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.sizeDelta = new Vector2(200, 0);

        // Score Text
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(entryObj.transform, false);
        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "1,234"; // Örnek metin
        scoreText.fontSize = 20;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.color = Color.white;
        scoreText.alignment = TextAlignmentOptions.Right;
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.sizeDelta = new Vector2(100, 0);

        // Layout Element ekle (scroll için önemli)
        LayoutElement layoutElement = entryObj.AddComponent<LayoutElement>();
        layoutElement.minHeight = 45;
        layoutElement.preferredHeight = 45;

        // LeaderboardEntryUI component'i ekle
        LeaderboardEntryUI entryUI = entryObj.AddComponent<LeaderboardEntryUI>();

        return entryObj;
    }
}
