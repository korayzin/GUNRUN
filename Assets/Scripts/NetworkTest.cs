using UnityEngine;
using Fusion;

public class NetworkTest : MonoBehaviour
{
    [Header("Test Buttons")]
    public bool autoStartHost = false;
    
    private void Start()
    {
        Debug.Log("<color=#00FF88>✅ NetworkTest scripti başlatıldı!</color>");
        Debug.Log($"<color=#00FF88>🔍 NetworkManager.Instance kontrolü: {(NetworkManager.Instance != null ? "VAR" : "NULL")}</color>");
        
        if (autoStartHost && NetworkManager.Instance != null)
        {
            NetworkManager.Instance.StartHost();
        }
    }
    
    private void Update()
    {
        // Test için klavye kısayolları
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("<color=#00FF88>🔵 H tuşuna basıldı!</color>");
            
            if (NetworkManager.Instance == null)
            {
                Debug.LogError("<color=#FF0000>❌ NetworkManager.Instance NULL! NetworkManager GameObject'i sahneye eklenmiş mi kontrol et!</color>");
                return;
            }
            
            Debug.Log("<color=#00FF88>🔵 Host başlatılıyor...</color>");
            NetworkManager.Instance.StartHost();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("<color=#00FF88>🔵 C tuşuna basıldı!</color>");
            Debug.Log($"<color=#00FF88>🔍 NetworkManager.Instance kontrolü: {(NetworkManager.Instance != null ? "VAR" : "NULL")}</color>");
            
            if (NetworkManager.Instance == null)
            {
                Debug.LogError("<color=#FF0000>❌ NetworkManager.Instance NULL! NetworkManager GameObject'i sahneye eklenmiş mi kontrol et!</color>");
                return;
            }
            
            Debug.Log("<color=#00FF88>🔵 Client başlatılıyor...</color>");
            NetworkManager.Instance.StartClient();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("<color=#00FF88>🔵 S tuşuna basıldı!</color>");
            
            if (NetworkManager.Instance == null)
            {
                Debug.LogError("<color=#FF0000>❌ NetworkManager.Instance NULL!</color>");
                return;
            }
            
            Debug.Log("<color=#00FF88>🔵 Player spawn ediliyor...</color>");
            NetworkManager.Instance.SpawnPlayer();
        }
        
        // Her frame test (debug için)
        if (Time.frameCount % 300 == 0) // Her 300 frame'de bir
        {
            if (NetworkManager.Instance == null)
            {
                Debug.LogWarning("<color=#FFAA00>⚠️ NetworkManager.Instance hala NULL! NetworkManager GameObject'i sahneye eklenmiş mi?</color>");
            }
        }
    }
}

