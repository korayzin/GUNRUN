using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance;

    public TextMeshProUGUI countdownText;
    private float countdownTime = 5f;

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
        float timeLeft = countdownTime;
        while (timeLeft > 0)
        {
            countdownText.text = timeLeft.ToString("F0");
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        StartGame();
    }

    private void StartGame()
    {
        Debug.Log("Oyun baþladý!");
        // Buraya sahne yükleme kodunu ekleyebilirsin
    }
}
