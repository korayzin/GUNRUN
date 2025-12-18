using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AudioSettingsController : MonoBehaviour
{
    [Header("Ses Ayarlarý")]
    [SerializeField] private AudioSource musicSource; 
    [SerializeField] private AudioSource soundSource; 

    [Header("Ses Kontrolleri")]
    [Range(0f, 1f), SerializeField] private float musicVolume = 0.5f; 
    [Range(0f, 1f), SerializeField] private float soundVolume = 0.5f;

    [Header("UI Butonlarý")]
    [SerializeField] private Button musicMinusButton;
    [SerializeField] private Button musicPlusButton;
    [SerializeField] private Button soundMinusButton;
    [SerializeField] private Button soundPlusButton;

    [Header("UI Göstergeleri")]
    [SerializeField] private TMP_Text musicVolumeText; 
    [SerializeField] private TMP_Text soundVolumeText; 

    private void Start()
    {
        UpdateAudioVolumes();

        musicMinusButton.onClick.AddListener(() => AdjustMusicVolume(-0.1f));
        musicPlusButton.onClick.AddListener(() => AdjustMusicVolume(0.1f));
        soundMinusButton.onClick.AddListener(() => AdjustSoundVolume(-0.1f));
        soundPlusButton.onClick.AddListener(() => AdjustSoundVolume(0.1f));
    }

    private void AdjustMusicVolume(float amount)
    {
        musicVolume = Mathf.Clamp(musicVolume + amount, 0f, 1f); 
        UpdateAudioVolumes();
    }

    private void AdjustSoundVolume(float amount)
    {
        soundVolume = Mathf.Clamp(soundVolume + amount, 0f, 1f); 
        UpdateAudioVolumes();
    }

    private void UpdateAudioVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }

        if (soundSource != null)
        {
            soundSource.volume = soundVolume;
        }

        if (musicVolumeText != null)
        {
            musicVolumeText.text = $"Music: {(int)(musicVolume * 100)}%";
        }

        if (soundVolumeText != null)
        {
            soundVolumeText.text = $"Sound: {(int)(soundVolume * 100)}%";
        }
    }
}
