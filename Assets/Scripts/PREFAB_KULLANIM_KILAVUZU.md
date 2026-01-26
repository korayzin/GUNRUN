# Leaderboard Prefab Kullanım Kılavuzu

Bu kılavuz, leaderboard için özel header ve entry tasarımları oluşturup kullanmanızı sağlar.

## 📋 Genel Bakış

Leaderboard sisteminde 2 tür prefab kullanabilirsiniz:
1. **Header Prefab** - Leaderboard'un en üstündeki başlık satırı (SIRA, OYUNCU ADI, PUAN)
2. **Entry Prefab** - Her oyuncu için gösterilen satır tasarımı

## 🎨 1. ENTRY PREFAB OLUŞTURMA (Oyuncu Satırı)

### Adım 1: Yeni GameObject Oluştur
1. Unity Hierarchy'de sağ tık → **UI → Panel** (veya boş GameObject)
2. İsmini `LeaderboardEntryPrefab` yap

### Adım 2: Tasarımı Yap
Entry prefab'ınızda şunlar olmalı:

#### Zorunlu Bileşenler:
- **RectTransform** (otomatik eklenir)
- **Image** (arka plan için - opsiyonel ama önerilir)
- **LeaderboardEntryUI** script'i (otomatik eklenir, manuel eklemenize gerek yok)

#### Text Alanları (3 adet TextMeshProUGUI):
1. **Rank Text** (Sıra numarası için)
   - GameObject ismi: `RankText` veya `Sira` veya `rank` içermeli
   - İçerik: Otomatik doldurulur (örn: "🥇 1.")

2. **Name Text** (Oyuncu adı için)
   - GameObject ismi: `NameText` veya `Isim` veya `Oyuncu` veya `name` içermeli
   - İçerik: Otomatik doldurulur (örn: "Player123")

3. **Score Text** (Puan için)
   - GameObject ismi: `ScoreText` veya `Puan` veya `score` içermeli
   - İçerik: Otomatik doldurulur (örn: "1,234")

### Adım 3: Layout Ayarları
- **Horizontal Layout Group** ekleyin (spacing, padding ayarlayın)
- Veya **Grid Layout Group** kullanabilirsiniz
- Her text için **Layout Element** ekleyip genişlik ayarlayın

### Adım 4: Prefab'a Çevir
1. Tasarımınızı tamamladıktan sonra
2. Project penceresinde bir klasör oluşturun (örn: `Prefabs/Leaderboard`)
3. Hierarchy'deki GameObject'i Project penceresine sürükleyip bırakın
4. Prefab oluşturuldu! 🎉

### Örnek Entry Prefab Yapısı:
```
LeaderboardEntryPrefab
├── Image (Background)
├── Horizontal Layout Group
├── RankText (TextMeshProUGUI)
├── NameText (TextMeshProUGUI)
└── ScoreText (TextMeshProUGUI)
```

## 🎨 2. HEADER PREFAB OLUŞTURMA (Başlık Satırı)

### Adım 1: Yeni GameObject Oluştur
1. Unity Hierarchy'de sağ tık → **UI → Panel**
2. İsmini `LeaderboardHeaderPrefab` yap

### Adım 2: Tasarımı Yap
Header prefab'ınızda şunlar olmalı:

#### Zorunlu Bileşenler:
- **RectTransform** (otomatik eklenir)
- **Image** (arka plan için - opsiyonel)

#### Text Alanları (3 adet TextMeshProUGUI):
1. **Rank Header** (SIRA başlığı)
   - GameObject ismi: `RankHeader` veya `SiraHeader`
   - İçerik: "SIRA" (veya istediğiniz metin)

2. **Name Header** (OYUNCU ADI başlığı)
   - GameObject ismi: `NameHeader` veya `IsimHeader`
   - İçerik: "OYUNCU ADI" (veya istediğiniz metin)

3. **Score Header** (PUAN başlığı)
   - GameObject ismi: `ScoreHeader` veya `PuanHeader`
   - İçerik: "PUAN" (veya istediğiniz metin)

### Adım 3: Layout Ayarları
- **Horizontal Layout Group** ekleyin
- Her header text için **Layout Element** ekleyip genişlik ayarlayın

### Adım 4: Prefab'a Çevir
1. Tasarımınızı tamamladıktan sonra
2. Project penceresindeki `Prefabs/Leaderboard` klasörüne sürükleyin
3. Prefab oluşturuldu! 🎉

### Örnek Header Prefab Yapısı:
```
LeaderboardHeaderPrefab
├── Image (Background)
├── Horizontal Layout Group
├── RankHeader (TextMeshProUGUI) - "SIRA"
├── NameHeader (TextMeshProUGUI) - "OYUNCU ADI"
└── ScoreHeader (TextMeshProUGUI) - "PUAN"
```

## 🔧 3. PREFAB'LARI ATAMA

### Adım 1: LeaderboardUI Script'ini Bul
1. Hierarchy'de `LeaderboardUI` component'ine sahip GameObject'i seçin
2. Inspector penceresinde `LeaderboardUI` script'ini göreceksiniz

### Adım 2: Prefab'ları Atayın
Inspector'da şu alanları göreceksiniz:

- **Leaderboard Entry Prefab**: Oluşturduğunuz entry prefab'ını buraya sürükleyin
- **Leaderboard Header Prefab**: Oluşturduğunuz header prefab'ını buraya sürükleyin

### Adım 3: Test Edin
1. Play moduna geçin
2. Leaderboard açıldığında özel tasarımlarınız görünecek!

## ⚠️ ÖNEMLİ NOTLAR

### Entry Prefab İçin:
- ✅ TextMeshProUGUI component'lerinin isimlerinde `rank`, `name`, `score` kelimelerinden biri olmalı (Türkçe: `sira`, `isim`, `puan`, `oyuncu`)
- ✅ En az 3 TextMeshProUGUI olmalı (rank, name, score için)
- ✅ `LeaderboardEntryUI` script'i otomatik eklenir, manuel eklemenize gerek yok
- ✅ Text'leri Inspector'da manuel atamak isterseniz, `LeaderboardEntryUI` component'ine ekleyip atayabilirsiniz

### Header Prefab İçin:
- ✅ Header için özel bir script gerekmez
- ✅ Text içerikleri istediğiniz gibi olabilir (örn: "SIRA", "#", "RANK" vs.)
- ✅ Sadece görsel tasarım önemli, veri otomatik doldurulmaz

### Prefab Atanmazsa:
- ❌ Eğer prefab atamazsanız, sistem otomatik olarak varsayılan tasarımı oluşturur
- ✅ Bu durumda kod çalışmaya devam eder, sadece varsayılan görünüm kullanılır

## 🎯 HIZLI BAŞLANGIÇ ÖRNEĞİ

### Entry Prefab (5 dakikada):
1. Hierarchy → Sağ tık → **UI → Panel** → İsim: `EntryPrefab`
2. Panel'e **Horizontal Layout Group** ekle
3. Panel içine 3 tane **UI → Text - TextMeshPro** ekle:
   - `RankText` (genişlik: 70)
   - `NameText` (genişlik: 200)
   - `ScoreText` (genişlik: 100)
4. Project'e sürükle → Prefab oluştu!
5. LeaderboardUI'da **Leaderboard Entry Prefab** alanına at

### Header Prefab (3 dakikada):
1. Hierarchy → Sağ tık → **UI → Panel** → İsim: `HeaderPrefab`
2. Panel'e **Horizontal Layout Group** ekle
3. Panel içine 3 tane **UI → Text - TextMeshPro** ekle:
   - `RankHeader` → Text: "SIRA"
   - `NameHeader` → Text: "OYUNCU ADI"
   - `ScoreHeader` → Text: "PUAN"
4. Project'e sürükle → Prefab oluştu!
5. LeaderboardUI'da **Leaderboard Header Prefab** alanına at

## 🐛 SORUN GİDERME

**Problem:** Entry'ler görünmüyor
- ✅ Prefab'ın RectTransform'u doğru mu kontrol edin
- ✅ TextMeshProUGUI component'leri var mı kontrol edin
- ✅ GameObject isimlerinde `rank`, `name`, `score` kelimeleri var mı?

**Problem:** Veriler doldurulmuyor
- ✅ TextMeshProUGUI component'leri doğru isimlendirilmiş mi?
- ✅ Console'da hata var mı kontrol edin
- ✅ `LeaderboardEntryUI` script'i otomatik eklenmiş mi?

**Problem:** Header görünmüyor
- ✅ Prefab doğru atanmış mı?
- ✅ RectTransform ayarları doğru mu?

## 💡 İPUÇLARI

- Entry prefab'ınızı test etmek için önce Hierarchy'de oluşturup tasarlayın, sonra prefab'a çevirin
- Text'lerin isimlerini doğru yapın, böylece otomatik bulma çalışır
- Layout Group kullanarak responsive tasarım yapın
- Prefab'ları bir klasörde toplayın (örn: `Assets/Prefabs/Leaderboard/`)
