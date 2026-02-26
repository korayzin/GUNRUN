using UnityEngine;

/// <summary>
/// Harita butonuna tıklandığında MapSelectionManager.SelectMap(sceneName) çağrılır.
/// Inspector'da sceneName atanır; Editor kurulumunda otomatik bağlanır.
/// </summary>
public class MapSelectionButton : MonoBehaviour
{
    [Tooltip("Build Settings'teki sahne adı")]
    public string sceneName = "Koray";

    public void OnMapSelected()
    {
        var manager = FindObjectOfType<MapSelectionManager>();
        if (manager != null)
            manager.SelectMap(sceneName);
    }
}
