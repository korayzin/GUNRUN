# 🎮 Photon Fusion - SIFIRDAN KURULUM REHBERİ

Bu rehber, hiç online sistem kurmamış biri için hazırlanmıştır. Her adımı sırayla takip et.

---

## 📋 ADIM 1: Photon Dashboard Hesabı Oluşturma

### 1.1 Photon Web Sitesine Git
1. Tarayıcını aç (Chrome, Firefox, Edge - fark etmez)
2. Adres çubuğuna şunu yaz: `https://dashboard.photonengine.com/`
3. Enter'a bas

### 1.2 Hesap Oluştur
1. Sağ üst köşede **"Sign Up"** veya **"Register"** butonuna tıkla
2. Email adresini gir (gerçek bir email, doğrulama kodu gelecek)
3. Şifre oluştur (en az 8 karakter)
4. **"Create Account"** veya **"Sign Up"** butonuna tıkla
5. Email'ine gelen doğrulama linkine tıkla (spam klasörüne bakabilir)

### 1.3 Giriş Yap
1. Email ve şifrenle giriş yap
2. İlk girişte bir tutorial görebilirsin, geçebilirsin (Skip)

---

## 📋 ADIM 2: Fusion App Oluşturma ve App ID Alma

### 2.1 Yeni App Oluştur
1. Dashboard'a giriş yaptıktan sonra, ana sayfada **"Create a new App"** veya **"New App"** butonuna tıkla
2. **"App Name"** kısmına bir isim yaz (örn: "Gunrun Multiplayer")
3. **"Photon Type"** dropdown'ından **"Fusion"** seç (ÖNEMLİ: PUN değil, Fusion!)
4. **"Region"** seç (Türkiye'ye yakın: Europe, Middle East, veya Asia)
5. **"Create"** veya **"Create App"** butonuna tıkla

### 2.2 App ID'yi Kopyala
1. App oluşturulduktan sonra, app'in detay sayfasına yönlendirileceksin
2. Sayfanın üst kısmında **"App ID"** veya **"Application ID"** yazan bir kutu göreceksin
3. İçinde uzun bir kod var (örn: `a1b2c3d4-e5f6-7890-abcd-ef1234567890`)
4. Bu kodu **KOPYALA** (Ctrl+C veya sağ tık > Copy)
5. **BİR YERE NOT ET** (Notepad'e yapıştır, kaydet - çok önemli!)

### 2.3 App Secret'i de Al (Opsiyonel ama önerilir)
1. Aynı sayfada **"App Secret"** veya **"Secret Key"** de görebilirsin
2. Bunu da kopyala ve not et (güvenlik için)

**✅ ADIM 2 TAMAMLANDI: Artık elinde bir App ID var!**

---

## 📋 ADIM 3: Unity'de Photon Fusion Paketini Kurma

**⚠️ ÖNEMLİ:** Photon Fusion'u kurmanın 3 farklı yolu var. En kolayı **YÖNTEM 1** (Asset Store). Eğer o çalışmazsa diğerlerini dene.

---

### 🎯 YÖNTEM 1: Unity Asset Store'dan Kurma (ÖNERİLEN - EN KOLAY)

#### 3.1.1 Asset Store'a Git
1. Unity Editor'de üst menüden: **Window** → **Asset Store**
2. Asset Store penceresi açılacak (tarayıcı gibi görünecek)

#### 3.1.2 Photon Fusion'u Bul
1. Asset Store'un üst kısmındaki **arama kutusuna** şunu yaz: **"Photon Fusion"**
2. Enter'a bas veya arama ikonuna tıkla
3. **"Photon Fusion"** paketini bul (Photon tarafından yayınlanmış olmalı)

#### 3.1.3 Paketi Hesabına Ekle
1. Photon Fusion sayfasında **"Add to My Assets"** veya **"Add to Cart"** butonuna tıkla
2. Eğer Unity hesabına giriş yapmanı isterse, giriş yap
3. Paket hesabına eklenecek (ücretsiz)

#### 3.1.4 Package Manager'dan Kur
1. Unity Editor'de: **Window** → **Package Manager**
2. Package Manager'ın sol üst köşesinde **"Packages: In Project"** yazıyor
3. Yanındaki **dropdown'a tıkla** (aşağı ok)
4. **"My Assets"** seçeneğini seç
5. Listede **"Photon Fusion"** görünecek
6. **"Photon Fusion"** paketini seç
7. Sağ altta **"Install"** butonuna tıkla
8. Unity paketi kurmaya başlayacak (1-3 dakika sürebilir)

**✅ YÖNTEM 1 TAMAMLANDI: Fusion Asset Store'dan kuruldu!**

---

### 🔧 YÖNTEM 2: GitHub'dan Git URL ile Kurma

#### 3.2.1 Package Manager'ı Aç
1. Unity Editor'de: **Window** → **Package Manager**
2. Package Manager penceresi açılacak

#### 3.2.2 Git URL Ekle
1. Package Manager'ın sol üst köşesinde **"+"** işareti var
2. **"+"** butonuna tıkla
3. Açılan menüden **"Add package from git URL..."** seç
4. Bir input kutusu çıkacak

#### 3.2.3 Doğru URL'yi Gir
1. Input kutusuna şunu yapıştır (DİKKAT: .git ile bitiyor):
   ```
   https://github.com/PhotonEngine/Fusion.git?path=/com.photonengine.fusion
   ```
2. **"Add"** butonuna tıkla
3. Unity paketi indirmeye başlayacak (internet hızına göre 2-5 dakika sürebilir)
4. İndirme bitince, Package Manager'da Fusion görünecek

**✅ YÖNTEM 2 TAMAMLANDI: Fusion GitHub'dan kuruldu!**

---

### 📦 YÖNTEM 3: Manuel İndirme ve Kurma (Son Çare)

#### 3.3.1 Fusion'u İndir
1. Tarayıcıda şu adrese git: `https://github.com/PhotonEngine/Fusion/releases`
2. En son sürümü bul (örn: v2.0.0 veya daha yeni)
3. **"Source code (zip)"** veya **"Fusion.unitypackage"** dosyasını indir

#### 3.3.2 Unity'ye Yükle
**Eğer .unitypackage indirdiysen:**
1. Unity Editor'de: **Assets** → **Import Package** → **Custom Package...**
2. İndirdiğin `.unitypackage` dosyasını seç
3. **"Import"** butonuna tıkla
4. Tüm dosyaları import et

**Eğer .zip indirdiysen:**
1. Zip dosyasını aç
2. Unity Editor'de Package Manager'ı aç: **Window** → **Package Manager**
3. **"+"** → **"Add package from disk..."**
4. Açılan zip klasöründe `com.photonengine.fusion` klasörüne git
5. `package.json` dosyasını seç
6. Unity paketi kuracak

**✅ YÖNTEM 3 TAMAMLANDI: Fusion manuel olarak kuruldu!**

---

### ✅ Kurulum Kontrolü

Hangi yöntemi kullanırsan kullan, şunu kontrol et:

1. Package Manager'da **"In Project"** seçiliyken
2. Listede **"Photon Fusion"** görünüyor mu?
3. Eğer görünüyorsa → ✅ **BAŞARILI!**
4. Eğer görmüyorsan → Yöntemleri tekrar dene

**✅ ADIM 3 TAMAMLANDI: Fusion paketi Unity'de kuruldu!**

---

## 📋 ADIM 4: NetworkRunner Oluşturma (Fusion Hub ile)

**⚠️ ÖNEMLİ:** Fusion'ın yeni versiyonlarında FusionLauncher yok! Bunun yerine **Fusion Hub** kullanıyoruz veya manuel **NetworkRunner** oluşturuyoruz.

---

### 🎯 YÖNTEM 1: Network Runner Controls ile (ÖNERİLEN)

#### 4.1.1 Network Runner Controls'u Aç
1. Unity Editor'de üst menüden: **Window** → **Fusion** → **Network Runner Controls**
2. Network Runner Controls penceresi açılacak (küçük bir pencere)

#### 4.1.2 NetworkRunner Oluştur
1. Network Runner Controls penceresinde **"Create Runner"** veya benzer bir buton gör
2. Bu butona tıkla
3. Unity otomatik olarak sahneye bir **NetworkRunner** GameObject'i ekleyecek

**NOT:** Eğer Network Runner Controls'da buton görmüyorsan → **YÖNTEM 2'ye geç** (Manuel oluşturma)

**✅ YÖNTEM 1 TAMAMLANDI: NetworkRunner Network Runner Controls ile oluşturuldu!**

---

### 🔧 YÖNTEM 2: Manuel NetworkRunner Oluşturma

Eğer Fusion Hub çalışmazsa veya göremiyorsan:

#### 4.2.1 Boş GameObject Oluştur
1. Hierarchy penceresinde (sol altta) **sağ tıkla**
2. **Create Empty** seçeneğine tıkla
3. Yeni GameObject oluşacak, ismini **"NetworkRunner"** yap (Inspector'da üstteki isim kutusundan)

#### 4.2.2 NetworkRunner Component Ekle
1. Hierarchy'de **NetworkRunner** GameObject'ini seç
2. Inspector penceresinde (sağ tarafta) en altta **"Add Component"** butonuna tıkla
3. Arama kutusuna **"Network Runner"** yaz
4. **"Network Runner"** component'ini bul ve tıkla
5. Component eklenecek

#### 4.2.3 NetworkRunner Ayarlarını Yap
1. Inspector'da **Network Runner** component'ini gör
2. **"Game Mode"** dropdown'ından seç:
   - **"Host"** → Sunucu olmak için
   - **"Client"** → Katılmak için
   - **"Shared"** → Shared mode için
3. **"Start On Awake"** checkbox'ını işaretle (otomatik başlatmak için)

**✅ YÖNTEM 2 TAMAMLANDI: NetworkRunner manuel olarak oluşturuldu!**

---

### ✅ Kontrol Et

Hangi yöntemi kullanırsan kullan:

1. Hierarchy'de **NetworkRunner** adında bir GameObject var mı?
2. Inspector'da **Network Runner** component'i var mı?
3. Eğer varsa → ✅ **BAŞARILI!**

**✅ ADIM 4 TAMAMLANDI: NetworkRunner sahneye eklendi!**

---

## 📋 ADIM 5: PhotonAppSettings'e App ID Ekleme

**⚠️ ÖNEMLİ:** App ID, NetworkProjectConfig'de değil, **PhotonAppSettings** dosyasında! Bu dosyayı bulmalısın.

### 5.1 PhotonAppSettings Dosyasını Bul
1. Unity Editor'de Project penceresinde (sol üstte) arama kutusuna **"PhotonAppSettings"** yaz
2. **"PhotonAppSettings.asset"** dosyasını bul
3. Dosya genelde şu klasörde: `Assets/Photon/Fusion/Resources/`

### 5.2 PhotonAppSettings'i Aç
1. **PhotonAppSettings.asset** dosyasına **çift tıkla**
2. Inspector'da ayarlar açılacak
3. Inspector'da **"Photon App Settings"** veya **"App Settings"** bölümünü gör

### 5.3 App ID'yi Gir
1. Inspector'da **"App Id Fusion"** yazan input kutusunu bul
2. Bu kutu muhtemelen **boş** görünecek
3. **ADIM 2'de kopyaladığın App ID'yi buraya yapıştır** (Ctrl+V)
   - App ID uzun bir kod olmalı (örn: `a1b2c3d4-e5f6-7890-abcd-ef1234567890`)

### 5.4 Region'ı Ayarla (Önemli!)
1. Inspector'da **"Fixed Region"** veya **"Region"** yazan input kutusunu bul
2. Photon Dashboard'da seçtiğin region'ı buraya yaz:
   - **"eu"** → Europe
   - **"us"** → US
   - **"asia"** → Asia
   - **"sa"** → South America
   - Boş bırakırsan otomatik en iyi region'ı seçer

### 5.5 Kaydet
1. Inspector'u kapat (otomatik kaydedilir)
2. Veya üst menüden: **File** → **Save** (Ctrl+S)

**✅ ADIM 5 TAMAMLANDI: App ID PhotonAppSettings'e eklendi!**

---

### 🔍 Eğer PhotonAppSettings Bulamazsan:

1. Project penceresinde **"PhotonAppSettings"** araması sonuç vermiyorsa:
2. Şu klasöre git: `Assets/Photon/Fusion/Resources/`
3. Orada **"PhotonAppSettings.asset"** dosyasını bul
4. Çift tıkla ve App ID'yi gir

**NOT:** NetworkProjectConfig'i de açık tutabilirsin, ama App ID'yi **PhotonAppSettings**'e girmen gerekiyor!

---

## 📋 ADIM 6: İlk Bağlantı Testi

### 6.1 Test Sahnesi Hazırla
1. Mevcut sahneni kaydet: **File** → **Save** (Ctrl+S)
2. Hierarchy'de FusionLauncher'ın olduğundan emin ol

### 6.2 Play Butonuna Bas
1. Unity Editor'ün üst ortasında **▶️ Play** butonuna bas
2. Oyun başlayacak

### 6.3 Console'u Kontrol Et
1. Unity Editor'ün alt kısmında **Console** penceresi var
2. Eğer görmüyorsan: **Window** → **General** → **Console**
3. Console'da şunları ara:
   - **"Connected"** veya **"Connection successful"** mesajı
   - **"Fusion started"** mesajı
   - **Kırmızı hata mesajları** (varsa not et)

### 6.4 Başarılı Bağlantı Kontrolü
**BAŞARILI İSE:**
- Console'da yeşil/başarı mesajları görürsün
- Hata yoksa, bağlantı çalışıyor demektir!

**HATA VARSA:**
- Console'da kırmızı mesajlar görürsün
- Hata mesajını oku ve not et
- En yaygın hatalar:
  - **"App ID not found"** → ADIM 5'i tekrar kontrol et
  - **"Connection failed"** → İnternet bağlantını kontrol et
  - **"Invalid App ID"** → App ID'yi tekrar kopyala-yapıştır

### 6.5 Play'i Durdur
1. **▶️ Play** butonuna tekrar bas (durduracak)
2. Veya **Esc** tuşuna bas

**✅ ADIM 6 TAMAMLANDI: İlk test yapıldı!**

---

## 🎉 TEBRİKLER!

Fusion kurulumu tamamlandı! Artık online sistemin hazır.

### Şimdi Ne Yapmalı?
1. ✅ Fusion kuruldu
2. ✅ App ID yapılandırıldı
3. ✅ İlk bağlantı test edildi

### Sonraki Adımlar:
- NetworkManager scripti oluşturma
- NetworkPlayer scripti oluşturma
- Spawn sistemini kurma

---

## ❓ SIK SORULAN SORULAR

### Q: App ID'yi nerede bulurum?
**A:** Photon Dashboard → App'in detay sayfası → Üstte "App ID" yazan kutu

### Q: Fusion paketi bulamıyorum?
**A:** Package Manager'da "In Project" yerine "Unity Registry" seç, sonra "Photon Fusion" ara

### Q: FusionLauncher prefab'ı yok?
**A:** Fusion paketi kurulduktan sonra Project'te ara. Eğer yoksa, Hierarchy'de sağ tık → Create Empty → FusionLauncher ekle, sonra FusionLauncher scriptini ekle

### Q: Bağlantı hatası alıyorum?
**A:** 
1. App ID'yi kontrol et (doğru mu?)
2. İnternet bağlantını kontrol et
3. Photon Dashboard'da app'in aktif olduğundan emin ol
4. Console'daki hata mesajını oku

### Q: NetworkProjectConfig bulamıyorum?
**A:** Project'te arama yap. Eğer yoksa, Assets klasöründe sağ tık → Create → Photon Fusion → Network Project Config

---

## 📞 YARDIM GEREKİRSE

Eğer bir adımda takılırsan:
1. Console'daki hata mesajını oku
2. Hangi adımda olduğunu söyle
3. Hata mesajını paylaş
4. Birlikte çözelim!

**Şimdi ADIM 1'den başla ve her adımı tamamladıktan sonra bana haber ver! 🚀**

