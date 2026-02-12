# UserName Sahnesi UI Kurulumu

UserName sahnesi oyunun ilk sahnesidir. Kullanıcı adı bir kere girilir ve leaderboard'da bu isim kullanılır.

## 1. UserName.unity sahnesini aç

`Assets/Gunrun 2.0/Scenes 2.0/UserName.unity`

## 2. Canvas ve EventSystem ekle

- **GameObject → UI → Canvas** (EventSystem otomatik eklenir)
- Canvas ayarları: Render Mode = Screen Space - Overlay, Canvas Scaler = Scale With Screen Size, Reference Resolution 1920x1080, Match = 0.5

## 3. UI öğeleri (Canvas altında)

1. **Başlık (Text - TextMeshPro veya UI Text)**  
   - Örn. "Kullanıcı adınız" veya "İsminizi girin"

2. **İsim girişi**  
   - **GameObject → UI → Input Field - TextMeshPro** (veya legacy Input Field)  
   - Placeholder ve metin ayarlarını yapın. Character Limit = 20 önerilir.

3. **3 adet örnek isim butonu**  
   - **GameObject → UI → Button - TextMeshPro** (veya Button) x3  
   - Buton metinleri örnek: "Sürat", "Nişancı", "Keskin" (UserNameController'daki `sampleNames` ile aynı sırada olmalı)

4. **Onay butonu**  
   - **GameObject → UI → Button - TextMeshPro** (veya Button)  
   - Metin: "Devam" veya "Oyna"

## 4. UserNameController bağlama

- Hierarchy'de boş bir GameObject oluştur (veya Canvas'ı kullan), adı örn. "UserNameController"
- **Add Component → UserNameController** (UserNameController script'i)
- Inspector'da atamalar:
  - **Next Scene Name**: `UI` (veya ana menü sahnenizin adı)
  - **Name Input TMP**: Oluşturduğunuz TMP_InputField (veya Name Input Legacy'ye legacy InputField)
  - **Sample Button 1, 2, 3**: Sırayla üç örnek isim butonu
  - **Confirm Button**: Onay butonu
  - **Sample Names**: Size 3, Element 0 = "Sürat", Element 1 = "Nişancı", Element 2 = "Keskin" (isterseniz değiştirin)

## 5. VR ile Canvas etkileşimi (ray ile butona tıklama)

Çok basit kurulum:

1. **UserName.unity** sahnesini aç.
2. **Tools → UserName Scene → Setup VR Ray Click (Basit)** menüsünü çalıştır.
3. Sahneyi kaydet (Ctrl+S).
4. VR'da oynat: Kontrolcüden çıkan **ray** ile butona bak, **tetikleyiciye bas** = tıklama.

Bu işlem sahnede **UserNameRayClick** adlı bir obje ekler (tek script, Canvas ve OVRCameraRig otomatik bulunur). Canvas’ta GraphicRaycaster yoksa script ekler. Sahnede **OVRCameraRig** olmalı.

## 6. Build Settings

UserName sahnesi Build Settings'te **ilk sırada** (index 0) olmalı. Bu projede zaten ayarlıdır.

## Davranış

- İlk açılışta kullanıcı isim ekranını görür. Örnek isimlerden birine tıklayınca input alanı o isimle dolar; kullanıcı kendi ismini de yazabilir. Onaylayınca isim kaydedilir ve UI sahnesine geçilir.
- Sonraki açılışlarda (aynı cihazda) UserName sahnesi atlanır, doğrudan UI sahnesi yüklenir.
- Leaderboard'da görünen isim bu sahnede girilen kullanıcı adıdır.
