using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject weaponPanel;
    public GameObject bestScoresPanel;
    public GameObject countdownPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        ActivatePanel(mainMenuPanel);
    }

    public void ShowOptions()
    {
        ActivatePanel(optionsPanel);
    }

    public void ShowWeapons()
    {
        ActivatePanel(weaponPanel);
    }

    public void ShowBestScores()
    {
        ActivatePanel(bestScoresPanel);
    }

    public void StartGameCountdown()
    {
        ActivatePanel(countdownPanel);
        CountdownManager.Instance.StartCountdown();
    }

    private void ActivatePanel(GameObject panel)
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        weaponPanel.SetActive(false);
        bestScoresPanel.SetActive(false);
        countdownPanel.SetActive(false);

        panel.SetActive(true);
    }
}
