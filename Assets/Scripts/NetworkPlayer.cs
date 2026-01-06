using UnityEngine;
using Fusion;
using System.Linq;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Spawn Positions")]
    public Transform hostSpawnPoint;      // Host oyuncu burada spawn olacak
    public Transform clientSpawnPoint;   // Client oyuncu burada spawn olacak
    
    [Header("Player References")]
    public OVRCameraRig ovrCameraRig;   // VR kamera rig
    
    [Networked]
    public int playerScore { get; set; }
    
    private NetworkManager networkManager;
    
    public override void Spawned()
    {
        // NetworkObject kontrolü
        if (Object == null)
        {
            Debug.LogError("<color=#FF0000>❌ NetworkObject bulunamadı! NetworkPlayer GameObject'inde NetworkObject component'i olmalı!</color>");
            return;
        }
        
        // InputAuthority kontrolü - eğer None ise ve bu local player ise ata
        if (Object.InputAuthority.IsNone && Runner != null && Runner.LocalPlayer.IsRealPlayer)
        {
            // Local player için InputAuthority ata
            Object.AssignInputAuthority(Runner.LocalPlayer);
            Debug.Log($"<color=#00FF88>🔧 InputAuthority Spawned() içinde atandı: {Runner.LocalPlayer}</color>");
        }
        
        // NetworkManager'ı bul
        networkManager = NetworkManager.Instance;
        
        // Spawn pozisyonunu ayarla
        SetSpawnPosition();
        
        // VR kamera rig'i ayarla
        SetupVRCamera();
        
        Debug.Log($"<color=#00FF88>✅ NetworkPlayer spawned! InputAuthority: {Object.InputAuthority}, HasInputAuthority: {Object.HasInputAuthority}</color>");
    }
    
    private void SetSpawnPosition()
    {
        // NetworkManager'dan spawn pozisyonunu al
        if (networkManager != null)
        {
            Transform spawnPoint = null;
            
            // InputAuthority kontrolü ile hangi oyuncu olduğunu belirle
            // LocalPlayer kontrolü yap (InputAuthority None olabilir spawn anında)
            bool isHost = false;
            
            // Önce LocalPlayer kontrolü yap
            if (Object.HasInputAuthority && Runner.LocalPlayer.IsRealPlayer)
            {
                // Local player ise, ilk oyuncu mu kontrol et
                if (Runner.ActivePlayers.Count() > 0)
                {
                    var firstPlayer = Runner.ActivePlayers.First();
                    isHost = Runner.LocalPlayer == firstPlayer;
                }
                else
                {
                    // Eğer henüz oyuncu yoksa, bu ilk oyuncu (Host)
                    isHost = true;
                }
            }
            else if (Object.InputAuthority.IsRealPlayer)
            {
                // InputAuthority varsa kontrol et
                if (Runner.ActivePlayers.Count() > 0)
                {
                    var firstPlayer = Runner.ActivePlayers.First();
                    isHost = Object.InputAuthority == firstPlayer;
                }
            }
            
            if (isHost && networkManager.hostSpawnPoint != null)
            {
                spawnPoint = networkManager.hostSpawnPoint;
            }
            else if (!isHost && networkManager.clientSpawnPoint != null)
            {
                spawnPoint = networkManager.clientSpawnPoint;
            }
            
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;
                Debug.Log($"<color=#00FF88>📍 Player spawn pozisyonu ayarlandı: {(isHost ? "Host" : "Client")} - {spawnPoint.position}</color>");
            }
            else
            {
                // Fallback: Prefab'taki spawn point'leri kullan
                Transform prefabSpawnPoint = isHost ? hostSpawnPoint : clientSpawnPoint;
                if (prefabSpawnPoint != null)
                {
                    transform.position = prefabSpawnPoint.position;
                    transform.rotation = prefabSpawnPoint.rotation;
                    Debug.Log($"<color=#00FF88>📍 Player spawn pozisyonu (prefab) ayarlandı: {(isHost ? "Host" : "Client")}</color>");
                }
                else
                {
                    Debug.LogWarning($"<color=#FFAA00>⚠️ Spawn point bulunamadı! {(isHost ? "Host" : "Client")} için varsayılan pozisyon kullanılıyor.</color>");
                }
            }
        }
        else
        {
            Debug.LogWarning("<color=#FFAA00>⚠️ NetworkManager bulunamadı! Spawn pozisyonu ayarlanamadı.</color>");
        }
    }
    
    private void SetupVRCamera()
    {
        // Player prefab'ı OVRCameraRig'in kendisi olduğu için:
        // - NetworkPlayer script'i OVRCameraRig'in üzerinde
        // - OVRCameraRig'i bu GameObject'ten al (GetComponent)
        
        // OVRCameraRig'i bu GameObject'ten al (player prefab'ı OVRCameraRig'in kendisi)
        if (ovrCameraRig == null)
        {
            ovrCameraRig = GetComponent<OVRCameraRig>();
        }
        
        // Eğer hala null ise, parent'ta ara (bazı durumlarda NetworkPlayer child'da olabilir)
        if (ovrCameraRig == null)
        {
            ovrCameraRig = GetComponentInParent<OVRCameraRig>();
        }
        
        if (ovrCameraRig == null)
        {
            Debug.LogError("<color=#FF0000>❌ OVRCameraRig bulunamadı! NetworkPlayer script'i OVRCameraRig'in üzerinde olmalı!</color>");
            return;
        }
        
        // Local player kontrolü - hem HasInputAuthority hem de LocalPlayer kontrolü
        bool isLocalPlayer = Object.HasInputAuthority || 
                           (Runner != null && Runner.LocalPlayer.IsRealPlayer && Object.InputAuthority == Runner.LocalPlayer);
        
        Debug.Log($"<color=#00FF88>🔍 VR Kamera Kontrolü: HasInputAuthority={Object.HasInputAuthority}, InputAuthority={Object.InputAuthority}, LocalPlayer={Runner?.LocalPlayer}, isLocalPlayer={isLocalPlayer}</color>");
        
        if (isLocalPlayer)
        {
            // Local player - kendi kamerasını aktif et
            ovrCameraRig.gameObject.SetActive(true);
            Debug.Log("<color=#00FF88>✅ Local player VR kamera aktif edildi</color>");
        }
        else
        {
            // Remote player - kendi kamerasını deaktif et (sadece local player görsün)
            ovrCameraRig.gameObject.SetActive(false);
            Debug.Log("<color=#00FF88>ℹ️ Remote player VR kamera deaktif edildi</color>");
        }
    }
    
    public void AddScore(int points)
    {
        if (Object.HasStateAuthority)
        {
            playerScore += points;
            Debug.Log($"<color=#00FF88>📊 Skor güncellendi: {playerScore}</color>");
        }
    }
    
    public override void FixedUpdateNetwork()
    {
        // Network update'ler burada yapılabilir
    }
}

