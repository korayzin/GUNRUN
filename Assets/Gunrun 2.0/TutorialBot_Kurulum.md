# TutorialBot Prefab Kurulumu ve Sistem Akışı

## 1. Prefab Hiyerarşisi (İçerik Nasıl Olsun?)

```
TutorialBot                    ← Ana obje
├── TutorialBot (Script)       ← Bu script BURAYA ekle (root'a)
├── Canvas                     ← UI için
│   └── Panel                  ← Adında "Panel" geçmeli (örn. DialoguePanel)
│       └── DialogueText      ← Bu objeye TextMeshPro - UGUI ekle
└── (opsiyonel) CanvasGroup    ← Fade için; root veya Canvas'a eklenebilir
```

---

## 2. Hangi Objeye Hangi Component?

| Obje | Eklenmesi Gereken Component | Açıklama |
|------|-----------------------------|----------|
| **TutorialBot (root)** | **TutorialBot** (script) | Zorunlu. Botu kontrol eder. |
| **TutorialBot (root)** | **CanvasGroup** (opsiyonel) | Dissolve yoksa bot açılış/kapanışta fade ile görünür kaybolur. |
| **Canvas** | **Canvas** | Zorunlu. UI için (Unity varsayılan Canvas component). |
| **Panel** | **Rect Transform** (Image veya sadece RectTransform) | Panel = Canvas'ın child'ı; adı "Panel" içermeli. |
| **DialogueText** | **TextMeshPro - UGUI** (TextMeshProUGUI) | Zorunlu. Diyalog metni burada görünür. |

**Not:** Root'ta veya herhangi bir child'ta `WeaponDissolveEffect` varsa bot dissolve ile gelir/gider; yoksa `CanvasGroup` ile fade kullanılır.

---

## 3. Sistem Nasıl Çalışıyor? (Akış)

```
[SAHNE: newtutorial]
       │
       ▼
TutorialSequenceController (sahnedeki bir GameObject'te)
       │
       ├─► "Tutorial Bot Prefab" alanına TutorialBot prefab'ını sürükle
       ├─► "Bot Spawn Point" alanına spawn konumu için boş bir Transform sürükle
       │
       ▼
Oyun başlayınca:
  1) Controller prefab'ı spawn eder → TutorialBot (clone) sahneye gelir
  2) Controller, botun spawnPoint'ini = senin verdiğin Bot Spawn Point yapar
  3) Bot Awake'te kendi içinde Canvas → Panel → Text (TMP) arar, bulduğu TMP'ye yazacak
  4) Her diyalogda:
     - Controller metni verir (örn. "Duvarları kırabilirsin...")
     - Bot spawn noktasına ışınlanır (PlaceAndShow)
     - Bot görünür olur (dissolve veya fade)
     - Metin botun Panel'indeki TMP'ye yazılır (typing veya anında)
     - Bekleme
     - Bot tekrar kaybolur (HideWithDissolve)
```

**Özet:** Controller ne zaman ne söyleyeceğini ve nerede göstereceğini bilir; bot sadece "şu konumda görün, şu metni paneldeki texte yaz, sonra kaybol" der.

---

## 4. Inspector'da Ne Bırakmalı? (TutorialBot Script)

- **Dialogue Text:** Boş bırak → Kod Canvas > Panel altındaki TMP'yi kendisi bulur.
- **Dialogue Panel:** Boş bırak → Kod "Panel" adlı child'ı kendisi bulur. İstersen Panel objesini buraya sürükleyebilirsin.
- **Spawn Point:** Prefab'ta boş bırak → Sahnedeki Controller, kendi "Bot Spawn Point" değerini botun spawnPoint'ine atar.
- **Look At Target:** İsteğe bağlı; spawn noktası kullanılıyorsa gerekmez.

---

## 5. Controller Tarafında (TutorialSequenceController)

- **Tutorial Bot Prefab:** TutorialBot prefab'ı (Project'tan sürükle).
- **Bot Spawn Point:** Sahnedeki boş bir GameObject (veya konumu/rotasyonu ayarladığın bir Transform). Bot her diyalogda bu konumda belirir.

Bu yapıyla bot senin tanımladığın konumda spawn olur ve diyalog metni her zaman botun Canvas'ındaki Panel'in üstündeki texte yazılır.
