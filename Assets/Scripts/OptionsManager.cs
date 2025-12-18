using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource soundSource;
    public float volumeStep = 0.1f;

    public void IncreaseMusicVolume()
    {
        musicSource.volume = Mathf.Clamp(musicSource.volume + volumeStep, 0f, 1f);
        PlayerPrefs.SetFloat("MusicVolume", musicSource.volume);
    }

    public void DecreaseMusicVolume()
    {
        musicSource.volume = Mathf.Clamp(musicSource.volume - volumeStep, 0f, 1f);
        PlayerPrefs.SetFloat("MusicVolume", musicSource.volume);
    }

    public void IncreaseSoundVolume()
    {
        soundSource.volume = Mathf.Clamp(soundSource.volume + volumeStep, 0f, 1f);
        PlayerPrefs.SetFloat("SoundVolume", soundSource.volume);
    }

    public void DecreaseSoundVolume()
    {
        soundSource.volume = Mathf.Clamp(soundSource.volume - volumeStep, 0f, 1f);
        PlayerPrefs.SetFloat("SoundVolume", soundSource.volume);
    }
}
