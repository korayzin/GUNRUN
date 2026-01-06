# 🎮 Multiplayer Yol Haritası - Photon Fusion

## 📋 Genel Bakış
Bu oyun **Photon Fusion** kullanarak host-client yapısında 2 oyunculu multiplayer yapılacak. Her oyuncu farklı tarafta başlayacak ve en yüksek skoru alan kazanacak.

## 🚀 Neden Photon Fusion?

### Fusion'ın Avantajları:
- ✅ **Tick-based Simulation** - Deterministic, tutarlı simülasyon
- ✅ **Client-side Prediction** - Daha iyi lag compensation, daha akıcı oyun
- ✅ **State Synchronization** - NetworkBehaviour ile otomatik state senkronizasyonu
- ✅ **Network Physics** - Fizik senkronizasyonu daha iyi
- ✅ **Server Authority** - Daha güvenli, cheat koruması
- ✅ **Daha Yüksek Performans** - PUN2'den daha optimize
- ✅ **Modern API** - Daha temiz ve güçlü API

### PUN2 vs Fusion:
- PUN2: Basit, hafif, eski teknoloji
- Fusion: Modern, güçlü, yüksek performans, tick-based

---

## 🚀 Faz 1: Photon Fusion Kurulumu ve Temel Altyapı

### 1.1 Photon Fusion Kurulumu
- [ ] Unity Package Manager'dan Photon Fusion paketini kur
- [ ] Photon Dashboard'dan App ID al (Fusion için)
- [ ] FusionLauncher prefab'ını sahneye ekle
- [ ] NetworkProjectConfig asset'ini yapılandır
- [ ] Test bağlantısını doğrula

### 1.2 Network Manager Oluşturma
- [ ] `NetworkManager.cs` - NetworkBehaviour'dan türeyen ana network yöneticisi
  - NetworkRunner başlatma/durdurma
  - Host/Client rolleri (GameMode.Host, GameMode.Client)
  - Oyun durumu yönetimi (Lobby, Playing, GameOver)
  - Session yönetimi
  
### 1.3 Lobby Sistemi
- [ ] `LobbyManager.cs` - Oyun bekleme odası
  - Host: "Oyun Başlat" butonu
  - Client: "Katıl" butonu ve oda listesi
  - Oyuncu sayısı gösterimi
  - Hazır olma durumu

---

## 👥 Faz 2: Oyuncu Network Sistemi

### 2.1 Network Player Prefab
- [ ] `NetworkPlayer.cs` - NetworkBehaviour'dan türeyen oyuncu scripti
  - `HasInputAuthority` / `HasStateAuthority` kontrolü
  - Spawn pozisyonu yönetimi (Host: bir taraf, Client: diğer taraf)
  - Kamera yönü ayarlama
  - OVRCameraRig network entegrasyonu
  - NetworkTransform component (pozisyon senkronizasyonu için)

### 2.2 Oyuncu Spawn Sistemi
- [ ] Spawn noktaları tanımla (2 farklı pozisyon)
- [ ] Host oyuncu bir tarafta, Client oyuncu diğer tarafta spawn
- [ ] Her oyuncu kendi tarafına bakacak şekilde kamera ayarla

### 2.3 Oyuncu Senkronizasyonu
- [ ] Pozisyon senkronizasyonu (gerekirse)
- [ ] Animasyon senkronizasyonu
- [ ] Silah görünürlüğü (sadece kendi silahını gör)

---

## 🔫 Faz 3: Silah ve Mermi Network Sistemi

### 3.1 Network Silah Sistemi
- [ ] `NetworkGunFire.cs` - NetworkBehaviour'dan türeyen silah scripti
  - `HasInputAuthority` kontrolü (sadece local oyuncu ateş edebilir)
  - `RPC` veya `NetworkEvents` ile ateş etme eventi gönder
  - Mermi spawn'ını `Runner.Spawn()` ile yap
  - Client-side prediction için input handling

### 3.2 Network Mermi
- [ ] `NetworkBullet.cs` - NetworkBehaviour'dan türeyen mermi scripti
  - `Runner.Spawn()` ile spawn
  - `HasStateAuthority` kontrolü (kimin mermisi)
  - Hasar verme yetkisi (sadece owner)
  - `Runner.Despawn()` ile destroy
  - NetworkTransform ile pozisyon senkronizasyonu

### 3.3 Mermi Görünürlüğü
- [ ] Tüm oyuncular tüm mermileri görebilmeli
- [ ] Mermi efektleri senkronize

---

## 👾 Faz 4: Düşman Network Sistemi

### 4.1 Network Düşman
- [ ] `NetworkEnemy.cs` - NetworkBehaviour'dan türeyen düşman scripti
  - Host tarafından `Runner.Spawn()` ile spawn edilir
  - `HasStateAuthority` kontrolü (Host kontrolünde)
  - Tüm oyuncular görebilir
  - Hasar verme yetkisi (her oyuncu hasar verebilir, RPC ile)
  - Ölüm durumu `NetworkProperties` ile senkronize
  - NetworkTransform ile pozisyon senkronizasyonu

### 4.2 Portal Spawner Network
- [ ] `NetworkPortalSpawner.cs` - NetworkBehaviour'dan türeyen spawner
  - Sadece Host spawn eder (`HasStateAuthority`)
  - Portal ve düşman spawn'ları `Runner.Spawn()` ile
  - Tüm oyuncular görebilir
  - Tick-based spawn timing

---

## 📊 Faz 5: Skor ve Oyun Yönetimi

### 5.1 Network Game Manager
- [ ] `NetworkGameManager.cs` - NetworkBehaviour'dan türeyen game manager
  - `NetworkProperties` ile skor senkronizasyonu (her oyuncunun skoru ayrı)
  - `NetworkProperties` ile timer senkronizasyonu (tüm oyuncular aynı süreyi görür)
  - Oyun başlatma/bitirme senkronizasyonu (`RPC` veya `NetworkEvents`)
  - Kazanan belirleme (en yüksek skor)
  - `HasStateAuthority` kontrolü (Host kontrolünde)

### 5.2 Skor Sistemi
- [ ] Her oyuncunun skoru ayrı tutulur
- [ ] Skorlar tüm oyunculara gösterilir
- [ ] Real-time skor güncellemesi
- [ ] Oyun sonunda kazanan gösterimi

### 5.3 Oyun Durumları
- [ ] Lobby (bekleme)
- [ ] Countdown (3-2-1-GO)
- [ ] Playing (oyun devam ediyor)
- [ ] GameOver (kazanan gösterimi)

---

## 🎨 Faz 6: UI ve Görsel İyileştirmeler

### 6.1 Multiplayer UI
- [ ] Lobby ekranı
  - Oyuncu listesi
  - Hazır olma durumu
  - Oda bilgileri
- [ ] Oyun içi UI
  - Kendi skorun
  - Rakip skoru
  - Timer
  - Oyuncu isimleri
- [ ] Oyun sonu ekranı
  - Kazanan gösterimi
  - Skor karşılaştırması
  - Tekrar oyna / Ana menü

### 6.2 Görsel Feedback
- [ ] Network durumu göstergesi
- [ ] Bağlantı kalitesi göstergesi
- [ ] Ping gösterimi

---

## 🔧 Faz 7: Optimizasyon ve Hata Ayıklama

### 7.1 Performans
- [ ] Network mesaj optimizasyonu
- [ ] Gereksiz RPC çağrılarını azalt
- [ ] Object pooling (mermiler için) - Fusion'ın built-in pooling'i
- [ ] Client-side prediction (Fusion'ın built-in özelliği)
- [ ] Tick rate optimizasyonu
- [ ] NetworkProperties compression

### 7.2 Hata Yönetimi
- [ ] Bağlantı kopması durumu
- [ ] Host ayrılması durumu
- [ ] Yeniden bağlanma mekanizması
- [ ] Hata mesajları

### 7.3 Test
- [ ] Local test (2 build)
- [ ] Network test
- [ ] Latency test
- [ ] Stress test

---

## 📝 Detaylı Implementasyon Notları

### Network Manager Yapısı
```csharp
// Ana yapı (Fusion):
- NetworkManager : NetworkBehaviour
  - NetworkRunner runner
  - StartHost() // GameMode.Host
  - StartClient() // GameMode.Client
  - OnPlayerJoined()
  - OnPlayerLeft()
  - Session management
```

### Oyuncu Yapısı
```csharp
// NetworkPlayer (Fusion):
- NetworkBehaviour component
- NetworkTransform (pozisyon senkronizasyonu)
- HasInputAuthority kontrolü
- HasStateAuthority kontrolü
- Spawn pozisyonu (Host: pos1, Client: pos2)
- Kamera yönü ayarı
```

### Skor Sistemi
```csharp
// Her oyuncu için (Fusion):
- NetworkDictionary<PlayerRef, int> playerScores
- NetworkProperties ile otomatik senkronizasyon
- RPC ile skor güncelleme (gerekirse)
- UI'da her iki skor gösterimi
```

### Fusion Temel Konseptler
```csharp
// NetworkBehaviour:
- HasInputAuthority: Local oyuncu mu?
- HasStateAuthority: Bu objeyi kontrol eden oyuncu mu?
- Runner: NetworkRunner referansı
- Runner.Spawn(): Network objesi spawn
- Runner.Despawn(): Network objesi destroy
- [Networked]: Network property
- [Rpc]: Remote procedure call
```

### Oyun Akışı
1. Lobby → Oyuncular katılır
2. Host "Başlat" → Countdown başlar
3. Countdown bitince → Oyun başlar
4. Oyun sırasında → Skorlar güncellenir
5. Timer bitince → GameOver, kazanan gösterilir

---

## 🎯 Öncelik Sırası

1. **YÜKSEK ÖNCELİK:**
   - Photon kurulumu
   - NetworkManager
   - NetworkPlayer (spawn ve kamera)
   - NetworkGameManager (skor ve timer)

2. **ORTA ÖNCELİK:**
   - Network silah ve mermi
   - Network düşman
   - Lobby sistemi

3. **DÜŞÜK ÖNCELİK:**
   - UI iyileştirmeleri
   - Optimizasyon
   - Görsel feedback

---

## 📚 Kaynaklar

- Photon Fusion Dokümantasyon: https://doc.photonengine.com/fusion/current
- Photon Dashboard: https://dashboard.photonengine.com/
- Fusion Örnekleri: https://doc.photonengine.com/fusion/current/game-samples
- Fusion API Referansı: https://doc.photonengine.com/fusion/current/api
- Fusion Best Practices: https://doc.photonengine.com/fusion/current/manual/network-objects

---

## ⚠️ Önemli Notlar

1. **State Authority:** Düşman spawn ve oyun durumu Host tarafından yönetilmeli (`HasStateAuthority`)
2. **Input Authority:** Her oyuncu kendi silahını kontrol eder (`HasInputAuthority`)
3. **Skor:** Her oyuncunun skoru ayrı tutulur, `NetworkProperties` ile senkronize, oyun sonunda karşılaştırılır
4. **Timer:** Host tarafından yönetilir (`HasStateAuthority`), `NetworkProperties` ile tüm oyunculara senkronize edilir
5. **Spawn Pozisyonları:** Host ve Client farklı pozisyonlarda başlar, farklı yönlere bakar
6. **Tick-based:** Fusion tick-based çalışır, `FixedUpdateNetwork()` kullan
7. **Spawn/Despawn:** `Runner.Spawn()` ve `Runner.Despawn()` kullan, normal `Instantiate/Destroy` değil
8. **NetworkProperties:** `[Networked]` attribute ile otomatik senkronizasyon

---

## ✅ Tamamlandığında

- [ ] 2 oyuncu aynı anda oynayabilir
- [ ] Host oyun kurar, Client katılır
- [ ] Her oyuncu farklı tarafta başlar
- [ ] Skorlar senkronize çalışır
- [ ] En yüksek skor kazanır
- [ ] Oyun sonu ekranı kazananı gösterir

