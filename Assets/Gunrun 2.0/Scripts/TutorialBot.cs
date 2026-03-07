using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Tutorial diyalog botu. Karşımızda spawn olur, diyaloğunu (text) gösterir, dissolve ile kaybolur.
/// Sonraki diyalogda tekrar dissolve ile gelir. Modüler: her diyalog için Show -> SetText -> ... -> Hide.
/// Diyalog metni bot içindeki TextMeshPro'da gösterilir.
/// </summary>
public class TutorialBot : MonoBehaviour
{
    [Header("Metin")]
    [Tooltip("Diyalog metninin yazılacağı TMP. Boşsa Canvas > Panel altındaki TMP aranır.")]
    public TextMeshProUGUI dialogueText;
    [Tooltip("World space kullanıyorsan TextMeshPro (3D) da atanabilir")]
    public TMP_Text dialogueTextFallback;
    [Tooltip("Panel (Canvas içinde). Boşsa Canvas altında 'Panel' adlı obje aranır.")]
    public RectTransform dialoguePanel;

    [Header("Spawn / Dissolve")]
    [Tooltip("Bot spawn noktası. Boşsa Camera.main önüne veya mevcut pozisyona yerleşir.")]
    public Transform spawnPoint;
    [Tooltip("Oyuncu/kamera - spawn mesafesi ve yönü için. Boşsa spawnPoint kullanılır.")]
    public Transform lookAtTarget;
    [Tooltip("lookAtTarget kullanılıyorsa bu mesafede, önümüze spawn olur")]
    public float spawnDistanceInFront = 2.5f;
    [Tooltip("Dissolve ile geliş süresi (saniye)")]
    public float spawnDissolveDuration = 0.5f;
    [Tooltip("Dissolve ile gidiş süresi (saniye)")]
    public float despawnDissolveDuration = 0.4f;

    [Header("Fallback (Dissolve shader yoksa)")]
    [Tooltip("Custom/WeaponDissolve yoksa CanvasGroup ile fade kullanılır")]
    public CanvasGroup fadeCanvasGroup;

    private WeaponDissolveEffect _dissolveEffect;
    private bool _isVisible;
    private bool _useFadeFallback;

    public bool IsVisible => _isVisible;

    private void Awake()
    {
        ResolveDialogueText();
        if (SceneManager.GetActiveScene().name != "newtutorial") return;

        _dissolveEffect = GetComponentInChildren<WeaponDissolveEffect>(true);
        if (_dissolveEffect == null) _dissolveEffect = GetComponent<WeaponDissolveEffect>();
        if (fadeCanvasGroup == null) fadeCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (fadeCanvasGroup == null) fadeCanvasGroup = GetComponent<CanvasGroup>();

        _useFadeFallback = (_dissolveEffect == null);
        gameObject.SetActive(false);
    }

    /// <summary>Diyalog metnini Canvas > Panel içindeki TMP'ye çözer. Panel üstündeki texte yazılır.</summary>
    private void ResolveDialogueText()
    {
        if (dialogueText != null) return;

        if (dialoguePanel != null)
        {
            dialogueText = dialoguePanel.GetComponentInChildren<TextMeshProUGUI>(true);
            if (dialogueText == null) dialogueTextFallback = dialoguePanel.GetComponentInChildren<TMP_Text>(true);
            return;
        }

        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            Transform panel = null;
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Transform c = canvas.transform.GetChild(i);
                if (c.name.Contains("Panel"))
                {
                    panel = c;
                    break;
                }
            }
            if (panel == null && canvas.transform.childCount > 0)
                panel = canvas.transform.GetChild(0);

            if (panel != null)
            {
                dialogueText = panel.GetComponentInChildren<TextMeshProUGUI>(true);
                if (dialogueText == null) dialogueTextFallback = panel.GetComponentInChildren<TMP_Text>(true);
            }
        }

        if (dialogueText == null) dialogueText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (dialogueText == null && dialogueTextFallback == null) dialogueTextFallback = GetComponentInChildren<TMP_Text>(true);
    }

    /// <summary>Controller çağırabilir: diyalog text referansı yoksa tekrar çözer.</summary>
    public void EnsureDialogueTextResolved()
    {
        if (dialogueText != null || dialogueTextFallback != null) return;
        ResolveDialogueText();
    }

    /// <summary>Diyalog metnini botun text bileşenine yazar.</summary>
    public void SetDialogueText(string text)
    {
        string t = text ?? "";
        if (dialogueText != null) dialogueText.text = t;
        if (dialogueTextFallback != null) dialogueTextFallback.text = t;
    }

    /// <summary>Mevcut diyalog metnini döner (typing için).</summary>
    public string GetDialogueText()
    {
        if (dialogueText != null) return dialogueText.text;
        if (dialogueTextFallback != null) return dialogueTextFallback.text;
        return "";
    }

    /// <summary>Typling için tek karakter ekler.</summary>
    public void AppendDialogueChar(char c)
    {
        string current = GetDialogueText();
        SetDialogueText(current + c);
    }

    /// <summary>Botu spawn noktasına yerleştirir (pozisyon). Dissolve veya fade ile görünür yapar.</summary>
    public void PlaceAndShow()
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        else if (lookAtTarget != null)
        {
            Vector3 fwd = lookAtTarget.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f) fwd = lookAtTarget.forward;
            fwd.Normalize();
            transform.position = lookAtTarget.position + fwd * spawnDistanceInFront;
            transform.LookAt(lookAtTarget.position + Vector3.up * transform.position.y);
        }
        gameObject.SetActive(true);
        _isVisible = true;
    }

    /// <summary>Dissolve/fade ile gelir. PlaceAndShow + animasyon.</summary>
    public IEnumerator ShowWithDissolve()
    {
        PlaceAndShow();
        SetDialogueText("");

        if (!_useFadeFallback && _dissolveEffect != null)
        {
            yield return _dissolveEffect.PlaySpawn(spawnDissolveDuration);
            yield break;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            float d = spawnDissolveDuration;
            for (float t = 0; t < d; t += Time.unscaledDeltaTime)
            {
                fadeCanvasGroup.alpha = Mathf.Clamp01(t / d);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }
    }

    /// <summary>Dissolve/fade ile kaybolur, sonra objeyi kapatır.</summary>
    public IEnumerator HideWithDissolve()
    {
        if (!_isVisible) yield break;

        if (!_useFadeFallback && _dissolveEffect != null)
        {
            yield return _dissolveEffect.PlayDespawn(despawnDissolveDuration);
        }
        else if (fadeCanvasGroup != null)
        {
            float d = despawnDissolveDuration;
            for (float t = 0; t < d; t += Time.unscaledDeltaTime)
            {
                fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(t / d);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }

        _isVisible = false;
        gameObject.SetActive(false);
    }
}
