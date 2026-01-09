using UnityEngine;
using TMPro;

public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    
    public void SetEntry(int rank, LeaderboardEntry entry)
    {
        // Önce otomatik bul (eğer atanmamışsa)
        if (rankText == null || nameText == null || scoreText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            
            if (texts.Length >= 3)
            {
                if (rankText == null) rankText = texts[0];
                if (nameText == null) nameText = texts[1];
                if (scoreText == null) scoreText = texts[2];
            }
            else if (texts.Length >= 1)
            {
                // Tek text varsa hepsini birleştir
                if (rankText == null) rankText = texts[0];
                rankText.text = $"{rank}. {entry.playerName} - {entry.maxScore}";
                return;
            }
        }
        
        // Text'leri set et
        if (rankText != null)
        {
            rankText.text = rank.ToString();
        }
        
        if (nameText != null)
        {
            nameText.text = entry.playerName;
        }
        
        if (scoreText != null)
        {
            scoreText.text = entry.maxScore.ToString();
        }
    }
}
