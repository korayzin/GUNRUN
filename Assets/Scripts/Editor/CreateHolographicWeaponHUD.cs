using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class CreateHolographicWeaponHUD
{
    private const float PanelWidth = 320f;
    private const float PanelHeight = 180f;
    private static readonly Color PanelBg = new Color(0.06f, 0.1f, 0.22f, 0.9f);
    private static readonly Color NeonBlue = new Color(0f, 0.95f, 1f, 1f);
    private static readonly Color NeonBlueGlow = new Color(0f, 0.85f, 1f, 0.5f);

    [MenuItem("Tools/Holographic Weapon HUD/Create in Scene")]
    public static void CreateInScene()
    {
        Transform parent = null;
        GameObject anchorGo = GameObject.Find("LeftHandAnchor");
        if (anchorGo != null) parent = anchorGo.transform;
        if (parent == null)
        {
            anchorGo = GameObject.Find("LeftHand");
            if (anchorGo != null) parent = anchorGo.transform;
        }

        GameObject root = new GameObject("HolographicWeaponHUD");
        Undo.RegisterCreatedObjectUndo(root, "Create Holographic Weapon HUD");

        if (parent != null)
            root.transform.SetParent(parent, false);

        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;
        scaler.referencePixelsPerUnit = 100f;
        scaler.scaleFactor = 1f;
        scaler.referenceResolution = new Vector2(PanelWidth, PanelHeight);

        root.AddComponent<GraphicRaycaster>();

        RectTransform rootRect = root.GetComponent<RectTransform>();
        if (rootRect == null) rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
        rootRect.localScale = Vector3.one * 0.01f;
        rootRect.localPosition = Vector3.zero;
        rootRect.localRotation = Quaternion.identity;

        GameObject panel = CreatePanel(root.transform);
        CreateWarningArea(panel.transform);
        CreateCenterSlot(panel.transform);
        CreateSideSlots(panel.transform);

        HolographicWeaponHUD hud = root.AddComponent<HolographicWeaponHUD>();
        AssignReferences(root, hud);

        if (parent != null)
        {
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("leftHandAnchor").objectReferenceValue = parent;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        Selection.activeGameObject = root;
        if (parent != null)
            Debug.Log("Holographic Weapon HUD created under LeftHandAnchor. Assign WeaponManager on the component.");
        else
            Debug.Log("LeftHandAnchor not found in scene. HUD created at root. Assign LeftHandAnchor and WeaponManager, or parent this under your hand anchor.");
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        GameObject glow = new GameObject("Glow");
        glow.transform.SetParent(panel.transform, false);
        RectTransform glowRect = glow.AddComponent<RectTransform>();
        glowRect.anchorMin = Vector2.zero;
        glowRect.anchorMax = Vector2.one;
        glowRect.offsetMin = new Vector2(-14f, -14f);
        glowRect.offsetMax = new Vector2(14f, 14f);
        Image glowImg = glow.AddComponent<Image>();
        glowImg.color = NeonBlueGlow;
        glowImg.raycastTarget = false;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(panel.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = PanelBg;
        Outline outline = fill.AddComponent<Outline>();
        outline.effectColor = NeonBlue;
        outline.effectDistance = new Vector2(5f, 5f);

        return panel;
    }

    private static void CreateWarningArea(Transform panel)
    {
        GameObject area = new GameObject("WarningArea");
        area.transform.SetParent(panel, false);

        RectTransform rect = area.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(12f, -12f);
        rect.sizeDelta = new Vector2(200f, 24f);

        TextMeshProUGUI tmp = area.AddComponent<TextMeshProUGUI>();
        tmp.text = "HEAVY MODE ANY DESTRUCTOR";
        tmp.fontSize = 12f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;
    }

    private static void CreateCenterSlot(Transform panel)
    {
        GameObject center = new GameObject("CenterWeapon");
        center.transform.SetParent(panel, false);

        RectTransform rect = center.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 10f);
        rect.sizeDelta = new Vector2(120f, 80f);

        Image img = center.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.2f);

        GameObject nameObj = new GameObject("WeaponName");
        nameObj.transform.SetParent(center.transform, false);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 0f);
        nameRect.anchorMax = new Vector2(0.5f, 0f);
        nameRect.pivot = new Vector2(0.5f, 0f);
        nameRect.anchoredPosition = new Vector2(0f, -42f);
        nameRect.sizeDelta = new Vector2(140f, 22f);
        TextMeshProUGUI nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
        nameTmp.text = "WEAPON";
        nameTmp.fontSize = 14f;
        nameTmp.color = Color.white;
        nameTmp.alignment = TextAlignmentOptions.Center;
    }

    private static void CreateSideSlots(Transform panel)
    {
        float slotWidth = 70f;
        float slotHeight = 50f;
        float centerX = PanelWidth * 0.5f;
        float leftX = centerX - 100f;
        float rightX = centerX + 100f;
        float y = PanelHeight * 0.5f - 10f;

        GameObject leftSlot = new GameObject("LeftSlot");
        leftSlot.transform.SetParent(panel, false);
        RectTransform leftRect = leftSlot.AddComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0.5f, 0.5f);
        leftRect.anchorMax = new Vector2(0.5f, 0.5f);
        leftRect.pivot = new Vector2(0.5f, 0.5f);
        leftRect.anchoredPosition = new Vector2(leftX - centerX, y - PanelHeight * 0.5f);
        leftRect.sizeDelta = new Vector2(slotWidth, slotHeight);

        GameObject leftIcon = new GameObject("Icon");
        leftIcon.transform.SetParent(leftSlot.transform, false);
        RectTransform leftIconRect = leftIcon.AddComponent<RectTransform>();
        leftIconRect.anchorMin = Vector2.zero;
        leftIconRect.anchorMax = Vector2.one;
        leftIconRect.offsetMin = Vector2.zero;
        leftIconRect.offsetMax = Vector2.zero;
        Image leftIconImg = leftIcon.AddComponent<Image>();
        leftIconImg.color = new Color(1f, 1f, 1f, 0.6f);

        GameObject leftBtn = new GameObject("LeftButton");
        leftBtn.transform.SetParent(leftSlot.transform, false);
        RectTransform leftBtnRect = leftBtn.AddComponent<RectTransform>();
        leftBtnRect.anchorMin = new Vector2(0.5f, 1f);
        leftBtnRect.anchorMax = new Vector2(0.5f, 1f);
        leftBtnRect.pivot = new Vector2(0.5f, 1f);
        leftBtnRect.anchoredPosition = new Vector2(0f, 8f);
        leftBtnRect.sizeDelta = new Vector2(40f, 28f);
        Image leftBtnImg = leftBtn.AddComponent<Image>();
        leftBtnImg.color = NeonBlue;
        Button leftBtnComp = leftBtn.AddComponent<Button>();

        GameObject rightSlot = new GameObject("RightSlot");
        rightSlot.transform.SetParent(panel, false);
        RectTransform rightRect = rightSlot.AddComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.5f, 0.5f);
        rightRect.anchorMax = new Vector2(0.5f, 0.5f);
        rightRect.pivot = new Vector2(0.5f, 0.5f);
        rightRect.anchoredPosition = new Vector2(rightX - centerX, y - PanelHeight * 0.5f);
        rightRect.sizeDelta = new Vector2(slotWidth, slotHeight);

        GameObject rightIcon = new GameObject("Icon");
        rightIcon.transform.SetParent(rightSlot.transform, false);
        RectTransform rightIconRect = rightIcon.AddComponent<RectTransform>();
        rightIconRect.anchorMin = Vector2.zero;
        rightIconRect.anchorMax = Vector2.one;
        rightIconRect.offsetMin = Vector2.zero;
        rightIconRect.offsetMax = Vector2.zero;
        Image rightIconImg = rightIcon.AddComponent<Image>();
        rightIconImg.color = new Color(1f, 1f, 1f, 0.6f);

        GameObject rightBtn = new GameObject("RightButton");
        rightBtn.transform.SetParent(rightSlot.transform, false);
        RectTransform rightBtnRect = rightBtn.AddComponent<RectTransform>();
        rightBtnRect.anchorMin = new Vector2(0.5f, 1f);
        rightBtnRect.anchorMax = new Vector2(0.5f, 1f);
        rightBtnRect.pivot = new Vector2(0.5f, 1f);
        rightBtnRect.anchoredPosition = new Vector2(0f, 8f);
        rightBtnRect.sizeDelta = new Vector2(40f, 28f);
        Image rightBtnImg = rightBtn.AddComponent<Image>();
        rightBtnImg.color = NeonBlue;
        rightBtn.AddComponent<Button>();
    }

    private static void AssignReferences(GameObject root, HolographicWeaponHUD hud)
    {
        Transform panel = root.transform.Find("Panel");
        if (panel == null) return;

        Transform center = panel.Find("CenterWeapon");
        if (center != null)
        {
            var centerImg = center.GetComponent<Image>();
            if (centerImg != null)
                SerializeSet(hud, "centerWeaponImage", centerImg);
            Transform nameObj = center.Find("WeaponName");
            if (nameObj != null)
            {
                var tmp = nameObj.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                    SerializeSet(hud, "centerWeaponName", tmp);
            }
        }

        Transform warning = panel.Find("WarningArea");
        if (warning != null)
        {
            var tmp = warning.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                SerializeSet(hud, "warningLabel", tmp);
        }

        Transform leftSlot = panel.Find("LeftSlot");
        if (leftSlot != null)
        {
            Transform icon = leftSlot.Find("Icon");
            if (icon != null)
            {
                var img = icon.GetComponent<Image>();
                if (img != null) SerializeSet(hud, "leftSlotImage", img);
            }
            Transform btn = leftSlot.Find("LeftButton");
            if (btn != null)
            {
                var b = btn.GetComponent<Button>();
                if (b != null) SerializeSet(hud, "leftButton", b);
            }
        }

        Transform rightSlot = panel.Find("RightSlot");
        if (rightSlot != null)
        {
            Transform icon = rightSlot.Find("Icon");
            if (icon != null)
            {
                var img = icon.GetComponent<Image>();
                if (img != null) SerializeSet(hud, "rightSlotImage", img);
            }
            Transform btn = rightSlot.Find("RightButton");
            if (btn != null)
            {
                var b = btn.GetComponent<Button>();
                if (b != null) SerializeSet(hud, "rightButton", b);
            }
        }

    }

    private static void SerializeSet(MonoBehaviour target, string fieldName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
