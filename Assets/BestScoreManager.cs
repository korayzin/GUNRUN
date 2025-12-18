using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BestScoreManager : MonoBehaviour
{
    public TextMeshProUGUI[] bestScoreTexts;
    public TextMeshProUGUI[] bestScoreDates; 

    private void Start()
    {
        LoadBestScores();
    }

    private void LoadBestScores()
    {
        List<ScoreEntry> bestScores = GetBestScores();

        for (int i = 0; i < bestScoreTexts.Length; i++)
        {
            if (i < bestScores.Count)
            {
                bestScoreTexts[i].text = bestScores[i].score.ToString();
                bestScoreDates[i].text = bestScores[i].date; 
            }
            else
            {
                bestScoreTexts[i].text = "0"; 
                bestScoreDates[i].text = "--/--/----"; 
            }
        }
    }

    private List<ScoreEntry> GetBestScores()
    {
        string json = PlayerPrefs.GetString("BestScores", "");
        if (!string.IsNullOrEmpty(json))
        {
            return JsonUtility.FromJson<ScoreList>(json).scores;
        }
        return new List<ScoreEntry>();
    }
}
