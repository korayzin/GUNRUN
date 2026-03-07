#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// newtutorial sahnesinde TutorialSequenceController kurulumu.
/// Menü: Tools > Gunrun > Setup Newtutorial Scene
/// </summary>
public static class AddTutorialSequenceSetup
{
    [MenuItem("Tools/Gunrun/Setup Newtutorial Scene")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.name.Contains("newtutorial"))
        {
            Debug.LogWarning("Aktif sahne newtutorial değil. newtutorial sahnesini açıp tekrar deneyin.");
            return;
        }

        var wm = Object.FindObjectOfType<WeaponManager>();
        var spawner = Object.FindObjectOfType<AdvancedPortalSpawner>();
        var hud = Object.FindObjectOfType<HolographicWeaponHUD>();
        var td = GameObject.Find("TD");
        TextMeshProUGUI dialogueText = null;
        GameObject dialoguePanel = null;
        AudioSource audioSrc = null;

        if (td != null)
        {
            dialogueText = td.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            dialoguePanel = td.transform.childCount > 0 ? td.transform.GetChild(0).gameObject : td;
            audioSrc = td.GetComponentInChildren<AudioSource>(true);
        }

        Transform rightHand = null;
        Transform leftHand = null;
        var ovrRig = Object.FindObjectOfType<OVRCameraRig>();
        if (ovrRig != null)
        {
            rightHand = ovrRig.transform.Find("TrackingSpace/RightHandAnchor");
            if (rightHand == null) rightHand = ovrRig.transform.Find("RightHandAnchor");
            leftHand = ovrRig.transform.Find("TrackingSpace/LeftHandAnchor");
            if (leftHand == null) leftHand = ovrRig.transform.Find("LeftHandAnchor");
        }

        TutorialSequenceController ctrl = Object.FindObjectOfType<TutorialSequenceController>();
        GameObject go;
        if (ctrl != null)
        {
            go = ctrl.gameObject;
            Debug.Log("TutorialSequenceController zaten mevcut, referanslar güncelleniyor.");
        }
        else
        {
            go = new GameObject("TutorialSequenceManager");
            ctrl = go.AddComponent<TutorialSequenceController>();
            Debug.Log("TutorialSequenceManager oluşturuldu.");
        }

        SerializedObject so = new SerializedObject(ctrl);
        so.FindProperty("weaponManager").objectReferenceValue = wm;
        so.FindProperty("portalSpawner").objectReferenceValue = spawner;
        so.FindProperty("holographicWeaponHUD").objectReferenceValue = hud;
        so.FindProperty("dialogueTextUI").objectReferenceValue = dialogueText;
        so.FindProperty("dialoguePanel").objectReferenceValue = dialoguePanel != null ? dialoguePanel : (dialogueText != null ? dialogueText.gameObject : null);
        so.FindProperty("audioSource").objectReferenceValue = audioSrc;
        so.ApplyModifiedPropertiesWithoutUndo();

        if (wm != null)
        {
            var wmSo = new SerializedObject(wm);
            if (rightHand != null || leftHand != null)
            {
                wmSo.FindProperty("tutorialWeaponChangeVFXParentRight").objectReferenceValue = rightHand;
                wmSo.FindProperty("tutorialWeaponChangeVFXParentLeft").objectReferenceValue = leftHand;
            }
            var vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Gunrun 2.0/VFX 2.0/TutorialWeaponChangeVFX.prefab");
            if (vfxPrefab != null)
            {
                for (int i = 1; i <= 8; i++)
                {
                    var prop = wmSo.FindProperty(GetVFXPropertyName(i));
                    if (prop != null && prop.objectReferenceValue == null)
                        prop.objectReferenceValue = vfxPrefab;
                }
            }
            wmSo.ApplyModifiedPropertiesWithoutUndo();
        }

        if (spawner != null)
        {
            var sp = new SerializedObject(spawner);
            var tm = sp.FindProperty("tutorialMode");
            if (tm != null)
                tm.boolValue = true;
            sp.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("newtutorial sahne kurulumu tamamlandı. Eksik referansları Inspector'dan kontrol edin.");
    }

    static string GetVFXPropertyName(int weaponIndex)
    {
        return weaponIndex switch { 1 => "vfxSecond", 2 => "vfxThird", 3 => "vfxFourth", 4 => "vfxFifth", 5 => "vfxSixth", 6 => "vfxSeventh", 7 => "vfxEighth", 8 => "vfxNinth", _ => "vfxSecond" };
    }
}
#endif
