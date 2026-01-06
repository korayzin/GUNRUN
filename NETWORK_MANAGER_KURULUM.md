# 🎮 NetworkManager Script ile Kurulum

Bu yöntemle NetworkRunner'ı manuel eklemek yerine script ile otomatik oluşturup yönetiyoruz.

---

## 📋 ADIM 1: NetworkManager Script'ini Ekle

### 1.1 Script Oluşturuldu
✅ `Assets/Scripts/NetworkManager.cs` dosyası oluşturuldu

### 1.2 GameObject Oluştur
1. Unity Editor'de Hierarchy'de **sağ tıkla**
2. **Create Empty** seçeneğine tıkla
3. İsmini **"NetworkManager"** yap

### 1.3 NetworkManager Script'ini Ekle
1. Hierarchy'de **NetworkManager** GameObject'ini seç
2. Inspector'da **Add Component** butonuna tıkla
3. Arama kutusuna **"Network Manager"** yaz
4. **Network Manager** script'ini bul ve ekle

---

## 📋 ADIM 2: NetworkManager Ayarları

### 2.1 Inspector'da Ayarları Kontrol Et
1. Hierarchy'de **NetworkManager** GameObject'ini seç
2. Inspector'da şu ayarları gör:
   - **Game Mode**: Auto Host Or Client (başlangıç için)
   - **Room Name**: "GunrunRoom" (değiştirebilirsin)
   - **Network Runner Prefab**: Boş (otomatik oluşturulacak)

### 2.2 Game Mode Seçimi
- **Auto Host Or Client**: İlk bağlanan Host olur, diğerleri Client
- **Host**: Her zaman sunucu ol
- **Client**: Her zaman client ol
- **Shared**: Shared mode (peer-to-peer)

---

## 📋 ADIM 3: Test Et

### 3.1 Play Butonuna Bas
1. Unity Editor'de **▶️ Play** butonuna bas
2. Console'u aç: **Window** → **General** → **Console**

### 3.2 Console'u Kontrol Et
- **"✅ NetworkRunner otomatik oluşturuldu!"** mesajını gör
- **"✅ Network başlatıldı!"** mesajını gör
- Hata yoksa → ✅ **BAŞARILI!**

---

## 📋 ADIM 4: Kod ile Bağlantı Başlatma

### 4.1 Host Olarak Başlat
```csharp
NetworkManager.Instance.StartHost();
```

### 4.2 Client Olarak Başlat
```csharp
NetworkManager.Instance.StartClient();
```

### 4.3 Bağlantıyı Kapat
```csharp
NetworkManager.Instance.Shutdown();
```

---

## 🎯 Kullanım Örnekleri

### Örnek 1: UI Buton ile Host Başlat
```csharp
public void OnHostButtonClick()
{
    NetworkManager.Instance.StartHost();
}
```

### Örnek 2: UI Buton ile Client Başlat
```csharp
public void OnJoinButtonClick()
{
    NetworkManager.Instance.StartClient();
}
```

### Örnek 3: Oyun Başında Otomatik Başlat
```csharp
void Start()
{
    // İlk bağlanan Host olur
    NetworkManager.Instance.StartHost();
}
```

---

## ✅ Avantajları

1. ✅ **Manuel GameObject eklemeye gerek yok**
2. ✅ **Kod ile kontrol edilebilir**
3. ✅ **Otomatik NetworkRunner oluşturma**
4. ✅ **Kolay kullanım**

---

## 🔧 Sonraki Adımlar

1. ✅ NetworkManager hazır
2. ⏭️ NetworkPlayer scripti oluşturma
3. ⏭️ Spawn sistemi kurma
4. ⏭️ Skor senkronizasyonu

---

## ❓ Sorun Giderme

### Q: "NetworkRunner otomatik oluşturuldu!" mesajı görünmüyor?
**A:** Console'u kontrol et, hata mesajı var mı bak.

### Q: Bağlantı başarısız oluyor?
**A:** 
1. PhotonAppSettings'te App ID doğru mu kontrol et
2. İnternet bağlantını kontrol et
3. Console'daki hata mesajını oku

### Q: NetworkManager.Instance null?
**A:** NetworkManager GameObject'inin sahneye eklendiğinden emin ol.

---

**Şimdi NetworkManager GameObject'ini sahneye ekle ve test et! 🚀**

