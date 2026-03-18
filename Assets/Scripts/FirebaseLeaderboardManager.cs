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
    
    /// <summary>Sondaki slash olmadan base URL (//leaderboard gibi çift slash hatalarını önler)</summary>
    private string FirebaseBaseUrl => firebaseDatabaseUrl?.TrimEnd('/') ?? "";
    
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
    
    public string GetPlayerId()
    {
        return playerId;
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
        string url = $"{FirebaseBaseUrl}/leaderboard/{playerId}/maxScore.json";
        
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
        string url = $"{FirebaseBaseUrl}/leaderboard/{playerId}.json";
        
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Put(url, bodyRaw))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            Debug.Log($"📤 Firebase'e gönderiliyor - URL: {url}");
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Max score kaydedildi: {playerName} - {score}");
                onComplete?.Invoke(true);
            }
            else
            {
                Debug.LogError($"❌ Firebase Save Error: {request.error}");
                Debug.LogError($"❌ HTTP Status: {request.responseCode}");
                Debug.LogError($"❌ URL: {url}");
                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                    Debug.LogError($"❌ Response: {request.downloadHandler.text}");
                onComplete?.Invoke(false);
            }
        }
    }
    
    // Tüm leaderboard'u çek
    public void GetLeaderboard(Action<List<LeaderboardEntry>> onComplete, int limit = 100)
    {
        StartCoroutine(GetLeaderboardCoroutine(onComplete, limit));
    }
    
    #region Realtime Leaderboard (Polling)
    
    private Coroutine realtimeLeaderboardCoroutine;
    private int realtimeMaxEntries = 50;
    private float realtimeIntervalSeconds = 3f;
    
    /// <summary>Leaderboard paneli açıkken gerçek zamanlı güncelleme için dinlemeyi başlat.
    /// Her intervalSeconds saniyede bir veriyi çekip onUpdate callback'ini çağırır.</summary>
    public void StartRealtimeLeaderboard(Action<List<LeaderboardEntry>> onUpdate, int maxEntries = 50, float intervalSeconds = 3f)
    {
        StopRealtimeLeaderboard();
        realtimeMaxEntries = maxEntries;
        realtimeIntervalSeconds = Mathf.Max(1f, intervalSeconds);
        realtimeLeaderboardCoroutine = StartCoroutine(RealtimeLeaderboardCoroutine(onUpdate));
        Debug.Log($"📡 Realtime leaderboard başlatıldı (her {realtimeIntervalSeconds}s güncelleme)");
    }
    
    /// <summary>Realtime leaderboard dinlemesini durdur.</summary>
    public void StopRealtimeLeaderboard()
    {
        if (realtimeLeaderboardCoroutine != null)
        {
            StopCoroutine(realtimeLeaderboardCoroutine);
            realtimeLeaderboardCoroutine = null;
            Debug.Log("📡 Realtime leaderboard durduruldu");
        }
    }
    
    private IEnumerator RealtimeLeaderboardCoroutine(Action<List<LeaderboardEntry>> onUpdate)
    {
        if (onUpdate == null) yield break;
        
        while (true)
        {
            yield return new WaitForSecondsRealtime(realtimeIntervalSeconds);
            
            string url = $"{FirebaseBaseUrl}/leaderboard.json";
            using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    if (!string.IsNullOrEmpty(json) && json != "null" && json != "{}")
                    {
                        List<LeaderboardEntry> entries = ParseLeaderboardJson(json);
                        entries = entries.OrderByDescending(e => e.maxScore).ThenBy(e => e.timestamp).ToList();
                        if (entries.Count > realtimeMaxEntries)
                            entries = entries.Take(realtimeMaxEntries).ToList();
                        onUpdate?.Invoke(entries);
                    }
                }
            }
        }
    }
    
    #endregion
    
    private IEnumerator GetLeaderboardCoroutine(Action<List<LeaderboardEntry>> onComplete, int limit)
    {
        // Firebase'den TÜM oyuncuları çek (sıralama client-side yapılacak)
        string url = $"{FirebaseBaseUrl}/leaderboard.json";
        Debug.Log($"📊 Firebase'den TÜM oyuncuların leaderboard verisi çekiliyor...");
        Debug.Log($"📊 URL: {url}");
        
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                
                if (json != null && json.Length > 0)
                {
                    string preview = json.Length > 200 ? json.Substring(0, 200) + "..." : json;
                    Debug.Log($"📊 Firebase'den JSON alındı (uzunluk: {json.Length}): {preview}");
                }
                else
                {
                    Debug.Log("📊 Firebase'den boş JSON alındı");
                }
                
                if (!string.IsNullOrEmpty(json) && json != "null" && json != "{}")
                {
                    // Firebase dictionary formatını parse et
                    List<LeaderboardEntry> entries = ParseLeaderboardJson(json);
                    Debug.Log($"📊 Parse edildi: {entries.Count} toplam oyuncu bulundu");
                    
                    if (entries.Count == 0)
                    {
                        Debug.LogWarning("⚠️ JSON parse edildi ama hiç entry bulunamadı!");
                        Debug.LogWarning($"⚠️ JSON içeriği: {json}");
                    }
                    
                    // Max score'a göre sırala (yüksekten düşüğe)
                    entries = entries.OrderByDescending(e => e.maxScore).ThenBy(e => e.timestamp).ToList();
                    
                    // Limit uygula
                    if (entries.Count > limit)
                    {
                        entries = entries.Take(limit).ToList();
                    }
                    
                    Debug.Log($"📊 === LEADERBOARD ({entries.Count} oyuncu) ===");
                    // Tüm entry'leri logla
                    for (int i = 0; i < entries.Count; i++)
                    {
                        Debug.Log($"   {i + 1}. {entries[i].playerName} - {entries[i].maxScore} puan (ID: {entries[i].playerId})");
                    }
                    Debug.Log($"📊 ============================");
                    
                    onComplete?.Invoke(entries);
                }
                else
                {
                    Debug.LogWarning("⚠️ Firebase'den boş veya geçersiz JSON geldi!");
                    Debug.LogWarning($"⚠️ JSON değeri: '{json}'");
                    onComplete?.Invoke(new List<LeaderboardEntry>());
                }
            }
            else
            {
                Debug.LogError($"❌ Firebase Get Leaderboard Error: {request.error}");
                Debug.LogError($"❌ Response Code: {request.responseCode}");
                Debug.LogError($"❌ URL: {url}");
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
                Debug.LogWarning("⚠️ Boş veya geçersiz JSON");
                return entries;
            }
            
            // JSON'dan player ID'leri ve entry'leri çıkar
            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}"))
            {
                Debug.LogWarning($"⚠️ Invalid JSON format: başlangıç='{json[0]}', bitiş='{json[json.Length - 1]}'");
                return entries;
            }
            
            json = json.Substring(1, json.Length - 2); // { } kaldır
            
            if (string.IsNullOrEmpty(json.Trim()))
            {
                Debug.LogWarning("⚠️ JSON içeriği boş");
                return entries;
            }
            
            Debug.Log($"📊 İçerik parse ediliyor: {(json.Length > 100 ? json.Substring(0, 100) + "..." : json)}");
            
            // Daha güvenilir parsing: Player ID key'lerini bul
            // Format: "playerId": {"maxScore":..., "playerName":..., ...}
            
            int i = 0;
            while (i < json.Length)
            {
                // Player ID key'ini bul: "playerId":
                if (json[i] == '"' && i + 1 < json.Length)
                {
                    int keyStart = i + 1;
                    int keyEnd = json.IndexOf('"', keyStart);
                    
                    if (keyEnd > keyStart)
                    {
                        string potentialKey = json.Substring(keyStart, keyEnd - keyStart);
                        
                        // Key'den sonra : ve { gelmeli (player ID key'i)
                        int colonIndex = keyEnd + 1;
                        while (colonIndex < json.Length && char.IsWhiteSpace(json[colonIndex]))
                            colonIndex++;
                        
                        if (colonIndex < json.Length && json[colonIndex] == ':')
                        {
                            int braceIndex = colonIndex + 1;
                            while (braceIndex < json.Length && char.IsWhiteSpace(json[braceIndex]))
                                braceIndex++;
                            
                            if (braceIndex < json.Length && json[braceIndex] == '{')
                            {
                                // Bu bir player ID key'i! Entry'yi bul
                                string playerId = potentialKey;
                                int entryStart = braceIndex;
                                
                                // Matching brace bul
                                int entryEnd = -1;
                                int depth = 0;
                                bool inString = false;
                                char lastChar = ' ';
                                
                                for (int j = entryStart; j < json.Length; j++)
                                {
                                    char c = json[j];
                                    
                                    if (c == '"' && lastChar != '\\')
                                    {
                                        inString = !inString;
                                    }
                                    
                                    if (!inString)
                                    {
                                        if (c == '{')
                                            depth++;
                                        else if (c == '}')
                                        {
                                            depth--;
                                            if (depth == 0)
                                            {
                                                entryEnd = j;
                                                break;
                                            }
                                        }
                                    }
                                    
                                    lastChar = c;
                                }
                                
                                if (entryEnd > entryStart)
                                {
                                    string entryJson = json.Substring(entryStart, entryEnd - entryStart + 1);
                                    Debug.Log($"   🔍 Parse ediliyor - PlayerID: {playerId}");
                                    
                                    try
                                    {
                                        LeaderboardEntry entry = JsonUtility.FromJson<LeaderboardEntry>(entryJson);
                                        if (entry != null && entry.maxScore > 0)
                                        {
                                            entry.playerId = playerId;
                                            entries.Add(entry);
                                            Debug.Log($"   ✅ Entry başarıyla parse edildi: {entry.playerName} - {entry.maxScore} puan");
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"   ⚠️ Entry parse edildi ama geçersiz: maxScore={entry?.maxScore ?? -1}");
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogWarning($"   ❌ Parse hatası ({playerId}): {e.Message}");
                                        Debug.LogWarning($"   ❌ JSON: {(entryJson.Length > 100 ? entryJson.Substring(0, 100) + "..." : entryJson)}");
                                    }
                                    
                                    // Sonraki entry'ye geç
                                    i = entryEnd + 1;
                                    continue;
                                }
                            }
                        }
                    }
                }
                
                i++;
            }
            
            Debug.Log($"📊 Parse tamamlandı: {entries.Count} oyuncu başarıyla yüklendi");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ JSON Parse Error: {e.Message}");
            Debug.LogError($"❌ Stack Trace: {e.StackTrace}");
            Debug.LogError($"❌ JSON: {json}");
        }
        
        return entries;
    }
    
    // Player'ın kendi max score'unu çek
    public void GetMyMaxScore(Action<int> onComplete)
    {
        StartCoroutine(GetPlayerMaxScore(onComplete));
    }
    
    // TEST FONKSIYONU: Rastgele test oyuncuları ekle (sadece geliştirme için)
    public void AddTestPlayers(int count = 10)
    {
        StartCoroutine(AddTestPlayersCoroutine(count));
    }
    
    private IEnumerator AddTestPlayersCoroutine(int count)
    {
        string[] names = new string[] 
        { 
            "Ali", "Ayşe", "Mehmet", "Fatma", "Ahmet", "Zeynep", "Mustafa", "Elif", 
            "Emir", "Yağmur", "Efe", "Defne", "Can", "Nehir", "Ömer", "Asya",
            "Burak", "Selin", "Kerem", "Nisa", "Mert", "Ecrin", "Eren", "Ada"
        };
        
        Debug.Log($"🧪 {count} test oyuncusu ekleniyor...");
        
        for (int i = 0; i < count; i++)
        {
            string testName = names[UnityEngine.Random.Range(0, names.Length)] + UnityEngine.Random.Range(100, 999);
            int testScore = UnityEngine.Random.Range(100, 10000);
            string testPlayerId = "test_" + Guid.NewGuid().ToString().Substring(0, 8);
            
            LeaderboardEntry entry = new LeaderboardEntry
            {
                playerName = testName,
                maxScore = testScore,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                playerId = testPlayerId
            };
            
            string json = JsonUtility.ToJson(entry);
            string url = $"{FirebaseBaseUrl}/leaderboard/{testPlayerId}.json";
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            
            using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Put(url, bodyRaw))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    Debug.Log($"   ✅ Test oyuncu eklendi: {testName} - {testScore}");
                }
                else
                {
                    Debug.LogError($"   ❌ Test oyuncu eklenemedi: {request.error}");
                }
            }
            
            // Rate limiting için kısa bekleme
            yield return new WaitForSeconds(0.2f);
        }
        
        Debug.Log($"✅ {count} test oyuncusu başarıyla eklendi!");
        Debug.Log("📊 Şimdi leaderboard'u yenileyebilirsin!");
    }
    
    // TEST FONKSIYONU: Tüm test oyuncuları sil
    public void ClearTestPlayers()
    {
        StartCoroutine(ClearTestPlayersCoroutine());
    }
    
    private IEnumerator ClearTestPlayersCoroutine()
    {
        Debug.Log("🧹 Test oyuncuları temizleniyor...");
        
        // Önce tüm listeyi çek
        yield return StartCoroutine(GetLeaderboardCoroutine((entries) =>
        {
            StartCoroutine(DeleteTestEntriesCoroutine(entries));
        }, 1000));
    }
    
    private IEnumerator DeleteTestEntriesCoroutine(List<LeaderboardEntry> entries)
    {
        int deletedCount = 0;
        
        foreach (var entry in entries)
        {
            if (entry.playerId.StartsWith("test_"))
            {
                string url = $"{FirebaseBaseUrl}/leaderboard/{entry.playerId}.json";
                
                using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Delete(url))
                {
                    yield return request.SendWebRequest();
                    
                    if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                    {
                        deletedCount++;
                        Debug.Log($"   🗑️ Test oyuncu silindi: {entry.playerName}");
                    }
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        Debug.Log($"✅ {deletedCount} test oyuncusu temizlendi!");
    }
}
