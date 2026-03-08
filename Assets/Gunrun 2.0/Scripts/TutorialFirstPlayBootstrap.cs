using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Uygulama açılışında, newtutorial'ın sadece ilk kez gösterilmesi için yönlendirme yapar.
/// BeforeSceneLoad'da çalışır - hiçbir sahne yüklenmeden önce.
/// Resources/TutorialFirstPlaySettings'te toggle kapatılırsa bu sistem devre dışı kalır.
/// </summary>
public static class TutorialFirstPlayBootstrap
{
    private const string PrefsKeyTutorialCompleted = "TutorialCompleted_Device";
    private const string NewTutorialSceneName = "newtutorial";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        var settings = Resources.Load<TutorialFirstPlaySettings>("TutorialFirstPlaySettings");
        if (settings != null && !settings.skipTutorialAfterFirstPlay)
            return; // Sistem kapalı - normal akış (newtutorial ile başla)

        // Sistem açık veya settings yok (varsayılan: açık)
        if (PlayerPrefs.GetInt(PrefsKeyTutorialCompleted, 0) == 1)
        {
            int targetIndex = (settings != null) ? settings.sceneIndexAfterTutorial : 1;
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            if (targetIndex >= 0 && targetIndex < sceneCount)
            {
                SceneManager.LoadScene(targetIndex);
            }
        }
    }

    /// <summary>Tutorial tamamlandığında çağrılır. Cihaz bazlı kaydeder.</summary>
    public static void MarkTutorialCompleted()
    {
        PlayerPrefs.SetInt(PrefsKeyTutorialCompleted, 1);
        PlayerPrefs.Save();
    }

    /// <summary>Debug/test için: Tutorial'ı tekrar göstermek üzere sıfırlar.</summary>
    public static void ResetTutorialCompleted()
    {
        PlayerPrefs.DeleteKey(PrefsKeyTutorialCompleted);
        PlayerPrefs.Save();
    }
}
