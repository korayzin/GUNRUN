# 👥 NetworkPlayer Kurulum Rehberi

NetworkPlayer scripti oluşturuldu! Şimdi kurulum yapalım.

---

## 📋 ADIM 1: Player Prefab Oluşturma

### 1.1 Player Prefab Hazırla
1. Hierarchy'de mevcut **OVRCameraRig** GameObject'ini bul
2. Bu GameObject'i **prefab yap**:
   - Hierarchy'de OVRCameraRig'e **sağ tıkla**
   - **Prefab** → **Unpack Prefab** (eğer zaten prefab ise)
   - Sonra tekrar **sağ tıkla** → **Prefab** → **Create Prefab**
   - Prefab'ı `Assets/Prefabs/` klasörüne kaydet

### 1.2 NetworkPlayer Component Ekle
1. Prefab'ı seç (Project'te)
2. Inspector'da **Add Component** → **"Network Player"** ara ve ekle
3. **NetworkObject** component'i de eklenmeli (otomatik eklenir)

---

## 📋 ADIM 2: Spawn Noktaları Oluşturma

### 2.1 Host Spawn Point
1. Hierarchy'de **sağ tıkla** → **Create Empty**
2. İsmini **"HostSpawnPoint"** yap
3. Pozisyonunu ayarla (örn: X: 0, Y: 0, Z: 0)
4. Rotation'ı ayarla (hangi yöne bakacak)

### 2.2 Client Spawn Point
1. Hierarchy'de **sağ tıkla** → **Create Empty**
2. İsmini **"ClientSpawnPoint"** yap
3. Pozisyonunu ayarla (örn: X: 10, Y: 0, Z: 0) - Host'tan farklı bir yerde
4. Rotation'ı ayarla (Host'un tersi yöne bakacak)

### 2.3 Spawn Point'leri Prefab'a Bağla
1. Player prefab'ını seç
2. Inspector'da **NetworkPlayer** component'ini gör
3. **Host Spawn Point** alanına Hierarchy'deki **HostSpawnPoint**'i sürükle
4. **Client Spawn Point** alanına Hierarchy'deki **ClientSpawnPoint**'i sürükle

---

## 📋 ADIM 3: NetworkManager'a Player Prefab'ı Ekle

### 3.1 NetworkManager Scriptini Güncelle
NetworkManager'a player spawn sistemi eklemeliyiz. Şimdilik manuel olarak spawn edebiliriz.

### 3.2 Player Spawn Test
1. Play'e bas
2. NetworkManager.Instance.StartHost() çağır
3. Player spawn olmalı

---

## 📋 ADIM 4: NetworkManager'a Spawn Sistemi Ekle

NetworkManager scriptine player spawn fonksiyonu ekleyelim:

```csharp
[Header("Player Prefab")]
public NetworkObject playerPrefab;

public void SpawnPlayer()
{
    if (_runner != null && _runner.IsRunning)
    {
        // Host mu Client mi kontrol et
        bool isHost = _runner.GameMode == GameMode.Host;
        
        // Player spawn et
        _runner.Spawn(playerPrefab);
    }
}
```

---

## ✅ Kontrol Listesi

- [ ] Player prefab oluşturuldu
- [ ] NetworkPlayer component eklendi
- [ ] HostSpawnPoint oluşturuldu
- [ ] ClientSpawnPoint oluşturuldu
- [ ] Spawn point'ler prefab'a bağlandı
- [ ] Test edildi

---

## 🎯 Sonraki Adımlar

1. ✅ NetworkPlayer hazır
2. ⏭️ NetworkManager'a spawn sistemi ekle
3. ⏭️ Skor senkronizasyonu
4. ⏭️ Düşman network sistemi

---

**Şimdi spawn point'leri oluştur ve prefab'a bağla! 🚀**

