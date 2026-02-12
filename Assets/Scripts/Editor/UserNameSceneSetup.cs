using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEditor.Events;
using TMPro;

public static class UserNameSceneSetup
{
    private const float RefWidth = 1920f;
    private const float RefHeight = 1080f;
    private const float CanvasDistance = 3f;
    private const float CanvasScale = 0.002f;

    [MenuItem("Tools/UserName Scene/Setup Canvas and EventSystem")]
    public static void SetupCanvasAndEventSystem()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Kurulum", "Oyun çalışırken kurulum yapılamaz. Play modundan çıkın.", "Tamam");
            return;
        }

        CreateEventSystemIfNeeded();
        Transform canvasTransform = CreateCanvasWorldSpace();
        CreateFullUserNameUI(canvasTransform);
        EditorUtility.DisplayDialog("Kurulum tamamlandı",
            "World Space Canvas ve raycast ile tıklanabilir butonlar oluşturuldu.\n" +
            "Fare: sol tık. VR: tetikleyici. Butonlara ray ile tıklayın.", "Tamam");
    }

    /// <summary>
    /// UserName sahnesinde VR kontrolcü ray + buton tıklama. Tek script, başka hiçbir şeye bağlı değil.
    /// </summary>
    [MenuItem("Tools/UserName Scene/Setup VR Ray Click (Basit)")]
    public static void SetupVRRayClick()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("VR Ray Click", "Play modundan çıkıp tekrar dene.", "Tamam");
            return;
        }

        UserNameRayClick existing = Object.FindObjectOfType<UserNameRayClick>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("VR Ray Click", "UserNameRayClick zaten sahnede. Bu obje kullanılıyor.", "Tamam");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        GameObject go = new GameObject("UserNameRayClick");
        Undo.RegisterCreatedObjectUndo(go, "UserName VR Ray Click");
        go.AddComponent<LineRenderer>();
        go.AddComponent<UserNameRayClick>();

        EditorUtility.DisplayDialog("VR Ray Click eklendi",
            "UserNameRayClick objesi oluşturuldu.\n\n" +
            "• Sahneyi kaydet (Ctrl+S), VR'da çalıştır.\n" +
            "• Kontrolcüden çıkan ray ile butona bak, tetikleyiciye bas = tıklama.", "Tamam");
        Selection.activeGameObject = go;
    }

    private static void CreateEventSystemIfNeeded()
    {
        if (Object.FindObjectOfType<EventSystem>() != null) return;
        GameObject es = new GameObject("EventSystem");
        Undo.RegisterCreatedObjectUndo(es, "UserName Scene Setup");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    private static Transform CreateCanvasWorldSpace()
    {
        GameObject canvasGo = new GameObject("Canvas");
        Undo.RegisterCreatedObjectUndo(canvasGo, "UserName Scene Setup");

        RectTransform canvasRect = canvasGo.AddComponent<RectTransform>();
        canvasRect.anchorMin = new Vector2(0.5f, 0.5f);
        canvasRect.anchorMax = new Vector2(0.5f, 0.5f);
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.anchoredPosition = Vector2.zero;
        canvasRect.sizeDelta = new Vector2(RefWidth, RefHeight);
        canvasRect.localPosition = new Vector3(0, 0, CanvasDistance);
        canvasRect.localScale = new Vector3(CanvasScale, CanvasScale, CanvasScale);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        canvasGo.AddComponent<GraphicRaycaster>();

        return canvasGo.transform;
    }

    private static void CreateFullUserNameUI(Transform canvasTransform)
    {
        GameObject titleGo = CreateText(canvasTransform, "TitleText", "Kullanıcı adınız", 42, 0, -60, 500, 50);
        Undo.RegisterCreatedObjectUndo(titleGo, "UserName UI");

        GameObject inputGo = CreateInputField(canvasTransform, 0, -140, 450, 48);
        Undo.RegisterCreatedObjectUndo(inputGo, "UserName UI");

        GameObject randomGo = CreateButtonWithCollider(canvasTransform, "RandomButton", "Random", 0, -220, 300, 44, UserNameButtonHit.Action.Random);
        Undo.RegisterCreatedObjectUndo(randomGo, "UserName UI");

        GameObject confirmGo = CreateButtonWithCollider(canvasTransform, "ConfirmButton", "Devam", 0, -330, 320, 52, UserNameButtonHit.Action.Confirm);
        Undo.RegisterCreatedObjectUndo(confirmGo, "UserName UI");

        GameObject controllerGo = new GameObject("UserNameController");
        controllerGo.transform.SetParent(canvasTransform, false);
        Undo.RegisterCreatedObjectUndo(controllerGo, "UserName UI");

        UserNameController controller = controllerGo.AddComponent<UserNameController>();
        SerializedObject so = new SerializedObject(controller);

        so.FindProperty("nextSceneName").stringValue = "UI";
        so.FindProperty("nameInputLegacy").objectReferenceValue = inputGo.GetComponent<InputField>();
        so.FindProperty("randomButton").objectReferenceValue = randomGo.GetComponent<Button>();
        so.FindProperty("confirmButton").objectReferenceValue = confirmGo.GetComponent<Button>();

        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(randomGo.GetComponent<Button>().onClick, (UnityAction)controller.OnRandomClick);
        UnityEventTools.AddPersistentListener(confirmGo.GetComponent<Button>().onClick, (UnityAction)controller.OnConfirmClick);
    }

    private static GameObject CreateText(Transform parent, string name, string text, int fontSize, float posX, float posY, float width, float height)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        SetRectTopCenter(rect, posX, posY, width, height);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return go;
    }

    private static GameObject CreateInputField(Transform parent, float posX, float posY, float width, float height)
    {
        GameObject go = new GameObject("NameInputField");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        SetRectTopCenter(rect, posX, posY, width, height);
        Image bg = go.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        InputField input = go.AddComponent<InputField>();
        input.characterLimit = 20;

        GameObject placeholderGo = new GameObject("Placeholder");
        placeholderGo.transform.SetParent(go.transform, false);
        RectTransform phRect = placeholderGo.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = new Vector2(12, 8);
        phRect.offsetMax = new Vector2(-12, -8);
        Text phText = placeholderGo.AddComponent<Text>();
        phText.text = "İsminizi girin...";
        phText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        phText.fontSize = 22;
        phText.font = GetDefaultFont();
        input.placeholder = phText;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12, 8);
        textRect.offsetMax = new Vector2(-12, -8);
        Text textComp = textGo.AddComponent<Text>();
        textComp.text = "";
        textComp.color = Color.white;
        textComp.fontSize = 22;
        textComp.font = GetDefaultFont();
        textComp.supportRichText = false;
        input.textComponent = textComp;

        return go;
    }

    private static GameObject CreateButtonWithCollider(Transform parent, string goName, string label, float posX, float posY, float width, float height, UserNameButtonHit.Action hitAction)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        SetRectTopCenter(rect, posX, posY, width, height);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.5f, 0.9f, 1f);
        img.raycastTarget = true;

        Button btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = new Color(0.25f, 0.5f, 0.9f, 1f);
        colors.highlightedColor = new Color(0.35f, 0.6f, 1f, 1f);
        colors.pressedColor = new Color(0.2f, 0.4f, 0.8f, 1f);
        btn.colors = colors;

        BoxCollider box = go.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(width, height, 100f);
        box.center = Vector3.zero;

        UserNameButtonHit hitTarget = go.AddComponent<UserNameButtonHit>();
        hitTarget.action = hitAction;

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return go;
    }

    private static void SetRectTopCenter(RectTransform rect, float posX, float posY, float width, float height)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static Font GetDefaultFont()
    {
        Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }
}
