using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Tek merkezden müzik ve SFX ses seviyelerini yönetir.
/// Deneme/UI sahnesindeki slider'lar bu script üzerinden ayar yapar; Koray sahnesinde müzik ve tüm silah/hit sesleri bu ayarlara uyar.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("MusicManager");
            go.AddComponent<MusicManager>();
        }
    }

    private const string PrefsMusic = "MusicVolume";
    private const string PrefsSfx = "SfxVolume";

    [Header("Slider bağlantıları (Deneme/Options)")]
    [Tooltip("Müzik slider'ı - atanırsa değer buradan okunur/güncellenir")]
    public Slider musicSlider;
    [Tooltip("SFX slider'ı - atanırsa değer buradan okunur/güncellenir")]
    public Slider sfxSlider;

    [Header("SFX ön izleme (Deneme sahnesinde)")]
    [Tooltip("SFX slider hareket ettirildiğinde çalınacak kısa ses (ne kıstığını duyarsın)")]
    public AudioClip sfxPreviewClip;
    [Tooltip("Ön izleme sesi için minimum aralık (saniye) - çok sık çalmasın")]
    public float sfxPreviewCooldown = 0.15f;

    [Header("Menü arka plan müziği (Deneme sahnesi)")]
    [Tooltip("Deneme sahnesinde çalacak arka plan müzik clip'i. Boş bırakılırsa sadece sahnedeki MusicSource'lar kullanılır.")]
    public AudioClip menuBackgroundMusicClip;

    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;
    private readonly List<AudioSource> _musicSources = new List<AudioSource>();
    private AudioSource _menuMusicSource;
    private AudioSource _sfxOneShotSource;
    private float _lastSfxPreviewTime = -999f;

    public float MusicVolume => _musicVolume;
    public float SfxVolume => _sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Sahnedeki MusicManager'ın config'ini mevcut Instance'a aktar (menuBackgroundMusicClip, sfxPreviewClip vb.)
            Instance.ReceiveConfigFromDuplicate(menuBackgroundMusicClip, sfxPreviewClip, musicSlider, sfxSlider);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicVolume = PlayerPrefs.GetFloat(PrefsMusic, 1f);
        _sfxVolume = PlayerPrefs.GetFloat(PrefsSfx, 1f);

        _sfxOneShotSource = gameObject.AddComponent<AudioSource>();
        _sfxOneShotSource.playOnAwake = false;
        _sfxOneShotSource.loop = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Sahne değişince yüklenen duplicate MusicManager'dan config al (menu clip, slider refs vb.)
    /// </summary>
    private void ReceiveConfigFromDuplicate(AudioClip menuClip, AudioClip sfxPreview, Slider musSlider, Slider sfxSl)
    {
        if (menuClip != null) menuBackgroundMusicClip = menuClip;
        if (sfxPreview != null) sfxPreviewClip = sfxPreview;
        if (musSlider != null) musicSlider = musSlider;
        if (sfxSl != null) sfxSlider = sfxSl;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this)
            Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindSliders(); // Sahne değişince slider refs eski sahneye aittir, yeniden bul
        ApplyMusicVolumeToAll();
        EnsureMenuMusic();
    }

    private void Start()
    {
        ApplyMusicVolumeToAll();
        EnsureMenuMusic();
        BindSliders();
    }

    private void OnEnable()
    {
        BindSliders();
    }

    private void BindSliders()
    {
        // Slider'lar Options panelinde olabilir (kapalı/inactive) - GameObject.Find inactive bulamaz
        if (musicSlider == null)
        {
            musicSlider = FindSliderByName("MusicSlider");
        }
        if (sfxSlider == null)
        {
            sfxSlider = FindSliderByName("SoundSlider");
        }
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(_musicVolume);
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(_sfxVolume);
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
    }

    private void EnsureMenuMusic()
    {
        if (menuBackgroundMusicClip == null) return;
        string name = SceneManager.GetActiveScene().name;
        bool isMenuScene = name == "Deneme" || name == "UI" || name.Contains("Menu");
        if (!isMenuScene)
        {
            StopMenuMusic();
            return;
        }
        if (_menuMusicSource == null)
        {
            GameObject go = new GameObject("MenuBackgroundMusic");
            go.transform.SetParent(transform);
            _menuMusicSource = go.AddComponent<AudioSource>();
            _menuMusicSource.clip = menuBackgroundMusicClip;
            _menuMusicSource.loop = true;
            _menuMusicSource.playOnAwake = false;
            RegisterMusicSource(_menuMusicSource);
        }
        _menuMusicSource.volume = _musicVolume;
        if (!_menuMusicSource.isPlaying)
            _menuMusicSource.Play();
    }

    private void StopMenuMusic()
    {
        if (_menuMusicSource != null && _menuMusicSource.isPlaying)
        {
            UnregisterMusicSource(_menuMusicSource);
            _menuMusicSource.Stop();
        }
    }

    // ---- Müzik ----

    public void RegisterMusicSource(AudioSource source)
    {
        if (source == null || _musicSources.Contains(source)) return;
        _musicSources.Add(source);
        source.volume = _musicVolume;
    }

    public void UnregisterMusicSource(AudioSource source)
    {
        if (source == null) return;
        _musicSources.Remove(source);
    }

    public void SetMusicVolume(float value)
    {
        _musicVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PrefsMusic, _musicVolume);
        PlayerPrefs.Save();
        ApplyMusicVolumeToAll();
        if (musicSlider != null && Mathf.Abs(musicSlider.value - _musicVolume) > 0.001f)
            musicSlider.SetValueWithoutNotify(_musicVolume);
    }

    private void ApplyMusicVolumeToAll()
    {
        for (int i = _musicSources.Count - 1; i >= 0; i--)
        {
            if (_musicSources[i] == null)
            {
                _musicSources.RemoveAt(i);
                continue;
            }
            _musicSources[i].volume = _musicVolume;
        }
        if (_menuMusicSource != null)
            _menuMusicSource.volume = _musicVolume;
    }

    public void ApplyMusicVolumeTo(AudioSource source)
    {
        if (source == null) return;
        source.volume = _musicVolume;
        if (!_musicSources.Contains(source))
            _musicSources.Add(source);
    }

    // ---- SFX ----

    public float GetSfxVolume()
    {
        return _sfxVolume;
    }

    public void SetSfxVolume(float value)
    {
        _sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PrefsSfx, _sfxVolume);
        PlayerPrefs.Save();
        if (sfxSlider != null && Mathf.Abs(sfxSlider.value - _sfxVolume) > 0.001f)
            sfxSlider.SetValueWithoutNotify(_sfxVolume);
    }

    private void OnSfxSliderChanged(float value)
    {
        SetSfxVolume(value);
    }

    /// <summary>
    /// SFX slider bırakıldığında çağrılır - sadece bu anda ön izleme sesi çalar.
    /// </summary>
    public void PlaySfxPreviewOnRelease()
    {
        if (sfxPreviewClip == null) return;
        if (Time.unscaledTime - _lastSfxPreviewTime < sfxPreviewCooldown) return;
        _lastSfxPreviewTime = Time.unscaledTime;
        if (_sfxOneShotSource != null)
            _sfxOneShotSource.PlayOneShot(sfxPreviewClip, _sfxVolume);
    }

    private void PlaySfxPreview()
    {
        if (_sfxOneShotSource == null || sfxPreviewClip == null) return;
        _sfxOneShotSource.PlayOneShot(sfxPreviewClip, _sfxVolume);
    }

    /// <summary>
    /// Merkezi SFX çalma - tüm silah/hit sesleri bu üzerinden gidebilir; SFX volume otomatik uygulanır.
    /// </summary>
    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        float vol = _sfxVolume * Mathf.Clamp01(volumeScale);
        if (vol <= 0f) return;
        if (_sfxOneShotSource != null)
            _sfxOneShotSource.PlayOneShot(clip, vol);
        else
            AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, vol);
    }

    /// <summary>
    /// 3D konumda SFX çalar (hit, mermi vb.); SFX volume uygulanır.
    /// </summary>
    public void PlaySfxAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        float vol = _sfxVolume * Mathf.Clamp01(volumeScale);
        if (vol <= 0f) return;
        AudioSource.PlayClipAtPoint(clip, position, vol);
    }

    /// <summary>
    /// Mevcut bir AudioSource ile one-shot çal; volume SFX ayarına göre ayarlanır.
    /// </summary>
    public void PlaySfxFromSource(AudioSource source, AudioClip clip, float volumeScale = 1f)
    {
        if (source == null || clip == null) return;
        float vol = _sfxVolume * Mathf.Clamp01(volumeScale);
        if (vol <= 0f) return;
        source.PlayOneShot(clip, vol);
    }

    /// <summary>
    /// Loop veya tek atım için kullanılan bir AudioSource'ın volume'ünü SFX ayarına göre günceller.
    /// Çalmadan önce çağır.
    /// </summary>
    public void ApplySfxVolumeTo(AudioSource source)
    {
        if (source != null)
            source.volume = _sfxVolume;
    }

    /// <summary>
    /// İsimle slider bul (inactive objeler dahil - Options panel kapalıyken de çalışır).
    /// </summary>
    private static Slider FindSliderByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        var sliders = Object.FindObjectsOfType<Slider>(true);
        foreach (var s in sliders)
        {
            if (s != null && s.gameObject != null && s.gameObject.name == name)
                return s;
        }
        return null;
    }

    /// <summary>
    /// Slider bağlantılarını yeniden dene (Options panel kapalıyken slider'lar bulunamazsa, panel açıldığında çağır).
    /// </summary>
    public void RefreshSliderBindings()
    {
        BindSliders();
    }
}

/// <summary>
/// Bu component'i müzik çalan bir GameObject'e ekle; MusicManager otomatik kaydeder ve volume'ü günceller.
/// UI/Koray sahnesindeki arka plan müziği ve game music için kullan.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicSource : MonoBehaviour
{
    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (MusicManager.Instance != null && _source != null)
            MusicManager.Instance.RegisterMusicSource(_source);
    }

    private void OnDestroy()
    {
        if (MusicManager.Instance != null && _source != null)
            MusicManager.Instance.UnregisterMusicSource(_source);
    }
}
