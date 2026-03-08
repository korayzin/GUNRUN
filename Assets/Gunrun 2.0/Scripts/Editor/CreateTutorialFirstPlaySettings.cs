#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Tools > Gunrun > Create Tutorial First Play Settings - Resources'a varsayılan asset oluşturur.
/// Bu asset ile sistemi açıp kapatabilirsiniz.
/// </summary>
public static class CreateTutorialFirstPlaySettings
{
    private const string AssetPath = "Assets/Resources/TutorialFirstPlaySettings.asset";

    [MenuItem("Tools/Gunrun/Create Tutorial First Play Settings")]
    public static void Create()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TutorialFirstPlaySettings>(AssetPath);
        if (existing != null)
        {
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        var settings = ScriptableObject.CreateInstance<TutorialFirstPlaySettings>();
        settings.skipTutorialAfterFirstPlay = true;
        settings.sceneIndexAfterTutorial = 1;

        AssetDatabase.CreateAsset(settings, AssetPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
    }

    [MenuItem("Tools/Gunrun/Reset Tutorial Completed (Editor test için)")]
    public static void ResetTutorialCompleted()
    {
        PlayerPrefs.DeleteKey("TutorialCompleted_Device");
        PlayerPrefs.Save();
        Debug.Log("TutorialCompleted sıfırlandı - bir sonraki Play'de newtutorial açılacak");
    }
}
#endif
