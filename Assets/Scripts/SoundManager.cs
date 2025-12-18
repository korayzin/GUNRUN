using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    private const string VolumeKey = "GameVolume";
    public float volumeStep = 0.1f;
    public Image[] volumeSegments;

    public Color activeColor = Color.cyan; 
    public Color inactiveColor = Color.gray;

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = savedVolume;

        UpdateVolumeUI(savedVolume);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bullet")) return;

        if (CompareTag("IncreaseSound"))
        {
            IncreaseVolume();
        }
        else if (CompareTag("DecreaseSound"))
        {
            DecreaseVolume();
        }

        Destroy(other.gameObject);
    }

    public void IncreaseVolume()
    {
        float newVolume = Mathf.Clamp(AudioListener.volume + volumeStep, 0f, 1f);
        SetVolume(newVolume);
    }

    public void DecreaseVolume()
    {
        float newVolume = Mathf.Clamp(AudioListener.volume - volumeStep, 0f, 1f);
        SetVolume(newVolume);
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat(VolumeKey, value);
        PlayerPrefs.Save();

        UpdateVolumeUI(value);
        Debug.Log("Ses seviyesi: " + value);
    }

    private void UpdateVolumeUI(float volume)
    {
        if (volumeSegments != null)
        {
            int activeBars = Mathf.RoundToInt(volume * volumeSegments.Length);

            for (int i = 0; i < volumeSegments.Length; i++)
            {
                volumeSegments[i].color = i < activeBars ? activeColor : inactiveColor;
            }
        }
    }
}
