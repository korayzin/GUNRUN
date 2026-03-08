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
    
    public void SetEntry(int rank, LeaderboardEntry entry, bool isCurrentPlayer = false)
    {
        // backgroundImage her zaman kontrol et - prefab'da null olabilir, root'ta Image var
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
            if (backgroundImage == null)
                backgroundImage = GetComponentInChildren<Image>(true);
        }
        
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
            
        }
        
        // Kendi ismin parlasın - özel highlight
        if (isCurrentPlayer)
        {
            ApplyCurrentPlayerGlow(rank, entry);
            return;
        }
        
        // Bizim dışımızdaki herkes aynı: tek tip arka plan ve beyaz metin
        string rankPrefix = rank switch { 1 => "🥇 ", 2 => "🥈 ", 3 => "🥉 ", _ => "" };
        
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(0.22f, 0.28f, 0.32f, 0.45f); // Hepsi aynı nötr mavi-gri
        }
        
        if (rankText != null)
        {
            rankText.text = $"{rankPrefix}{rank}.";
            rankText.color = Color.white;
            rankText.fontStyle = FontStyles.Normal;
        }
        
        if (nameText != null)
        {
            nameText.text = entry.playerName;
            nameText.color = Color.white;
            nameText.fontStyle = FontStyles.Normal;
        }
        
        if (scoreText != null)
        {
            scoreText.text = $"{entry.maxScore:N0}";
            scoreText.color = Color.white;
            scoreText.fontStyle = FontStyles.Normal;
        }
    }
    
    private void ApplyCurrentPlayerGlow(int rank, LeaderboardEntry entry)
    {
        // Daha parlak renk - diğer satırlardan belirgin şekilde ayrılsın
        Color glowColor = new Color(1f, 1f, 0.6f); // Parlak sarı-beyaz
        
        // Arka plan - daha parlak ve vurgulu
        if (backgroundImage != null)
        {
            backgroundImage.color = new Color(1f, 1f, 0.5f, 0.4f);
        }
        
        // Format diğer satırlarla aynı: rankPrefix + sayı + nokta (emoji yok, kare/karışıklık önlenir)
        string rankPrefix = rank switch
        {
            1 => "🥇 ",
            2 => "🥈 ",
            3 => "🥉 ",
            _ => ""
        };
        
        if (rankText != null)
        {
            rankText.text = $"{rankPrefix}{rank}.";
            rankText.color = glowColor;
            rankText.fontStyle = FontStyles.Bold;
        }
        
        if (nameText != null)
        {
            // Diğerleriyle aynı format: sadece isim + parantez içinde You (emoji yok)
            nameText.text = entry.playerName + " (You)";
            nameText.color = glowColor;
            nameText.fontStyle = FontStyles.Bold;
            ApplyTextGlow(nameText);
        }
        
        if (scoreText != null)
        {
            scoreText.text = $"{entry.maxScore:N0}";
            scoreText.color = glowColor;
            scoreText.fontStyle = FontStyles.Bold;
        }
    }
    
    private void ApplyTextGlow(TextMeshProUGUI text)
    {
        if (text == null) return;
        try
        {
            Material instanceMat = text.fontMaterial;
            if (instanceMat != null && instanceMat.HasProperty(Shader.PropertyToID("_OutlineWidth")))
            {
                instanceMat.SetFloat("_OutlineWidth", 0.2f);
                instanceMat.SetColor("_OutlineColor", new Color(1f, 1f, 0.5f, 1f));
            }
        }
        catch { /* Outline desteklenmiyorsa sessizce geç */ }
    }
}
