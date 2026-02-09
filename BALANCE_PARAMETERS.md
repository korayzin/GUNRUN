# GUNRUN – Silah ve Düşman Parametreleri (Final)

> Bu dosya tüm silah ve düşman denge parametrelerinin güncel ve doğru değerlerini içerir.
> Kaynak: Prefab'lar, sahne override'ları ve script varsayılanları.

**GameBalanceManager:** Oyun içi denge tek bir merkezden yönetilir. Sahneye bir `GameBalanceManager` (MonoBehaviour) ekleyin ve isteğe bağlı olarak `GameBalance` (ScriptableObject) asset'ini atayın. Asset yoksa kod içi tablo varsayılanları kullanılır. Asset oluşturmak için menü: **GUNRUN > Create Default Game Balance Asset** (Resources/GameBalance.asset). Her silah ve her düşman tipi (aşamaya göre) kendi parametre setini Manager üzerinden alır; değerler tablodaki gibi ayrı ayrı uygulanır.

---

## Oculus / Meta Quest Tuş Eşlemesi

| Fiziksel Tuş | Sol Kontrolcü | Sağ Kontrolcü | OVRInput |
|--------------|---------------|---------------|----------|
| Tetik | İndeks tetiği | İndeks tetiği | PrimaryIndexTrigger / SecondaryIndexTrigger |
| A tuşu | X | A | Button.One |
| B tuşu | Y | B | Button.Two |
| Ek tuş | — | — | Button.Three (bazı senaryolarda One ile aynı) |

---

# BÖLÜM 1: SİLAHLAR

---

## 1. Silah (FirstGun)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tetik** | Index trigger | Ateş |
| **Mermi hızı (velocity)** | 10 | |
| **Maksimum mermi** | 30 | |
| **Atış arası bekleme (cooldown)** | 0.5 saniye | |
| **Baretta mı?** | Hayır | |
| **Otomatik ateş** | Hayır | |
| **Mermi başı hasar** | 25 | FirstGun_Bullet |
| **Her atışta mermi sayısı** | 1 | |
| **İkincil özellik** | Yok | |

---

## 2. Silah (SecondGun)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tetik** | Index trigger | Ateş |
| **Mermi hızı (velocity)** | 200 | |
| **Maksimum mermi** | 30 | |
| **Atış arası bekleme (cooldown)** | 0.1 saniye | |
| **Baretta mı?** | Hayır | |
| **Otomatik ateş** | Hayır | |
| **Mermi başı hasar** | 30 | SecondGun_Bullet |
| **Her atışta mermi sayısı** | 1 | |
| **İkincil özellik** | Yok | |

---

## 3. Silah (ThirdGun)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tetik** | Index trigger | Ateş |
| **Mermi hızı (velocity)** | 100 | |
| **Maksimum mermi** | 10 | |
| **Atış arası bekleme (cooldown)** | 0.5 saniye | |
| **Baretta mı?** | Hayır | |
| **Otomatik ateş** | Hayır | |
| **Mermi başı hasar** | 25 | |
| **Her atışta mermi sayısı** | 1 | |
| **İkincil özellik** | Yok | |

---

## 4. Silah (FourthGun) – Makineli

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tetik** | Index trigger (basılı tut) | Ateş |
| **Mermi hızı (velocity)** | 80 | |
| **Maksimum mermi** | 100 | |
| **Atış arası bekleme (cooldown)** | 0.05 saniye | |
| **Baretta mı?** | Hayır | |
| **Otomatik ateş** | Evet | |
| **Mermi başı hasar** | 50 | PotionGunBullet |
| **Her atışta mermi sayısı** | 1 | |
| **İkincil özellik** | Yok | |

---

## 5. Silah (FifthGun) – Yıldırım

### Ana atış (mermi)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tetik** | Index trigger | Mermi ateşi |
| **Mermi hızı (velocity)** | 20 | |
| **Maksimum mermi** | 60 | |
| **Atış arası bekleme (cooldown)** | 0.01 saniye | |
| **Baretta mı?** | Hayır | |
| **Otomatik ateş** | Hayır | |
| **Mermi başı hasar** | 50 | |
| **Her atışta mermi sayısı** | 1 | |

### İkincil: Yıldırım (FifthGunLaser)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tuş** | A (Button.One / Button.Three) | Basılı tut = charge, bırak = yıldırım |
| **Yıldırım hasarı** | 9999 | Anlık öldürme |
| **Yıldırım menzili** | 100 metre | |
| **Max yıldırım atışı** | 100 | |
| **Şarj süresi** | 1.5 saniye | Tam şarj için |
| **Minimum şarj oranı** | %30 | Bu seviyeye gelmeden ateşlenmez |

---

## 6. Silah (SixthGun) – Yavaşlatma Laseri

### Ana atış (mermi)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tetik** | Index trigger | Mermi ateşi |
| **Mermi hızı (velocity)** | 10–20 | Sahne ayarına göre |
| **Maksimum mermi** | 20–30 | Sahne ayarına göre |
| **Mermi başı hasar** | 25 | OctopusAmmo |
| **Her atışta mermi sayısı** | 1 | |

### İkincil: Yavaşlatma (SixthGunLaser)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tuş** | B (Button.Two / Button.Three) | Toggle (aç/kapat) |
| **Yavaşlatma oranı** | %60 | Düşman hızı × 0.4 = %40 hız kalır |
| **Laser menzili** | 100 metre | |
| **Slow tick rate** | 10/saniye | Her 0.1 sn raycast kontrolü |
| **Max enerji** | 100 | Koray/Gamze sahnesi |
| **Enerji tüketimi** | 1/sn (boşta), 3/sn (düşmana değerken) | |
| **Enerji dolum hızı** | 2/saniye | Laser kapalıyken |
| **Buz efekti** | Mavi glow, SLOWED yazısı | Düşman üzerinde |

---

## 7. Silah (SeventhGun) – Kırbaç

### Ana atış

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Mermi atıyor mu?** | Hayır | bulletPrefab boş |
| **Maksimum mermi** | 25 | UI gösterimi için |

### İkincil: Kırbaç (SeventhGunWhip)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tuş** | A (Button.One / Button.Three) | Tek basış |
| **Kırbaç hasarı** | 5 | Her vuruşta |
| **Savurma gücü (knockback)** | 100 | |
| **Kırbaç uzunluğu** | 12 metre | |
| **Animasyon süresi** | 0.3 saniye | |
| **Cooldown** | 0.1 saniye | Atışlar arası |
| **Max kırbaç atışı** | 5 | Kullanım limiti |

---

## 8. Silah (EightGun) – Toy Yardımcısı

### Ana atış (çift mermi)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tetik** | Index trigger | Ateş |
| **Mermi hızı (velocity)** | 20 | |
| **Maksimum mermi** | 20 | |
| **Atış arası bekleme (cooldown)** | 0.5 saniye | |
| **Baretta mı?** | Hayır | |
| **Otomatik ateş** | Hayır | |
| **Mermi başı hasar** | 50 | EightAmmo |
| **Her atışta mermi sayısı** | 2 (üst + alt) | dualShotSpacing: 0.14 |
| **İkincil özellik** | Toy | |

### İkincil: Toy (ToyHelper)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tuş** | A (Button.One) | Tek basış |
| **Aktif süre** | 15 saniye | Yanında kalma |
| **Cooldown** | 12 saniye | Tekrar kullanım bekleme |
| **Tick hasarı** | 8 | Her lazer tick |
| **Tick aralığı** | 0.1 saniye | |
| **Menzil** | 25 metre | Düşman hedefleme |

---

## 9. Silah (LastGun) – Alev Püskürtücü

### Ana: Alev püskürtme (LastGunFlameSpray)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tuş** | Index trigger (basılı tut) | Tetik |
| **Saniyedeki hasar (DPS)** | 12 | |
| **Püskürtme menzili** | 8 metre | |
| **Hasar kontrol aralığı** | 0.08 saniye | |
| **Max püskürtme enerjisi** | 100 | |
| **Enerji tüketimi** | 25/saniye | Tetik basılıyken |
| **Enerji yenileme** | 15/saniye | Tetik bırakıldığında |
| **Ne kadar süre kullanılabilir?** | ~4 saniye | 100 ÷ 25 |
| **Tam dolum süresi** | ~6.7 saniye | 100 ÷ 15 |

### Alternatif: Fireball (tek atış)

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Tuş** | A (Button.One) | Tek basış |
| **Fireball hasarı** | 100 | |
| **Mermi hızı** | 80 | |
| **Maksimum mermi** | 8 | |

---

## Silah Tuş Özeti

| Silah | Tetik | A tuşu | B tuşu |
|-------|-------|--------|--------|
| 1–4 | Mermi ateş | — | — |
| 5 | Mermi ateş | Yıldırım charge | — |
| 6 | Mermi ateş | — | Yavaşlatma laser (toggle) |
| 7 | — | Kırbaç | — |
| 8 | Mermi ateş | Toy gönder | — |
| 9 | Alev püskürt | Fireball tek atış | — |

---

# BÖLÜM 2: DÜŞMANLAR

---

## Stage Geçiş Eşikleri (kill sayısına göre)

| Parametre | Varsayılan (script) | Koray/Gamze sahne | Açıklama |
|-----------|---------------------|-------------------|----------|
| **Stage 2 başlangıcı** | 24 kill | 36 kill | Bu kill'dan sonra Stage 2 |
| **Stage 3 başlangıcı** | 54 kill | 90 kill | Bu kill'dan sonra Stage 3 |

---

## Stage Bazlı Spawn Aralıkları

| Stage | Spawn aralığı | Açıklama |
|-------|---------------|----------|
| **Stage 1** | 2.5 saniye | Her 2.5 sn'de 1 düşman |
| **Stage 2** | 2.0 saniye | Her 2 sn'de 1 düşman |
| **Stage 3** | 1.5 saniye | Her 1.5 sn'de 1 düşman |

---

## Stage Bazlı Düşman Hızı

| Stage | Düşman hızı | Açıklama |
|-------|-------------|----------|
| **Stage 1** | 3.0 | |
| **Stage 2** | 3.5 | |
| **Stage 3** | 4.0 | |

---

## Düşman Tipleri – Can (HP)

| Düşman tipi | Varsayılan HP | Koray/Gamze HP | Açıklama |
|-------------|---------------|----------------|----------|
| **Tur1** (Kolay) | 40 | 50 | |
| **Tur2** (Orta) | 80 | 100 | |
| **Tur3** (Zor) | 150 | 200 | |
| **Tur4** (Elit) | 200 | 200 | |

---

## Düşman Tipleri – Verilen Skor

| Düşman tipi | Skor |
|-------------|------|
| **Tur1** | 25 |
| **Tur2** | 50 |
| **Tur3** | 100 |
| **Tur4** | 150 |

---

## Düşman Spawn Yüzdeleri

### Stage 1

| Düşman | Spawn oranı | Pool |
|--------|-------------|------|
| **Tur1** | %80 | 8/10 |
| **Tur2** | %20 | 2/10 |
| **Tur3** | %0 | Yok |
| **Tur4** | %0 | Yok |

### Stage 2

| Düşman | Spawn oranı | Pool |
|--------|-------------|------|
| **Tur1** | ~%45 | 5/11 |
| **Tur2** | ~%36 | 4/11 |
| **Tur3** | ~%9 | 1/11 |
| **Tur4** | ~%9 | 1/11 (tur4Enemy atanmışsa) |

### Stage 3

| Düşman | Spawn oranı | Pool |
|--------|-------------|------|
| **Tur1** | ~%25 | 3/12 |
| **Tur2** | ~%42 | 5/12 |
| **Tur3** | ~%17 | 2/12 |
| **Tur4** | ~%17 | 2/12 (tur4Enemy atanmışsa) |

---

## Hasar Çarpanları (vurulan bölgeye göre)

| Bölge | Çarpan | Açıklama |
|-------|--------|----------|
| **Kafa (headshot)** | 2x | |
| **Gövde (body)** | 1x | |
| **Bacak (legs)** | 0.7x | |

*Tek collider sistemli düşmanlarda multiplier uygulanmaz.*

---

## NavMeshAgent Parametreleri

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **Radius** | 0.04 | |
| **Height** | 0 | |
| **Acceleration** | 8 | |
| **Angular Speed** | 120 | |
| **Stopping Distance** | 2 | |
| **Auto Braking** | Evet | |
| **Auto Repath** | Evet | |

---

## Portal Spawn Ayarları

| Parametre | Değer | Açıklama |
|-----------|-------|----------|
| **İlk spawn gecikmesi** | 2 saniye | Portallar açıldıktan sonra |
| **Spawn yüksekliği offset** | 1.5 | |
| **Portal açılma gecikmesi** | 7.5 saniye | Oyun başladıktan sonra |
| **Portal animasyon süresi** | 1.5 saniye | |
| **Portallar arası gecikme** | 0.3 saniye | A → B → C |
| **Ardışık portal limiti** | 2 | Aynı portaldan üst üste en fazla 2 düşman |

---

## Stage Özet Tablosu

| Parametre | Stage 1 | Stage 2 | Stage 3 |
|-----------|---------|---------|---------|
| **Stage geçiş (kill)** | 0–24 (veya 0–36) | 24–54 (veya 36–90) | 54+ (veya 90+) |
| **Spawn aralığı** | 2.5 sn | 2.0 sn | 1.5 sn |
| **Düşman hızı** | 3.0 | 3.5 | 4.0 |
| **Tur1 oranı** | %80 | ~%45 | ~%25 |
| **Tur2 oranı** | %20 | ~%36 | ~%42 |
| **Tur3 oranı** | %0 | ~%9 | ~%17 |
| **Tur4 oranı** | %0 | ~%9 | ~%17 |

---

*Son güncelleme: Prefab ve sahne verilerine göre derlenmiştir.*
