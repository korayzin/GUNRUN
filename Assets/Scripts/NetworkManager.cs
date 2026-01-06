using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    
    [Header("Network Settings")]
    public GameMode gameMode = GameMode.AutoHostOrClient;
    public string roomName = "GunrunRoom";
    
    [Header("References")]
    public NetworkRunner networkRunnerPrefab;
    public NetworkObject playerPrefab;  // Player prefab'ı (NetworkObject component'i olmalı)
    
    [Header("Spawn Points")]
    public Transform hostSpawnPoint;
    public Transform clientSpawnPoint;
    
    private NetworkRunner _runner;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // NetworkRunner prefab'ı yoksa otomatik oluştur
        if (networkRunnerPrefab == null)
        {
            CreateNetworkRunner();
        }
    }
    
    private void CreateNetworkRunner()
    {
        // NetworkRunner GameObject'i oluştur
        GameObject runnerObj = new GameObject("NetworkRunner");
        _runner = runnerObj.AddComponent<NetworkRunner>();
        
        // Gerekli component'leri ekle
        if (runnerObj.GetComponent<NetworkEvents>() == null)
        {
            runnerObj.AddComponent<NetworkEvents>();
        }
        
        // Prefab referansını kaydet
        networkRunnerPrefab = _runner;
        
        Debug.Log("<color=#00FF88>✅ NetworkRunner otomatik oluşturuldu!</color>");
    }
    
    public async void StartHost()
    {
        Debug.Log("<color=#00FF88>🚀 StartHost() çağrıldı!</color>");
        await StartGame(GameMode.Host);
    }
    
    public async void StartClient()
    {
        Debug.Log("<color=#00FF88>🚀 StartClient() çağrıldı!</color>");
        await StartGame(GameMode.Client);
    }
    
    public async void StartShared()
    {
        await StartGame(GameMode.Shared);
    }
    
    private async Task StartGame(GameMode mode)
    {
        Debug.Log($"<color=#00FF88>🎮 StartGame() çağrıldı! Mod: {mode}</color>");
        
        // NetworkRunner yoksa oluştur
        if (_runner == null)
        {
            if (networkRunnerPrefab != null)
            {
                _runner = Instantiate(networkRunnerPrefab);
            }
            else
            {
                CreateNetworkRunner();
            }
        }
        
        // NetworkRunner zaten çalışıyorsa durdur
        if (_runner.IsRunning)
        {
            await _runner.Shutdown();
        }
        
        // SceneManager yoksa ekle
        var sceneManager = _runner.GetComponent<INetworkSceneManager>();
        if (sceneManager == null)
        {
            sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        
        // ObjectProvider yoksa ekle
        var objectProvider = _runner.GetComponent<INetworkObjectProvider>();
        if (objectProvider == null)
        {
            objectProvider = _runner.gameObject.AddComponent<NetworkObjectProviderDefault>();
        }
        
        // Scene bilgisini hazırla
        var sceneInfo = new NetworkSceneInfo();
        var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Additive);
        
        // Oyunu başlat
        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            Scene = sceneInfo,
            SceneManager = sceneManager,
            ObjectProvider = objectProvider
        };
        
        Debug.Log($"<color=#00FF88>⏳ Network başlatılıyor... Mod: {mode}, Oda: {roomName}</color>");
        var result = await _runner.StartGame(startGameArgs);
        
        Debug.Log($"<color=#00FF88>📊 StartGame sonucu: Ok={result.Ok}, ShutdownReason={result.ShutdownReason}</color>");
        
        if (result.Ok)
        {
            Debug.Log($"<color=#00FF88>✅ Network başlatıldı! Mod: {mode}, Oda: {roomName}</color>");
            
            // NetworkEvents'e bağlan
            var networkEvents = _runner.GetComponent<NetworkEvents>();
            if (networkEvents != null)
            {
                networkEvents.PlayerJoined.AddListener(OnPlayerJoined);
                Debug.Log("<color=#00FF88>✅ PlayerJoined event listener eklendi</color>");
            }
            else
            {
                Debug.LogWarning("<color=#FFAA00>⚠️ NetworkEvents bulunamadı! PlayerJoined event'i dinlenemiyor.</color>");
            }
            
            // Mevcut oyuncuları kontrol et ve spawn et (Host başlatıldığında kendisi için de spawn gerekir)
            // Kısa bir gecikme ile kontrol et (NetworkRunner hazır olması için)
            StartCoroutine(CheckAndSpawnPlayers());
        }
        else
        {
            Debug.LogError($"<color=#FF0000>❌ Network başlatılamadı! Mod: {mode}, ShutdownReason: {result.ShutdownReason}, Ok: {result.Ok}</color>");
            
            // Client bağlantı hatası için özel mesaj
            if (mode == GameMode.Client)
            {
                Debug.LogError($"<color=#FF0000>💡 Client bağlantı hatası! Kontrol et:</color>");
                Debug.LogError($"<color=#FF0000>   1. Host başlatıldı mı? (H tuşuna bas)</color>");
                Debug.LogError($"<color=#FF0000>   2. Aynı oda adı kullanılıyor mu? ({roomName})</color>");
                Debug.LogError($"<color=#FF0000>   3. İnternet bağlantısı var mı?</color>");
            }
        }
    }
    
    public void Shutdown()
    {
        if (_runner != null && _runner.IsRunning)
        {
            _runner.Shutdown();
            Debug.Log("<color=#00FF88>🔌 Network kapatıldı</color>");
        }
    }
    
    private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"<color=#00FF88>👤 Oyuncu katıldı: {player}, LocalPlayer: {runner.LocalPlayer}, IsServer: {runner.IsServer}</color>");
        
        // Her yeni oyuncu için player spawn et
        // Host/Server her oyuncu için spawn yapar (hem kendi hem de client'lar için)
        if (runner.IsServer || runner.GameMode == GameMode.Host)
        {
            Debug.Log($"<color=#00FF88>🎮 Server/Host - Player spawn ediliyor: {player}</color>");
            SpawnPlayerForPlayer(player);
        }
        else
        {
            // Client ise, server'ın spawn işlemini bekle
            // Ama eğer bu client'in kendisi ise ve henüz spawn edilmemişse, server'a bildir
            Debug.Log($"<color=#00FF88>ℹ️ Client - Spawn işlemi server tarafından yapılacak (Player: {player})</color>");
        }
    }
    
    public void SpawnPlayer()
    {
        if (_runner != null && _runner.IsRunning && playerPrefab != null)
        {
            // Local player için spawn et
            SpawnPlayerForPlayer(_runner.LocalPlayer);
        }
        else
        {
            Debug.LogWarning("<color=#FFAA00>⚠️ Player spawn edilemedi! NetworkRunner çalışmıyor veya playerPrefab atanmamış.</color>");
        }
    }
    
    private void SpawnPlayerForPlayer(PlayerRef player)
    {
        if (_runner == null || !_runner.IsRunning || playerPrefab == null)
            return;
        
        // Spawn pozisyonunu belirle (ilk oyuncu Host, ikinci Client)
        Transform spawnPoint = null;
        bool isFirstPlayer = false;
        if (_runner.ActivePlayers.Count() > 0)
        {
            var firstPlayer = _runner.ActivePlayers.First();
            isFirstPlayer = player == firstPlayer;
        }
        else
        {
            // Eğer henüz oyuncu yoksa, bu ilk oyuncu
            isFirstPlayer = true;
        }
        
        if (isFirstPlayer && hostSpawnPoint != null)
        {
            spawnPoint = hostSpawnPoint;
        }
        else if (!isFirstPlayer && clientSpawnPoint != null)
        {
            spawnPoint = clientSpawnPoint;
        }
        
        // Player spawn et
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        
        // InputAuthority ile spawn et
        var playerObject = _runner.Spawn(playerPrefab, spawnPos, spawnRot, inputAuthority: player);
        
        // InputAuthority'yi kontrol et ve gerekirse ata
        if (playerObject != null)
        {
            // Eğer InputAuthority atanmamışsa, manuel ata
            if (playerObject.InputAuthority.IsNone && player.IsRealPlayer)
            {
                playerObject.AssignInputAuthority(player);
                Debug.Log($"<color=#00FF88>🔧 InputAuthority manuel olarak atandı: {player}</color>");
            }
            
            Debug.Log($"<color=#00FF88>✅ Player spawn edildi: PlayerRef={player}, InputAuthority={playerObject.InputAuthority}, Pozisyon={spawnPos}, İlkOyuncu={isFirstPlayer}</color>");
        }
        else
        {
            Debug.LogError("❌ Player spawn edilemedi! playerObject null döndü.");
        }
    }
    
    private IEnumerator CheckAndSpawnPlayers()
    {
        // NetworkRunner'ın hazır olması için kısa bir bekleme
        yield return new WaitForSeconds(0.5f);
        
        if (_runner == null || !_runner.IsRunning)
            yield break;
        
        Debug.Log($"<color=#00FF88>🔍 Mevcut oyuncular kontrol ediliyor... ActivePlayers: {_runner.ActivePlayers.Count()}, IsServer: {_runner.IsServer}, GameMode: {_runner.GameMode}</color>");
        
        // Host/Server ise, mevcut tüm oyuncular için player spawn et
        if (_runner.IsServer || _runner.GameMode == GameMode.Host)
        {
            foreach (var player in _runner.ActivePlayers)
            {
                // Bu oyuncu için zaten player spawn edilmiş mi kontrol et
                bool playerExists = false;
                foreach (var obj in _runner.GetAllBehaviours<NetworkPlayer>())
                {
                    if (obj.Object != null && obj.Object.InputAuthority == player)
                    {
                        playerExists = true;
                        Debug.Log($"<color=#00FF88>ℹ️ Player {player} için zaten NetworkPlayer mevcut</color>");
                        break;
                    }
                }
                
                // Eğer player spawn edilmemişse, spawn et
                if (!playerExists)
                {
                    Debug.Log($"<color=#00FF88>🎮 Mevcut oyuncu için player spawn ediliyor: {player}</color>");
                    SpawnPlayerForPlayer(player);
                }
            }
        }
        else
        {
            // Client ise, kendi player'ını kontrol et
            Debug.Log($"<color=#00FF88>🔍 Client modunda - LocalPlayer: {_runner.LocalPlayer}</color>");
            
            // Client'in kendi player'ı spawn edilmiş mi kontrol et
            bool localPlayerExists = false;
            foreach (var obj in _runner.GetAllBehaviours<NetworkPlayer>())
            {
                if (obj.Object != null && obj.Object.InputAuthority == _runner.LocalPlayer)
                {
                    localPlayerExists = true;
                    Debug.Log($"<color=#00FF88>✅ Client'in kendi player'ı zaten spawn edilmiş</color>");
                    break;
                }
            }
            
            if (!localPlayerExists)
            {
                Debug.LogWarning($"<color=#FFAA00>⚠️ Client'in kendi player'ı spawn edilmemiş! Server spawn işlemini yapmalı.</color>");
            }
        }
    }
    
    private void OnDestroy()
    {
        Shutdown();
    }
}

