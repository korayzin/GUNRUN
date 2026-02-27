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
        if (bestScoreTexts == null || bestScoreDates == null) return;

        List<ScoreEntry> bestScores = GetBestScores();
        int textCount = bestScoreTexts.Length;
        int dateCount = bestScoreDates != null ? bestScoreDates.Length : 0;

        for (int i = 0; i < textCount; i++)
        {
            if (bestScoreTexts[i] != null)
            {
                bestScoreTexts[i].text = (i < bestScores.Count) ? bestScores[i].score.ToString() : "0";
            }
            if (i < dateCount && bestScoreDates[i] != null)
            {
                bestScoreDates[i].text = (i < bestScores.Count) ? bestScores[i].date : "--/--/----";
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
