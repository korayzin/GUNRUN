# 🧪 NetworkManager Test Rehberi

NetworkManager component'i eklendi! Şimdi test edelim.

---

## 📋 ADIM 1: İlk Test

### 1.1 Play Butonuna Bas
1. Unity Editor'de **▶️ Play** butonuna bas
2. Console'u aç: **Window** → **General** → **Console**

### 1.2 Console'u Kontrol Et
Şu mesajları ara:
- ✅ **"✅ NetworkRunner otomatik oluşturuldu!"** → NetworkRunner oluşturuldu
- ✅ **"✅ Network başlatıldı!"** → Network başarıyla başlatıldı
- ❌ **Kırmızı hatalar** → Hata varsa not et

### 1.3 Hierarchy'yi Kontrol Et
1. Hierarchy penceresinde **"NetworkRunner"** adında bir GameObject görünmeli
2. Bu GameObject'te **Network Runner** component'i olmalı

---

## 📋 ADIM 2: Manuel Bağlantı Testi

### 2.1 Test Script Oluştur (Opsiyonel)
Eğer UI butonlarıyla test etmek istersen, basit bir test scripti oluşturabilirsin:

```csharp
using UnityEngine;

public class NetworkTestUI : MonoBehaviour
{
    public void OnHostButtonClick()
    {
        NetworkManager.Instance.StartHost();
    }
    
    public void OnClientButtonClick()
    {
        NetworkManager.Instance.StartClient();
    }
}
```

### 2.2 Kod ile Test
1. Herhangi bir script'ten şunu çağır:
```csharp
// Host olarak başlat
NetworkManager.Instance.StartHost();

// Veya Client olarak başlat
NetworkManager.Instance.StartClient();
```

---

## ✅ Başarılı Test Sonucu

**BAŞARILI İSE:**
- Console'da yeşil mesajlar görürsün
- Hierarchy'de NetworkRunner GameObject'i var
- Hata yok

**HATA VARSA:**
- Console'daki hata mesajını oku
- En yaygın hatalar:
  - **"App ID not found"** → PhotonAppSettings'i kontrol et
  - **"Connection failed"** → İnternet bağlantını kontrol et
  - **"Invalid App ID"** → App ID'yi tekrar kontrol et

---

## 🎯 Sonraki Adımlar

Test başarılıysa:
1. ✅ NetworkManager hazır
2. ⏭️ **NetworkPlayer scripti oluştur** (oyuncuları spawn etmek için)
3. ⏭️ **Spawn sistemi kur** (Host ve Client farklı pozisyonlarda)
4. ⏭️ **Skor senkronizasyonu**

---

**Şimdi Play'e bas ve test et! Sonucu paylaş, devam edelim! 🚀**

