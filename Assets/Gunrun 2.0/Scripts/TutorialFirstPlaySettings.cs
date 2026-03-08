using UnityEngine;

/// <summary>
/// newtutorial sahnesinin sadece ilk oynanışta açılması sistemini kontrol eder.
/// Resources/TutorialFirstPlaySettings asset olarak oluşturulur. Toggle burada.
/// </summary>
[CreateAssetMenu(fileName = "TutorialFirstPlaySettings", menuName = "Gunrun/Tutorial First Play Settings")]
public class TutorialFirstPlaySettings : ScriptableObject
{
    [Tooltip("Açık: Tutorial bir kez tamamlandıktan sonra bir sonraki açılışta newtutorial atlanır, Build Settings'teki sıradaki sahneyle başlanır. Kapalı: Her zaman newtutorial ile başlar.")]
    public bool skipTutorialAfterFirstPlay = true;

    [Tooltip("Build Settings'te newtutorial'dan sonraki sahnenin index'i (örn. UserName=1). Build sırası değişirse güncelle.")]
    public int sceneIndexAfterTutorial = 1;
}
