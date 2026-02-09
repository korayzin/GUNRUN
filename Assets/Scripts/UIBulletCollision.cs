using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class UIBulletCollision : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject infoPanel;
    public GameObject optionsPanel;
    public TextMeshProUGUI countdownText;

    private void Start()
    {
        if (countdownText) countdownText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Bullet")) return;

        Debug.Log("Bullet hit: " + this.gameObject.name);

        if (gameObject.CompareTag("PlayButton"))
        {
            StartCoroutine(StartGameCountdown());
        }
        else if (gameObject.CompareTag("InfoButton"))
        {
            ShowPanel(infoPanel);
        }
        else if (gameObject.CompareTag("OptionsButton"))
        {
            ShowPanel(optionsPanel);
        }
        else if (gameObject.CompareTag("BackButton"))
        {
            ShowPanel(mainMenuPanel);
        }
        else if (gameObject.CompareTag("TrainingLabButton"))
        {
            Debug.Log("Loading AimLab scene...");
            SceneManager.LoadScene("AimLab");
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (infoPanel) infoPanel.SetActive(false);
        if (optionsPanel) optionsPanel.SetActive(false);

        if (panelToShow) panelToShow.SetActive(true);
    }

    private IEnumerator StartGameCountdown()
    {
        if (countdownText)
        {
            countdownText.gameObject.SetActive(true);

            DeactivatePanelAndChildren(mainMenuPanel);
            DeactivatePanelAndChildren(infoPanel);
            DeactivatePanelAndChildren(optionsPanel);

            for (int i = 5; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }

            countdownText.text = "GO!";
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene("Koray");
        }
        else
        {
            yield return new WaitForSeconds(5f);
            SceneManager.LoadScene("Koray");
        }
    }

    private void DeactivatePanelAndChildren(GameObject panel)
    {
        if (panel != null)
        {
            var image = panel.GetComponent<Image>();
            var collider = panel.GetComponent<Collider>();
            var textMeshPro = panel.GetComponent<TextMeshProUGUI>();
            if (image) image.enabled = false;
            if (collider) collider.enabled = false;
            if (textMeshPro) textMeshPro.enabled = false;

            DisableComponentsInChildren(panel.transform);
        }
    }

    private void DisableComponentsInChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            var childImage = child.GetComponent<Image>();
            var childCollider = child.GetComponent<Collider>();
            var childTextMeshPro = child.GetComponent<TextMeshProUGUI>();

            if (childImage) childImage.enabled = false;
            if (childCollider) childCollider.enabled = false;
            if (childTextMeshPro) childTextMeshPro.enabled = false;

            DisableComponentsInChildren(child);
        }
    }
}
