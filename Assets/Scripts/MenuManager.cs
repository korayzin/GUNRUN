using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuManager : MonoBehaviour
{
    [Header("Canvas'lar")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject infoCanvas;
    [SerializeField] private GameObject optionsCanvas;

    [Header("Sahne Ayarlarý")]
    [SerializeField] private string playSceneName;

    [Header("Butonlar")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button backButton;

    [Header("Hover Ayarlarý")]
    [SerializeField] private Color hoverColor = Color.yellow; 
    [SerializeField] private Color defaultColor = Color.white; 
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float defaultScale = 1f; 

    private Button currentHoveredButton;
    private Button lastHoveredButton;

    private void Update()
    {
        DetectButtonHover();

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            if (currentHoveredButton != null)
            {
                currentHoveredButton.onClick.Invoke();
            }
        }
    }

    private void DetectButtonHover()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Button button = hit.collider.GetComponent<Button>();
            if (button != null)
            {
                if (currentHoveredButton != button)
                {
                    ResetButtonVisual(lastHoveredButton);

                    currentHoveredButton = button;
                    ApplyHoverEffect(currentHoveredButton);
                    lastHoveredButton = currentHoveredButton;
                }
            }
            else
            {
                ResetButtonVisual(lastHoveredButton);
                currentHoveredButton = null;
            }
        }
        else
        {
            ResetButtonVisual(lastHoveredButton);
            currentHoveredButton = null;
        }
    }

    private void ApplyHoverEffect(Button button)
    {
        if (button == null) return;

        var buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }

        button.transform.localScale = Vector3.one * hoverScale;
    }

    private void ResetButtonVisual(Button button)
    {
        if (button == null) return;

        var buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = defaultColor;
        }

        button.transform.localScale = Vector3.one * defaultScale;
    }

    public void OnPlayButtonPressed()
    {
        if (!string.IsNullOrEmpty(playSceneName))
        {
            SceneManager.LoadScene(playSceneName);
        }
        else
        {
            Debug.LogWarning("Play sahne adý ayarlanmadý!");
        }
    }

    public void OnInfoButtonPressed()
    {
        mainMenuCanvas.SetActive(false);
        infoCanvas.SetActive(true);
    }

    public void OnOptionsButtonPressed()
    {
        mainMenuCanvas.SetActive(false);
        optionsCanvas.SetActive(true);
    }

    public void OnBackToMainMenuButtonPressed()
    {
        infoCanvas.SetActive(false);
        optionsCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }
}
