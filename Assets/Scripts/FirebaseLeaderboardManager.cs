using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int maxScore;
    public long timestamp;
    public string playerId;
}

[System.Serializable]
public class LeaderboardData
{
    public Dictionary<string, LeaderboardEntry> players = new Dictionary<string, LeaderboardEntry>();
}

public class FirebaseLeaderboardManager : MonoBehaviour
{
    public static FirebaseLeaderboardManager Instance;
    
    [Header("Firebase Config")]
    public string firebaseDatabaseUrl = "https://gunrundata-default-rtdb.europe-west1.firebasedatabase.app"; // Firebase Console'dan alınacak
    
    private string playerId;
    private string playerName = "Player";
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePlayer();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializePlayer()
    {
        // Unique player ID oluştur (device ID + random)
        playerId = SystemInfo.deviceUniqueIdentifier;
        if (string.IsNullOrEmpty(playerId))
        {
            playerId = SystemInfo.deviceModel + "_" + UnityEngine.Random.Range(1000, 9999);
        }
        
        // Player name'i PlayerPrefs'ten al veya default kullan
        playerName = PlayerPrefs.GetString("PlayerName", "Player_" + UnityEngine.Random.Range(100, 999));
    }
    
    public void SetPlayerName(string name)
    {
        playerName = name;
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.Save();
    }
    
    public string GetPlayerName()
    {
        return playerName;
    }
    
    // Max score'u Firebase'e kaydet
    public void SaveMaxScore(int score, Action<bool> onComplete = null)
    {
        StartCoroutine(SaveMaxScoreCoroutine(score, onComplete));
    }
    
    private IEnumerator SaveMaxScoreCoroutine(int score, Action<bool> onComplete)
    {
        // Önce mevcut max score'u kontrol et
        yield return StartCoroutine(GetPlayerMaxScore((currentMax) =>
        {
            // Sadece yeni score daha yüksekse kaydet
            if (score > currentMax)
            {
                StartCoroutine(UpdatePlayerScore(score, onComplete));
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }));
    }
    
    private IEnumerator GetPlayerMaxScore(Action<int> onComplete)
    {
        string url = $"{firebaseDatabaseUrl}/leaderboard/{playerId}/maxScore.json";
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                if (!string.IsNullOrEmpty(json) && json != "null")
                {
                    int maxScore = int.Parse(json.Trim('"'));
                    onComplete?.Invoke(maxScore);
                }
                else
                {
                    onComplete?.Invoke(0);
                }
            }
            else
            {
                Debug.LogError($"Firebase Get Error: {request.error}");
                onComplete?.Invoke(0);
            }
        }
    }
    
    private IEnumerator UpdatePlayerScore(int score, Action<bool> onComplete)
    {
        LeaderboardEntry entry = new LeaderboardEntry
        {
            playerName = playerName,
            maxScore = score,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            playerId = playerId
        };
        
        string json = JsonUtility.ToJson(entry);
        string url = $"{firebaseDatabaseUrl}/leaderboard/{playerId}.json";
        
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Put(url, bodyRaw))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Max score kaydedildi: {playerName} - {score}");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"❌ Firebase Save Error: {request.error}");
                onComplete?.Invoke(false);
            }
        }
    }
    
    // Tüm leaderboard'u çek
    public void GetLeaderboard(Action<List<LeaderboardEntry>> onComplete, int limit = 100)
    {
        StartCoroutine(GetLeaderboardCoroutine(onComplete, limit));
    }
    
    private IEnumerator GetLeaderboardCoroutine(Action<List<LeaderboardEntry>> onComplete, int limit)
    {
        string url = $"{firebaseDatabaseUrl}/leaderboard.json?orderBy=\"maxScore\"&limitToLast={limit}";
        Debug.Log($"📊 Firebase'den leaderboard çekiliyor: {url}");
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                if (json != null && json.Length > 0)
                {
                    string preview = json.Length > 100 ? json.Substring(0, 100) : json;
                    Debug.Log($"📊 Firebase'den JSON alındı: {preview}...");
                }
                else
                {
                    Debug.Log("📊 Firebase'den boş JSON alındı");
                }
                
                if (!string.IsNullOrEmpty(json) && json != "null")
                {
                    // Firebase dictionary formatını parse et
                    List<LeaderboardEntry> entries = ParseLeaderboardJson(json);
                    Debug.Log($"📊 Parse edildi: {entries.Count} entry bulundu");
                    
                    // Max score'a göre sırala (yüksekten düşüğe)
                    entries = entries.OrderByDescending(e => e.maxScore).ToList();
                    
                    // İlk 5 entry'yi logla
                    for (int i = 0; i < Mathf.Min(5, entries.Count); i++)
                    {
                        Debug.Log($"   {i + 1}. {entries[i].playerName} - {entries[i].maxScore}");
                    }
                    
                    onComplete?.Invoke(entries);
                }
                else
                {
                    Debug.LogWarning("⚠️ Firebase'den boş JSON geldi");
                    onComplete?.Invoke(new List<LeaderboardEntry>());
                }
            }
            else
            {
                Debug.LogError($"❌ Firebase Get Leaderboard Error: {request.error}");
                onComplete?.Invoke(new List<LeaderboardEntry>());
            }
        }
    }
    
    private List<LeaderboardEntry> ParseLeaderboardJson(string json)
    {
        List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
        Debug.Log($"📊 JSON parse ediliyor... (uzunluk: {(json != null ? json.Length : 0)})");
        
        try
        {
            // Firebase JSON formatı: {"playerId1": {...}, "playerId2": {...}}
            // JsonUtility Dictionary desteklemediği için manuel parse yapıyoruz
            
            if (string.IsNullOrEmpty(json) || json == "null" || json == "{}")
            {
                return entries;
            }
            
            // JSON'dan player ID'leri ve entry'leri çıkar
            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}"))
            {
                Debug.LogWarning("Invalid JSON format");
                return entries;
            }
            
            json = json.Substring(1, json.Length - 2); // { } kaldır
            
            // Her player entry'sini bul
            int depth = 0;
            int startIndex = 0;
            string currentKey = "";
            
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                
                if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    // Key başlangıcı
                    if (depth == 0)
                    {
                        int keyStart = i + 1;
                        int keyEnd = json.IndexOf('"', keyStart);
                        if (keyEnd > 0)
                        {
                            currentKey = json.Substring(keyStart, keyEnd - keyStart);
                            i = keyEnd + 1;
                            // : karakterini atla
                            while (i < json.Length && (json[i] == ':' || char.IsWhiteSpace(json[i])))
                                i++;
                            i--; // for loop'ta i++ olacak
                            startIndex = i + 1;
                        }
                    }
                }
                else if (c == '{')
                {
                    depth++;
                }
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0 && !string.IsNullOrEmpty(currentKey))
                    {
                        // Entry tamamlandı
                        string entryJson = json.Substring(startIndex, i - startIndex + 1);
                        try
                        {
                            LeaderboardEntry entry = JsonUtility.FromJson<LeaderboardEntry>(entryJson);
                            if (entry != null)
                            {
                                entry.playerId = currentKey;
                                entries.Add(entry);
                                Debug.Log($"   ✅ Entry parse edildi: {entry.playerName} - {entry.maxScore}");
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Parse error for {currentKey}: {e.Message}");
                        }
                        currentKey = "";
                        // Sonraki entry için hazırla
                        while (i + 1 < json.Length && (json[i + 1] == ',' || char.IsWhiteSpace(json[i + 1])))
                            i++;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON Parse Error: {e.Message}\nJSON: {json}");
        }
        
        return entries;
    }
    
    // Player'ın kendi max score'unu çek
    public void GetMyMaxScore(Action<int> onComplete)
    {
        StartCoroutine(GetPlayerMaxScore(onComplete));
    }
}
