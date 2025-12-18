using System.Collections;
using UnityEngine;
using TMPro;

public class AimLabManager : MonoBehaviour
{
    public GameObject tutorialPanel;  
    public GameObject chooseGunPanel; 
    public TextMeshProUGUI tutorialText; 

    private string[] tutorialSentences =
    {
        "Before you start \"GUNRUN\"",
        "Get to know your weapons,",
        "Sharpen your reflexes",
        "Choose your weapon, shoot at targets,",
        "And warm up for the best performance.",
        "If you ready",
        "Join the battle"
    };

    public float typingSpeed = 0.05f;

    private void Start()
    {
        StartCoroutine(StartTutorial());
    }

    private IEnumerator StartTutorial()
    {
        tutorialPanel.SetActive(true);
        chooseGunPanel.SetActive(false);
        tutorialText.text = "";

        foreach (string sentence in tutorialSentences)
        {
            yield return StartCoroutine(TypeSentence(sentence));
            yield return new WaitForSeconds(0.8f);
        }

        tutorialPanel.SetActive(false);
        chooseGunPanel.SetActive(true);
    }

    private IEnumerator TypeSentence(string sentence)
    {
        tutorialText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            tutorialText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
