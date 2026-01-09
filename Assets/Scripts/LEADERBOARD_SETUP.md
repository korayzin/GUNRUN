# 🏆 Leaderboard Kurulum Rehberi

## 📋 Özellikler

✅ Firebase Realtime Database ile tam entegrasyon
✅ Tüm oyuncuların max puanlarını görüntüleme
✅ Scroll view ile sınırsız oyuncu listesi
✅ Otomatik sıralama (en yüksek puandan düşüğe)
✅ Altın/Gümüş/Bronz madalya desteği
✅ Kendi max skorunu ayrı gösterme
✅ Test oyuncuları ekleme/silme fonksiyonları

## 🎮 Unity UI Kurulumu

### 1. Temel Yapı Oluştur

Scene'inize şu yapıyı oluşturun:

```
Canvas
  └── LeaderboardPanel (Panel)
      ├── Title (TextMeshPro - Text)
      ├── MyScoreText (TextMeshPro - Text)
      ├── ScrollView (Scroll View)
      │   └── Viewport
      │       └── Content
      ├── RefreshButton (Button)
      ├── CloseButton (Button)
      └── LoadingText (TextMeshPro - Text)
```

### 2. Detaylı Ayarlar

#### **LeaderboardPanel**
- Ekleme: `Hierarchy > Right Click > UI > Panel`
- RectTransform: Anchor Presets = Center, Stretch (her iki ok)
- Margin: Left=100, Right=100, Top=50, Bottom=50
- Image: Color = Yarı şeffaf siyah (R:0, G:0, B:0, A:200)

#### **Title**
- Ekleme: `Hierarchy > Right Click > UI > TextMeshPro - Text`
- Text: "🏆 LEADERBOARD 🏆"
- Font Size: 36
- Alignment: Center
- RectTransform: Anchor = Top Center, Height = 60

#### **MyScoreText**
- Ekleme: `Hierarchy > Right Click > UI > TextMeshPro - Text`
- Text: "Senin Max Skorun: 0"
- Font Size: 24
- Alignment: Center
- RectTransform: Anchor = Top Center, PosY = -70, Height = 40

#### **ScrollView**
1. Ekleme: `Hierarchy > Right Click > UI > Scroll View`
2. **ScrollView** ayarları:
   - RectTransform: Anchor = Stretch Both
   - Margin: Left=20, Right=20, Top=120, Bottom=80
   - Scroll Rect: 
     - ✅ Vertical = true
     - ❌ Horizontal = false
     - Movement Type = Elastic
     - Elasticity = 0.1
     - Inertia = true
     - Deceleration Rate = 0.135

3. **Viewport** (otomatik oluşturulur):
   - Mask component olmalı
   - Image component olmalı

4. **Content** (otomatik oluşturulur):
   - **ÖNEMLİ**: Bu component'i `LeaderboardUI` script'ine atayacaksınız!
   - RectTransform: 
     - Anchor = Top Stretch
     - Pivot = X:0.5, Y:1
   - Not: VerticalLayoutGroup ve ContentSizeFitter otomatik eklenecek

#### **RefreshButton**
- Ekleme: `Hierarchy > Right Click > UI > Button - TextMeshPro`
- Button Text: "🔄 Yenile"
- RectTransform: Anchor = Bottom Right
- Size: Width=150, Height=50
- Position: X=-100, Y=30

#### **CloseButton**
- Ekleme: `Hierarchy > Right Click > UI > Button - TextMeshPro`
- Button Text: "✖ Kapat"
- RectTransform: Anchor = Top Right
- Size: Width=100, Height=50
- Position: X=-20, Y=-20

#### **LoadingText**
- Ekleme: `Hierarchy > Right Click > UI > TextMeshPro - Text`
- Text: "Yükleniyor..."
- Font Size: 28
- Alignment: Center
- RectTransform: Anchor = Center
- Color: Sarı
- Varsayılan: **Inactive** (checkbox'u kapat)

### 3. Script Ayarları

#### **FirebaseLeaderboardManager**
1. Boş bir GameObject oluştur: `Hierarchy > Right Click > Create Empty`
2. İsim: "FirebaseManager"
3. Script ekle: `FirebaseLeaderboardManager`
4. Inspector'da:
   - Firebase Database Url: `https://gunrundata-default-rtdb.europe-west1.firebasedatabase.app`

#### **LeaderboardUI**
1. LeaderboardPanel'e script ekle: `LeaderboardUI`
2. Inspector'da referansları ata:
   - **Leaderboard Panel**: LeaderboardPanel (kendi kendisi)
   - **Leaderboard Content**: ScrollView/Viewport/Content ⚠️ ÇOK ÖNEMLİ!
   - **Leaderboard Entry Prefab**: (Boş bırakılabilir - otomatik oluşturulacak)
   - **Loading Text**: LoadingText
   - **My Score Text**: MyScoreText
   - **Refresh Button**: RefreshButton
   - **Max Entries**: 50

### 4. Test Butonları (Opsiyonel)

Leaderboard'u test etmek için debug butonları ekleyebilirsiniz:

#### **AddTestPlayersButton**
- Button Text: "➕ Test Oyuncuları Ekle"
- LeaderboardUI Inspector'da: `Add Test Players Button` = bu butona ata
- Fonksiyon: 10 rastgele test oyuncusu ekler

#### **ClearTestPlayersButton**
- Button Text: "🗑️ Test Oyuncuları Sil"
- LeaderboardUI Inspector'da: `Clear Test Players Button` = bu butona ata
- Fonksiyon: Tüm test oyuncuları siler

## 🎨 Görsel İyileştirmeler (Opsiyonel)

### Renk Şeması
- **Panel Arka Plan**: Koyu gri, yarı şeffaf
- **Scroll View Arka Plan**: Hafif gri
- **Header**: Koyu siyah
- **1. Sıra**: Altın sarısı
- **2. Sıra**: Gümüş grisi
- **3. Sıra**: Bronz kahverengisi

### Font Ayarları
- **Title**: 36-42pt, Bold
- **Header**: 18-20pt, Bold
- **Entry**: 18-20pt, Normal
- **Score**: 20pt, Bold

## 🔧 Kullanım

### Leaderboard'u Açmak
```csharp
LeaderboardUI leaderboardUI = FindObjectOfType<LeaderboardUI>();
leaderboardUI.ShowLeaderboard();
```

### Leaderboard'u Kapatmak
```csharp
leaderboardUI.HideLeaderboard();
```

### Oyuncu Puanı Kaydetmek
```csharp
FirebaseLeaderboardManager.Instance.SaveMaxScore(scoreValue, (success) => {
    if (success) {
        Debug.Log("Puan kaydedildi!");
    }
});
```

### Manuel Yenileme
```csharp
leaderboardUI.LoadLeaderboard();
```

## 🐛 Sorun Giderme

### Problem: Leaderboard boş görünüyor
**Çözüm**:
1. Console'da Firebase loglarını kontrol et
2. Firebase Database URL'i doğru mu kontrol et
3. Internet bağlantısı var mı kontrol et
4. Test oyuncuları ekle: `AddTestPlayers()` butonuna tıkla

### Problem: ScrollView çalışmıyor
**Çözüm**:
1. Content'e VerticalLayoutGroup eklendi mi kontrol et (otomatik ekleniyor)
2. Content'e ContentSizeFitter eklendi mi kontrol et (otomatik ekleniyor)
3. ScrollView'ın Vertical checkbox'u işaretli mi kontrol et

### Problem: Script referansları yok
**Çözüm**:
1. LeaderboardUI script'i otomatik referans bulmayı dener
2. Manuel olarak atamak için Unity Inspector'ı kullan
3. Console'da otomatik bulma loglarını kontrol et

### Problem: Firebase'e kayıt olmuyor
**Çözüm**:
1. Firebase Database URL'i doğru mu?
2. Firebase Database Rules kontrol et:
```json
{
  "rules": {
    "leaderboard": {
      ".read": true,
      ".write": true
    }
  }
}
```

## 📊 Firebase Database Yapısı

```json
{
  "leaderboard": {
    "player_id_1": {
      "playerName": "Ali123",
      "maxScore": 1500,
      "timestamp": 1704841200,
      "playerId": "player_id_1"
    },
    "player_id_2": {
      "playerName": "Ayşe456",
      "maxScore": 2300,
      "timestamp": 1704841300,
      "playerId": "player_id_2"
    }
  }
}
```

## ✅ Test Adımları

1. ✅ Scene'i çalıştır
2. ✅ "Test Oyuncuları Ekle" butonuna tıkla
3. ✅ 2-3 saniye bekle
4. ✅ "Yenile" butonuna tıkla
5. ✅ Leaderboard'da oyuncuları gör
6. ✅ Scroll yaparak test et
7. ✅ Kendi oyununu oyna ve puan kaydet
8. ✅ Leaderboard'da kendi skorunu gör

## 🎯 Özellikler

- ✅ **Otomatik Sıralama**: En yüksek puandan düşüğe
- ✅ **Madalyalar**: İlk 3 sıra özel renk ve emoji
- ✅ **Alternatif Satır Renkleri**: Daha iyi görünüm
- ✅ **Binlik Ayırıcılar**: Puanlar okunabilir formatta
- ✅ **Kendi Skorun**: Ayrı bir alanda gösterilir
- ✅ **Scroll View**: Sınırsız oyuncu sayısı
- ✅ **Otomatik Referanslar**: UI elemanları otomatik bulunur
- ✅ **Debug Tools**: Test oyuncuları ekleme/silme

## 💡 İpuçları

1. **Test Oyuncuları**: Geliştirme sırasında test oyuncuları kullan
2. **Firebase Rules**: Production'da daha güvenli rules kullan
3. **Rate Limiting**: Çok sık refresh yapmaktan kaçın
4. **Caching**: Gelecekte client-side caching ekleyebilirsin
5. **Pagination**: 100+ oyuncu için pagination ekleyebilirsin

## 📝 Notlar

- Script'ler otomatik olarak VerticalLayoutGroup ve ContentSizeFitter ekler
- Prefab kullanmak opsiyoneldir, otomatik entry oluşturma mevcuttur
- Tüm debug logları Console'da görülebilir
- Firebase bağlantısı async olarak çalışır

Başarılar! 🚀
