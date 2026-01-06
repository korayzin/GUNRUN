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

### 3.1 Unity Package Manager'ı Aç
1. Unity Editor'ü aç (projen açık olmalı)
2. Üst menüden: **Window** → **Package Manager**
3. Package Manager penceresi açılacak

### 3.2 Photon Fusion Paketini Bul
1. Package Manager penceresinin sol üst köşesinde **"+"** işareti var
2. **"+"** butonuna tıkla
3. Açılan menüden **"Add package from git URL..."** seç
4. Bir input kutusu çıkacak

### 3.3 Fusion Paket URL'ini Gir
1. Input kutusuna şunu yapıştır:
   ```
   https://registry.npmjs.com/com.photonengine.fusion/-/com.photonengine.fusion-2.0.0.tgz
   ```
2. **"Add"** butonuna tıkla
3. Unity paketi indirmeye başlayacak (internet hızına göre 1-5 dakika sürebilir)
4. İndirme bitince, Package Manager'da Fusion görünecek

**ALTERNATIF YOL (Eğer yukarıdaki çalışmazsa):**
1. Package Manager'da sol üstte **"Packages: In Project"** yazıyor, yanındaki dropdown'a tıkla
2. **"My Registries"** veya **"Unity Registry"** seç
3. Arama kutusuna **"Photon Fusion"** yaz
4. **"Photon Fusion"** paketini bul ve **"Install"** butonuna tıkla

**✅ ADIM 3 TAMAMLANDI: Fusion paketi Unity'de kuruldu!**

---

## 📋 ADIM 4: FusionLauncher Prefab'ını Sahneye Ekleme

### 4.1 FusionLauncher'ı Bul
1. Unity Editor'de, sol üstteki **Project** penceresinde (Assets klasörü görünüyor)
2. Üstteki arama kutusuna **"FusionLauncher"** yaz
3. Bir prefab bulacaksın (mavi küp ikonu)

### 4.2 Sahneye Ekle
1. **FusionLauncher** prefab'ını **sürükle-bırak** ile Hierarchy penceresine bırak
   - Hierarchy penceresi: Sol altta, Scene görünümünün yanında
2. Veya Hierarchy'de sağ tık → **Create Empty** → İsmini **"FusionLauncher"** yap
3. Sonra Project'ten FusionLauncher prefab'ını bu GameObject'e sürükle

### 4.3 FusionLauncher Ayarlarını Kontrol Et
1. Hierarchy'de **FusionLauncher**'ı seç
2. Sağ tarafta **Inspector** penceresinde ayarları gör
3. **"Game Mode"** ayarını kontrol et:
   - **"Auto Host Or Client"** seçili olmalı (başlangıç için)
   - Veya **"Host"** (sunucu olmak için)
   - Veya **"Client"** (katılmak için)

**✅ ADIM 4 TAMAMLANDI: FusionLauncher sahneye eklendi!**

---

## 📋 ADIM 5: NetworkProjectConfig Yapılandırma

### 5.1 NetworkProjectConfig Dosyasını Bul
1. Project penceresinde arama kutusuna **"NetworkProjectConfig"** yaz
2. Bir asset dosyası bulacaksın (beyaz sayfa ikonu)

### 5.2 NetworkProjectConfig'i Aç
1. **NetworkProjectConfig** asset'ine **çift tıkla**
2. Inspector'da ayarlar açılacak

### 5.3 App ID'yi Gir
1. Inspector'da **"Photon App Settings"** bölümünü bul
2. **"App Id Fusion"** veya **"App ID"** yazan input kutusunu bul
3. **ADIM 2'de kopyaladığın App ID'yi buraya yapıştır** (Ctrl+V)
4. Eğer **"App Id Realtime"** diye bir kutu varsa, aynı ID'yi oraya da yapıştır

### 5.4 Diğer Ayarları Kontrol Et
1. **"Region"** ayarını kontrol et (Dashboard'da seçtiğin region ile aynı olmalı)
2. **"Fixed Update Rate"** genelde **30** veya **60** olur (değiştirme şimdilik)
3. **"Network Tick Rate"** genelde **30** olur (değiştirme şimdilik)

### 5.5 Kaydet
1. Üst menüden: **File** → **Save** (veya Ctrl+S)
2. Veya sadece Inspector'u kapat (otomatik kaydedilir)

**✅ ADIM 5 TAMAMLANDI: App ID yapılandırıldı!**

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

