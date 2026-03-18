using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance;

    public TextMeshProUGUI countdownText;
    [Tooltip("Ger�ek geri say?m s�resi (saniye)")]
    public float countdownTime = 5f;
    [Tooltip("Countdown sonras? y�klenecek sahne ad?")]
    public string sceneToLoad = "Koray";

    private void Awake()
    {
        Instance = this;
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        if (countdownText) countdownText.gameObject.SetActive(true);

        var tooltipMgr = CountdownTooltipManager.Instance;
        if (tooltipMgr != null) tooltipMgr.BeginCountdownTooltips(countdownTime);

        float timeLeft = countdownTime;
        while (timeLeft > 0)
        {
            int num = Mathf.RoundToInt(timeLeft);
            if (countdownText) countdownText.text = num.ToString();
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        if (tooltipMgr != null) tooltipMgr.StopCountdownTooltips();
        if (countdownText) countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneManager.LoadScene(sceneToLoad);
    }
}
