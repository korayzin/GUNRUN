# 🔥 Firebase Leaderboard Kurulum Rehberi

## 1. Firebase Projesi Oluşturma

1. [Firebase Console](https://console.firebase.google.com/)'a git
2. "Add project" veya "Proje Ekle" butonuna tıkla
3. Proje adını gir (örn: "GUNRUN")
4. Google Analytics'i isteğe bağlı olarak etkinleştir
5. Projeyi oluştur

## 2. Realtime Database Kurulumu

1. Firebase Console'da sol menüden **Realtime Database** seç
2. **Create Database** butonuna tıkla
3. **Test mode** seç (geliştirme için) veya **Production mode** (güvenlik kuralları ile)
4. Bölge seç (örn: `europe-west1` veya `us-central1`)
5. Database oluşturulduktan sonra **Database URL**'i kopyala
   - Format: `https://PROJECT-ID-default-rtdb.REGION.firebasedatabase.app`

## 3. Unity'de Firebase URL Ayarlama

1. Unity Editor'da `FirebaseLeaderboardManager` script'ini bul
2. Inspector'da **Firebase Database Url** alanına Firebase URL'ini yapıştır
   - Örnek: `https://gunrun-default-rtdb.europe-west1.firebasedatabase.app`

## 4. Database Kuralları (Security Rules)

Firebase Console → Realtime Database → Rules sekmesine git:

### Test Mode (Geliştirme için - GÜVENSİZ):
```json
{
  "rules": {
    ".read": true,
    ".write": true
  }
}
```

### Production Mode (Güvenli - Önerilen):
```json
{
  "rules": {
    "leaderboard": {
      ".read": true,
      "$playerId": {
        ".write": "!data.exists() || data.child('maxScore').val() < newData.child('maxScore').val()"
      }
    }
  }
}
```

Bu kural:
- ✅ Herkes leaderboard'u okuyabilir
- ✅ Sadece yeni score daha yüksekse yazılabilir (cheat koruması)

## 5. Unity Scene'de Kurulum

1. **FirebaseLeaderboardManager** GameObject oluştur:
   - Hierarchy'de sağ tık → Create Empty
   - İsmi: `FirebaseLeaderboardManager`
   - `FirebaseLeaderboardManager` script'ini ekle
   - Inspector'da Firebase URL'ini ayarla

2. **LeaderboardUI** kurulumu:
   - Leaderboard panel'i oluştur
   - `LeaderboardUI` script'ini ekle
   - UI referanslarını Inspector'dan ata:
     - Leaderboard Panel
     - Content (ScrollView içindeki)
     - Entry Prefab (opsiyonel)
     - Loading Text
     - My Score Text
     - Refresh Button

## 6. Test Etme

1. Oyunu çalıştır
2. Oyunu bitir (score al)
3. Console'da "✅ Max score kaydedildi" mesajını kontrol et
4. Firebase Console → Realtime Database'de veriyi kontrol et
5. Leaderboard UI'yi aç ve skorları görüntüle

## 7. Veri Yapısı

Firebase'de veri şu şekilde saklanır:

```
leaderboard/
  └── playerId1/
      ├── playerName: "Oyuncu1"
      ├── maxScore: 5000
      ├── timestamp: 1234567890
      └── playerId: "playerId1"
```

## 8. Sorun Giderme

### "Firebase Get Error" hatası:
- ✅ Firebase URL'in doğru olduğundan emin ol
- ✅ Internet bağlantısını kontrol et
- ✅ Firebase Console'da database'in aktif olduğunu kontrol et

### "Firebase Save Error" hatası:
- ✅ Database kurallarını kontrol et (write izni var mı?)
- ✅ Firebase URL'in doğru olduğundan emin ol

### Leaderboard boş görünüyor:
- ✅ Firebase Console'da veri var mı kontrol et
- ✅ Internet bağlantısını kontrol et
- ✅ Console loglarını kontrol et

## 9. Player Name Ayarlama

Oyuncu ismini ayarlamak için:

```csharp
FirebaseLeaderboardManager.Instance.SetPlayerName("OyuncuAdı");
```

Veya UI'de input field ile:
- Input field'dan ismi al
- `FirebaseLeaderboardManager.Instance.SetPlayerName(inputText)` çağır

## 10. Önemli Notlar

⚠️ **Test Mode güvensizdir!** Sadece geliştirme için kullan.
⚠️ Production'da mutlaka güvenlik kuralları ekle.
⚠️ Firebase ücretsiz tier'da günlük limitler var (100K okuma, 20K yazma).
