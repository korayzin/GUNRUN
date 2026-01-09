using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;
    public Image backgroundImage;
    
    public void SetEntry(int rank, LeaderboardEntry entry)
    {
        // Önce otomatik bul (eğer atanmamışsa)
        if (rankText == null || nameText == null || scoreText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            
            if (texts.Length >= 3)
            {
                // Child object'lerin isimlerine göre ata
                foreach (TextMeshProUGUI text in texts)
                {
                    string name = text.gameObject.name.ToLower();
                    if (name.Contains("rank") || name.Contains("sira"))
                    {
                        rankText = text;
                    }
                    else if (name.Contains("name") || name.Contains("isim") || name.Contains("oyuncu"))
                    {
                        nameText = text;
                    }
                    else if (name.Contains("score") || name.Contains("puan"))
                    {
                        scoreText = text;
                    }
                }
                
                // Hala null'sa sırayla ata
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
            
            // Background Image'ı bul
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }
        }
        
        // Özel renk ve emoji için ilk 3 sırayı vurgula
        Color textColor = Color.white;
        string rankPrefix = "";
        
        if (rank == 1)
        {
            textColor = new Color(1f, 0.84f, 0f); // Altın
            rankPrefix = "🥇 ";
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(1f, 0.84f, 0f, 0.15f);
            }
        }
        else if (rank == 2)
        {
            textColor = new Color(0.75f, 0.75f, 0.75f); // Gümüş
            rankPrefix = "🥈 ";
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.75f, 0.75f, 0.75f, 0.15f);
            }
        }
        else if (rank == 3)
        {
            textColor = new Color(0.8f, 0.5f, 0.2f); // Bronz
            rankPrefix = "🥉 ";
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.8f, 0.5f, 0.2f, 0.15f);
            }
        }
        else
        {
            // Alternatif satır renkleri
            if (backgroundImage != null)
            {
                backgroundImage.color = (rank % 2 == 0) 
                    ? new Color(0.2f, 0.2f, 0.2f, 0.3f) 
                    : new Color(0.15f, 0.15f, 0.15f, 0.3f);
            }
        }
        
        // Text'leri set et
        if (rankText != null)
        {
            rankText.text = $"{rankPrefix}{rank}.";
            rankText.color = textColor;
            if (rank <= 3)
            {
                rankText.fontStyle = FontStyles.Bold;
            }
        }
        
        if (nameText != null)
        {
            nameText.text = entry.playerName;
            nameText.color = textColor;
            if (rank <= 3)
            {
                nameText.fontStyle = FontStyles.Bold;
            }
        }
        
        if (scoreText != null)
        {
            scoreText.text = $"{entry.maxScore:N0}"; // Binlik ayırıcılarla
            scoreText.color = textColor;
            if (rank <= 3)
            {
                scoreText.fontStyle = FontStyles.Bold;
            }
        }
    }
}
