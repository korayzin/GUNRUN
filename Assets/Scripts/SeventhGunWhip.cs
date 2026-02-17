using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SeventhGun için kırbaç/tentakül sistemi.
/// Ayrı bir tuş ile tetiklenir, düşmanlara hasar verir ve savurur.
/// </summary>
public class SeventhGunWhip : MonoBehaviour
{
    [Header("=== KIRBAÇ AYARLARI ===")]
    [Tooltip("Kırbaç çıkış noktası (Transform object)")]
    public Transform whipSpawnPoint;
    
    [Tooltip("Kırbaç uzunluğu (metre)")]
    public float whipLength = 8f;
    
    [Tooltip("Kırbaç kalınlığı (başlangıç)")]
    public float whipStartWidth = 0.15f;
    
    [Tooltip("Kırbaç kalınlığı (uç)")]
    public float whipEndWidth = 0.05f;
    
    [Tooltip("Segment sayısı (smooth görünüm için)")]
    [Range(10, 50)]
    public int segments = 25;
    
    [Header("=== SAVAŞ AYARLARI ===")]
    [Tooltip("Hasar miktarı")]
    public float damage = 5f;
    
    [Tooltip("Savurma gücü")]
    public float knockbackForce = 5f;
    
    [Tooltip("Collision detection radius")]
    public float collisionRadius = 0.3f;
    
    [Tooltip("Layer mask (hangi layer'lardaki objeler tespit edilsin)")]
    public LayerMask enemyLayerMask = -1;
    
    [Header("=== ANİMASYON ===")]
    [Tooltip("Kırbaç animasyon süresi (saniye)")]
    public float whipDuration = 0.3f; // Daha hızlı (0.5'ten 0.3'e)
    
    [Tooltip("Cooldown süresi (saniye)")]
    public float cooldown = 1f;
    
    [Tooltip("Whip motion sapma miktarı (ne kadar kıvrımlı)")]
    public float whipSwayAmount = 1.2f; // Daha kıvrak (0.5'ten 1.2'ye)
    
    [Tooltip("Whip motion hızı")]
    public float whipSwaySpeed = 15f; // Daha hızlı hareket (8'den 15'e)
    
    [Tooltip("Uzama hızı çarpanı (ne kadar hızlı uzasın)")]
    public float extensionSpeedMultiplier = 2f; // Daha hızlı uzama
    
    [Header("=== GÖRSEL ===")]
    [Tooltip("Kırbaç rengi (ahtapot tentakülü)")]
    public Color whipColor = new Color(0.5f, 0.3f, 0.8f, 1f); // Mor-mavi
    
    [Tooltip("Glow rengi")]
    public Color glowColor = new Color(0.6f, 0.4f, 0.9f, 0.4f);
    
    [Tooltip("Core rengi (parlak çekirdek)")]
    public Color coreColor = new Color(1f, 1f, 1f, 1f);
    
    [Tooltip("Glow kalınlığı")]
    public float glowWidth = 0.25f;
    
    [Tooltip("Core kalınlığı")]
    public float coreWidth = 0.05f;
    
    [Header("=== HAPTİK ===")]
    [Tooltip("Haptic feedback şiddeti")]
    public float hapticStrength = 0.8f;
    
    [Header("=== KIRBAÇ UI ===")]
    [Tooltip("Maksimum kırbaç atış sayısı")]
    public int maxWhipShots = 5;
    
    [Tooltip("UI pozisyonu (Transform - kullanıcı tarafından atanacak)")]
    public Transform uiPosition;
    
    [Tooltip("UI boyutu")]
    public float uiSize = 0.08f;
    
    [Tooltip("UI offset (X=sağ, Y=yukarı, Z=ileri) - uiPosition'a göre")]
    public Vector3 uiOffset = new Vector3(0f, 0f, 0f);
    
    [Tooltip("Ana renk (kırbaç teması)")]
    public Color uiColor = new Color(0.5f, 0.3f, 0.8f, 1f); // Mor-mavi
    
    [Tooltip("Arka plan rengi")]
    public Color uiBgColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
    
    [Tooltip("Text'in normal rengi")]
    public Color textNormalColor = Color.white;
    
    [Tooltip("Text'in kırmızı rengi (0/5 olduğunda)")]
    public Color textEmptyColor = Color.red;
    
    [Tooltip("Batarya genişliği")]
    public float batteryWidth = 3f;
    
    [Tooltip("Batarya yüksekliği")]
    public float batteryHeight = 1f;
    
    [Tooltip("Segment arası boşluk")]
    public float segmentGap = 0.1f;
    
    [Tooltip("Batarya outline kalınlığı")]
    public float outlineThickness = 0.15f;
    
    // Private değişkenler
    private LineRenderer whipMain;
    private LineRenderer whipGlow;
    private LineRenderer whipCore;
    private GameObject whipContainer;
    private bool isWhipActive = false;
    private bool canUseWhip = true;
    private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();
    private Coroutine whipCoroutine;
    private GunFire gunFire;
    private int remainingWhipShots;
    
    // UI değişkenleri
    private GameObject whipUIContainer;
    private Canvas whipCanvas;
    private Image batteryBackground;
    private Image batteryOutline;
    private List<Image> batterySegments = new List<Image>(); // 5 segment
    private TextMeshPro countText;
    
    void Start()
    {
        gunFire = GetComponent<GunFire>();
        
        // Whip spawn point'i bul
        if (whipSpawnPoint == null)
        {
            // Prefab içinde "WhipSpawnPoint" veya benzer bir isimle ara
            Transform whipChild = transform.Find("WhipSpawnPoint");
            if (whipChild == null)
            {
                // Tüm child'ları kontrol et
                foreach (Transform child in transform)
                {
                    if (child.name.Contains("Whip") || child.name.Contains("Spawn"))
                    {
                        whipSpawnPoint = child;
                        break;
                    }
                }
            }
            else
            {
                whipSpawnPoint = whipChild;
            }
        }
        
        // Whip container oluştur
        whipContainer = new GameObject("WhipContainer");
        whipContainer.transform.SetParent(transform);
        whipContainer.transform.localPosition = Vector3.zero;
        whipContainer.transform.localRotation = Quaternion.identity;
        
        // Whip line renderer'ları oluştur
        CreateWhipRenderers();
        
        // Kırbaç atış sayısını başlat
        remainingWhipShots = maxWhipShots;
        
        // UI oluştur
        CreateWhipUI();
    }
    
    void Update()
    {
        bool tutorialUnlimited = TutorialIntroController.TutorialSeventhWeaponPhase && !TutorialIntroController.TutorialSeventhWeaponGunChangeAfterEnabled;
        if (!tutorialUnlimited && maxWhipShots > 0 && remainingWhipShots <= 0) return;
        
        if (!TutorialIntroController.TutorialCompleteFreehand && TutorialIntroController.TutorialSeventhWeaponPhase && !TutorialIntroController.TutorialSeventhWeaponSecondaryEnabled)
            return; // 17. diyalog bitmeden ikincil (kırbaç) kapalı
        
        // Ayrı tuş kontrolü (OVRInput.Button.One veya Button.Three)
        if ((OVRInput.GetDown(OVRInput.Button.One) || OVRInput.GetDown(OVRInput.Button.Three)) && canUseWhip && !isWhipActive)
        {
            ActivateWhip();
        }
        
        // UI güncelle
        UpdateWhipUI();
    }
    
    void CreateWhipRenderers()
    {
        // 1. GLOW LAYER (Dış glow - en geniş)
        GameObject glowObj = new GameObject("WhipGlow");
        glowObj.transform.SetParent(whipContainer.transform);
        whipGlow = glowObj.AddComponent<LineRenderer>();
        SetupWhipRenderer(whipGlow, glowWidth, glowColor, 0);
        
        // 2. MAIN LAYER (Ana kırbaç)
        GameObject mainObj = new GameObject("WhipMain");
        mainObj.transform.SetParent(whipContainer.transform);
        whipMain = mainObj.AddComponent<LineRenderer>();
        SetupWhipRenderer(whipMain, whipStartWidth, whipColor, 1);
        
        // 3. CORE LAYER (Parlak çekirdek - en dar)
        GameObject coreObj = new GameObject("WhipCore");
        coreObj.transform.SetParent(whipContainer.transform);
        whipCore = coreObj.AddComponent<LineRenderer>();
        SetupWhipRenderer(whipCore, coreWidth, coreColor, 2);

        UICameraStackSetup.SetLayerRecursivelyToUI(whipContainer);
        
        // Başlangıçta kapalı
        SetWhipVisible(false);
    }
    
    void SetupWhipRenderer(LineRenderer line, float width, Color color, int sortingOrder)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        line.material = mat;
        
        // Width curve (başlangıçta kalın, uçta ince)
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 1f);
        widthCurve.AddKey(0.3f, 0.9f);
        widthCurve.AddKey(0.7f, 0.5f);
        widthCurve.AddKey(1f, 0.2f);
        line.widthCurve = widthCurve;
        line.widthMultiplier = width;
        
        line.useWorldSpace = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.sortingOrder = sortingOrder;
        line.positionCount = segments + 1;
        
        // Smooth görünüm
        line.numCapVertices = 5;
        line.numCornerVertices = 5;
        
        // Gradient (başlangıçta parlak, uçta soluk)
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(color, 0f),
                new GradientColorKey(Color.Lerp(color, Color.clear, 0.3f), 0.7f),
                new GradientColorKey(Color.Lerp(color, Color.clear, 0.6f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(color.a, 0f),
                new GradientAlphaKey(color.a * 0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        line.colorGradient = gradient;
        
        line.enabled = false;
    }
    
    void ActivateWhip()
    {
        bool tutorialUnlimited = TutorialIntroController.TutorialSeventhWeaponPhase && !TutorialIntroController.TutorialSeventhWeaponGunChangeAfterEnabled;
        if (!tutorialUnlimited && maxWhipShots > 0 && remainingWhipShots <= 0)
        {
            return; // Kırbaç bitti, aktifleştirme
        }
        
        if (whipSpawnPoint == null)
        {
            Debug.LogWarning("SeventhGunWhip: whipSpawnPoint atanmamış!");
            return;
        }
        
        if (tutorialUnlimited)
            TutorialIntroController.NotifyTutorialSeventhWeaponWhipUsed();
        
        isWhipActive = true;
        canUseWhip = false;
        hitEnemies.Clear();
        
        // Atış hakkını düşür (tutorial sınırsız modda düşürme)
        if (!tutorialUnlimited && maxWhipShots > 0)
        {
            remainingWhipShots--;
            Debug.Log($"⚡ KIRBAÇ! Kalan: {remainingWhipShots}/{maxWhipShots}");
        }
        
        // Haptic feedback
        StartCoroutine(HapticFeedback());
        
        // Whip animasyonunu başlat
        if (whipCoroutine != null)
        {
            StopCoroutine(whipCoroutine);
        }
        whipCoroutine = StartCoroutine(WhipAnimationRoutine());
    }
    
    IEnumerator WhipAnimationRoutine()
    {
        SetWhipVisible(true);
        
        float elapsed = 0f;
        float animationTime = whipDuration;
        
        Vector3 startPos = whipSpawnPoint.position;
        Vector3 forward = whipSpawnPoint.forward;
        
        while (elapsed < animationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationTime;
            
            // Whip motion: bezier curve + sinüs dalgası kombinasyonu
            Vector3[] whipPoints = GenerateWhipPoints(startPos, forward, t);
            
            // Line renderer pozisyonlarını güncelle
            UpdateWhipPositions(whipPoints);
            
            // Collision detection ve hasar/knockback
            CheckCollisions(whipPoints, t);
            
            yield return null;
        }
        
        // Animasyon bitti, kırbaç görünmez yap
        SetWhipVisible(false);
        isWhipActive = false;
        
        // Cooldown bekle
        yield return new WaitForSeconds(cooldown);
        canUseWhip = true;
    }
    
    Vector3[] GenerateWhipPoints(Vector3 startPos, Vector3 forward, float t)
    {
        Vector3[] points = new Vector3[segments + 1];
        points[0] = startPos;
        
        // Whip uzunluğu (animasyon sırasında uzar) - daha hızlı uzama için ease out cubic
        float easedT = 1f - Mathf.Pow(1f - t, extensionSpeedMultiplier);
        float currentLength = whipLength * easedT;
        
        // Whip motion için sinüs dalgası ve bezier curve kombinasyonu
        float swayPhase = Time.time * whipSwaySpeed;
        
        for (int i = 1; i <= segments; i++)
        {
            float segmentT = (float)i / segments;
            
            // Temel pozisyon (düz çizgi)
            Vector3 basePos = startPos + forward * currentLength * segmentT;
            
            // Whip motion: sinüs dalgası ile yan yana sapma - DAHA KIVRAK
            Vector3 perpendicular = Vector3.Cross(forward, Vector3.up).normalized;
            if (perpendicular.magnitude < 0.1f)
            {
                perpendicular = Vector3.Cross(forward, Vector3.right).normalized;
            }
            
            // Çoklu sinüs dalgası kombinasyonu (daha kıvrak hareket)
            float sway1 = Mathf.Sin(swayPhase + segmentT * Mathf.PI * 3f) * whipSwayAmount * segmentT;
            float sway2 = Mathf.Sin(swayPhase * 1.5f + segmentT * Mathf.PI * 5f) * whipSwayAmount * 0.6f * segmentT;
            float swayAmount = (sway1 + sway2) * 0.7f; // Kombine sapma
            Vector3 swayOffset = perpendicular * swayAmount;
            
            // Bezier curve için kontrol noktası (ortada bir eğri) - DAHA BELİRGİN
            float curveAmount = Mathf.Sin(segmentT * Mathf.PI) * whipSwayAmount * 0.8f;
            Vector3 curveOffset = Vector3.up * curveAmount * segmentT;
            
            // Ekstra rastgele sapma (daha organik görünüm)
            float randomOffset = Mathf.PerlinNoise(Time.time * 2f + segmentT * 10f, 0f) - 0.5f;
            Vector3 randomSway = perpendicular * randomOffset * whipSwayAmount * 0.3f * segmentT;
            
            // Final pozisyon
            points[i] = basePos + swayOffset + curveOffset + randomSway;
        }
        
        return points;
    }
    
    void UpdateWhipPositions(Vector3[] points)
    {
        if (whipGlow != null && whipGlow.enabled)
        {
            whipGlow.positionCount = points.Length;
            whipGlow.SetPositions(points);
        }
        
        if (whipMain != null && whipMain.enabled)
        {
            whipMain.positionCount = points.Length;
            whipMain.SetPositions(points);
        }
        
        if (whipCore != null && whipCore.enabled)
        {
            whipCore.positionCount = points.Length;
            whipCore.SetPositions(points);
        }
    }
    
    void CheckCollisions(Vector3[] whipPoints, float t)
    {
        // CapsuleCast benzeri collision detection
        // Her segment arasında sphere cast yap
        
        for (int i = 0; i < whipPoints.Length - 1; i++)
        {
            Vector3 segmentStart = whipPoints[i];
            Vector3 segmentEnd = whipPoints[i + 1];
            Vector3 segmentDir = (segmentEnd - segmentStart).normalized;
            float segmentLength = Vector3.Distance(segmentStart, segmentEnd);
            
            // SphereCast ile collision detection
            RaycastHit[] hits = Physics.SphereCastAll(
                segmentStart,
                collisionRadius,
                segmentDir,
                segmentLength,
                enemyLayerMask
            );
            
            foreach (RaycastHit hit in hits)
            {
                EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemyHealth != null && !hitEnemies.Contains(enemyHealth))
                {
                    // İlk kez vuruldu, hasar ver ve savur
                    hitEnemies.Add(enemyHealth);
                    ApplyDamageAndKnockback(enemyHealth, hit, segmentEnd);
                }
            }
        }
    }
    
    void ApplyDamageAndKnockback(EnemyHealth enemyHealth, RaycastHit hit, Vector3 whipTipPos)
    {
        // Hasar ver
        enemyHealth.TakeDamage(damage, hit.collider, fromFlameSpray: false, fromSecondary: true, tutorialWeaponIndex: 6);
        
        // Knockback uygula - düşmanın merkezinden whip tipine doğru
        Vector3 enemyPos = hit.collider.bounds.center;
        Vector3 knockbackDirection = (enemyPos - whipTipPos).normalized;
        knockbackDirection.y = 0f; // Y ekseninde çok fazla yukarı kaldırma
        knockbackDirection.Normalize();
        
        // NavMeshAgent varsa transform.position ile knockback
        NavMeshAgent agent = enemyHealth.agent;
        if (agent != null && agent.enabled)
        {
            // Agent'ı geçici olarak durdur
            bool wasStopped = agent.isStopped;
            agent.isStopped = true;
            
            // Transform.position ile direkt hareket ettir (daha güçlü knockback)
            Vector3 knockbackVector = knockbackDirection * knockbackForce;
            agent.transform.position += knockbackVector;
            
            // Agent'ı tekrar aktif et ve velocity uygula (devam eden momentum için)
            agent.isStopped = wasStopped;
            agent.velocity = knockbackDirection * knockbackForce * 0.5f; // Ekstra momentum
            
            // Bir süre sonra velocity'yi sıfırla (düşman tekrar NavMesh'e dönsün)
            StartCoroutine(ResetAgentVelocity(agent, 0.5f));
        }
        else
        {
            // Rigidbody varsa AddForce kullan
            Rigidbody rb = enemyHealth.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(knockbackDirection * knockbackForce * 10f, ForceMode.Impulse);
            }
            else
            {
                // Rigidbody yoksa direkt transform ile hareket ettir
                Transform enemyTransform = enemyHealth.transform;
                enemyTransform.position += knockbackDirection * knockbackForce * 0.1f;
            }
        }
        
        Debug.Log($"Kırbaç düşmana vurdu! Hasar: {damage}, Savurma: {knockbackForce}, Yön: {knockbackDirection}");
    }
    
    IEnumerator ResetAgentVelocity(NavMeshAgent agent, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (agent != null && agent.enabled)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = false; // Agent'ı tekrar aktif et
        }
    }
    
    void SetWhipVisible(bool visible)
    {
        if (whipGlow != null) whipGlow.enabled = visible;
        if (whipMain != null) whipMain.enabled = visible;
        if (whipCore != null) whipCore.enabled = visible;
    }
    
    IEnumerator HapticFeedback()
    {
        OVRInput.Controller controller = (gunFire != null && gunFire.isLeftHanded) 
            ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        
        // Güçlü haptic (kırbaç vuruşu)
        OVRInput.SetControllerVibration(1f, hapticStrength, controller);
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0.5f, hapticStrength * 0.7f, controller);
        yield return new WaitForSeconds(0.1f);
        OVRInput.SetControllerVibration(0f, 0f, controller);
    }
    
    void CreateWhipUI()
    {
        if (uiPosition == null)
        {
            Debug.LogWarning("SeventhGunWhip: uiPosition atanmamış! UI oluşturulamıyor.");
            return;
        }
        
        // Ana container - uiPosition'a parent'la
        whipUIContainer = new GameObject("WhipUI");
        whipUIContainer.transform.SetParent(uiPosition);
        whipUIContainer.transform.localPosition = uiOffset;
        whipUIContainer.transform.localRotation = Quaternion.identity;
        
        // World Space Canvas
        whipCanvas = whipUIContainer.AddComponent<Canvas>();
        whipCanvas.renderMode = RenderMode.WorldSpace;
        UICameraStackSetup.Instance?.RegisterWorldSpaceCanvas(whipCanvas);
        
        RectTransform canvasRect = whipCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(batteryWidth + 1f, batteryHeight + 0.5f);
        canvasRect.localScale = Vector3.one * uiSize * 0.01f;
        
        // 1. Batarya arka planı (koyu)
        GameObject bgObj = new GameObject("BatteryBackground");
        bgObj.transform.SetParent(whipUIContainer.transform, false);
        batteryBackground = bgObj.AddComponent<Image>();
        batteryBackground.color = uiBgColor;
        batteryBackground.raycastTarget = false;
        RectTransform bgRect = batteryBackground.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(batteryWidth, batteryHeight);
        bgRect.anchoredPosition = Vector2.zero;
        
        // 2. Batarya outline (dış çerçeve)
        GameObject outlineObj = new GameObject("BatteryOutline");
        outlineObj.transform.SetParent(whipUIContainer.transform, false);
        batteryOutline = outlineObj.AddComponent<Image>();
        batteryOutline.color = new Color(uiColor.r, uiColor.g, uiColor.b, 0.8f);
        batteryOutline.raycastTarget = false;
        RectTransform outlineRect = batteryOutline.GetComponent<RectTransform>();
        outlineRect.sizeDelta = new Vector2(batteryWidth + outlineThickness * 2f, batteryHeight + outlineThickness * 2f);
        outlineRect.anchoredPosition = Vector2.zero;
        outlineRect.SetAsFirstSibling();
        
        // Batarya başlığı (üst kısım - pil başlığı gibi)
        GameObject tipObj = new GameObject("BatteryTip");
        tipObj.transform.SetParent(whipUIContainer.transform, false);
        Image batteryTip = tipObj.AddComponent<Image>();
        batteryTip.color = new Color(uiColor.r, uiColor.g, uiColor.b, 0.8f);
        batteryTip.raycastTarget = false;
        RectTransform tipRect = batteryTip.GetComponent<RectTransform>();
        tipRect.sizeDelta = new Vector2(batteryWidth * 0.3f, batteryHeight * 0.4f);
        tipRect.anchoredPosition = new Vector2(0f, batteryHeight * 0.5f + batteryHeight * 0.2f);
        
        // 3. 5 SEGMENT OLUŞTUR (yan yana)
        float segmentWidth = (batteryWidth - segmentGap * 4f) / 5f; // 5 segment, 4 boşluk
        
        for (int i = 0; i < 5; i++)
        {
            GameObject segmentObj = new GameObject($"Segment{i}");
            segmentObj.transform.SetParent(whipUIContainer.transform, false);
            Image segmentImage = segmentObj.AddComponent<Image>();
            segmentImage.color = GetBatteryColor(5 - i); // Başlangıçta yeşil
            segmentImage.raycastTarget = false;
            
            RectTransform segmentRect = segmentImage.GetComponent<RectTransform>();
            segmentRect.sizeDelta = new Vector2(segmentWidth, batteryHeight * 0.9f);
            
            // Pozisyon: soldan sağa, ortalanmış
            float startX = -batteryWidth * 0.5f + segmentWidth * 0.5f;
            float xPos = startX + i * (segmentWidth + segmentGap);
            segmentRect.anchoredPosition = new Vector2(xPos, 0f);
            
            batterySegments.Add(segmentImage);
        }
        
        // 4. Text (kalan atış sayısı: 5/5) - Bataryanın altında
        GameObject textObj = new GameObject("CountText");
        textObj.transform.SetParent(whipUIContainer.transform, false);
        countText = textObj.AddComponent<TextMeshPro>();
        countText.text = $"{maxWhipShots}/{maxWhipShots}";
        countText.fontSize = 1.5f;
        countText.color = textNormalColor;
        countText.alignment = TextAlignmentOptions.Center;
        countText.fontStyle = FontStyles.Bold;
        countText.sortingOrder = 10;
        
        RectTransform textRect = countText.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(batteryWidth, 0.5f);
        textRect.anchoredPosition = new Vector2(0f, -batteryHeight * 0.5f - 0.3f);
    }
    
    Color GetBatteryColor(int remainingShots)
    {
        // Renk geçişi: Yeşil -> Sarı -> Turuncu -> Kırmızı
        if (remainingShots >= 4)
            return new Color(0.2f, 1f, 0.2f, 1f); // Yeşil
        else if (remainingShots >= 3)
            return new Color(1f, 1f, 0.2f, 1f); // Sarı
        else if (remainingShots >= 2)
            return new Color(1f, 0.6f, 0.2f, 1f); // Turuncu
        else if (remainingShots >= 1)
            return new Color(1f, 0.3f, 0.2f, 1f); // Kırmızı-turuncu
        else
            return new Color(0.3f, 0.3f, 0.3f, 1f); // Gri (boş)
    }
    
    
    void UpdateWhipUI()
    {
        if (whipUIContainer == null || batterySegments.Count == 0 || countText == null) return;
        
        bool tutorialUnlimited = TutorialIntroController.TutorialSeventhWeaponPhase && !TutorialIntroController.TutorialSeventhWeaponGunChangeAfterEnabled;
        whipUIContainer.SetActive(!tutorialUnlimited);
        if (tutorialUnlimited) return;
        
        // Billboard - her zaman kameraya baksın
        if (Camera.main != null)
        {
            Vector3 lookDir = whipUIContainer.transform.position - Camera.main.transform.position;
            if (lookDir != Vector3.zero)
            {
                whipUIContainer.transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
        
        // Kalan atış sayısını hesapla
        float shotsPercent = maxWhipShots > 0 ? (float)remainingWhipShots / maxWhipShots : 1f;
        
        // 5 segment'i güncelle - HER ATIŞTA NET BİR SEGMENT KAYBOLUR
        // 5 atış -> 5 segment görünür
        // 4 atış -> 4 segment görünür
        // 3 atış -> 3 segment görünür
        // 2 atış -> 2 segment görünür
        // 1 atış -> 1 segment görünür
        // 0 atış -> 0 segment görünür
        
        for (int i = 0; i < batterySegments.Count; i++)
        {
            // Segment index'i ters (sağdan sola kaybolur: 4,3,2,1,0)
            int segmentIndex = batterySegments.Count - 1 - i;
            bool isVisible = remainingWhipShots > segmentIndex;
            
            Image segment = batterySegments[i];
            
            // Smooth görünürlük geçişi
            float targetAlpha = isVisible ? 1f : 0f;
            Color segmentColor = segment.color;
            segmentColor.a = Mathf.Lerp(segmentColor.a, targetAlpha, Time.deltaTime * 15f);
            
            // Renk güncelle (kalan atış sayısına göre)
            if (isVisible)
            {
                Color targetColor = GetBatteryColor(remainingWhipShots);
                segmentColor.r = Mathf.Lerp(segmentColor.r, targetColor.r, Time.deltaTime * 10f);
                segmentColor.g = Mathf.Lerp(segmentColor.g, targetColor.g, Time.deltaTime * 10f);
                segmentColor.b = Mathf.Lerp(segmentColor.b, targetColor.b, Time.deltaTime * 10f);
                
                // Son segmentlerde pulse efekti
                if (remainingWhipShots <= 2 && segmentColor.a > 0.5f)
                {
                    float pulse = 0.8f + Mathf.Sin(Time.time * 12f) * 0.2f;
                    segmentColor.r *= pulse;
                    segmentColor.g *= pulse;
                    segmentColor.b *= pulse;
                }
            }
            
            segment.color = segmentColor;
            segment.enabled = segmentColor.a > 0.01f;
        }
        
        // Text güncelle
        int displayRemaining = Mathf.Clamp(remainingWhipShots, 0, maxWhipShots);
        countText.text = $"{displayRemaining}/{maxWhipShots}";
        
        // Text rengi (batarya rengine göre)
        if (displayRemaining <= 0)
        {
            countText.color = textEmptyColor;
        }
        else
        {
            Color batteryTextColor = GetBatteryColor(displayRemaining);
            countText.color = Color.Lerp(countText.color, batteryTextColor, Time.deltaTime * 8f);
        }
        
        // Outline rengi güncelle
        if (batteryOutline != null)
        {
            Color outlineColor = GetBatteryColor(remainingWhipShots);
            outlineColor.a = 0.8f;
            batteryOutline.color = Color.Lerp(batteryOutline.color, outlineColor, Time.deltaTime * 8f);
        }
    }
    
    void OnDestroy()
    {
        if (whipCoroutine != null)
        {
            StopCoroutine(whipCoroutine);
        }
        
        if (whipContainer != null)
        {
            Destroy(whipContainer);
        }
        
        // UI temizle
        if (whipUIContainer != null)
        {
            Destroy(whipUIContainer);
        }
    }
    
    // Public metodlar
    public void ResetWhipShots()
    {
        remainingWhipShots = maxWhipShots;
    }
    
    public bool HasWhipBeenExhausted()
    {
        return maxWhipShots > 0 && remainingWhipShots <= 0;
    }
    
    public int GetRemainingWhipShots()
    {
        return maxWhipShots == 0 ? -1 : remainingWhipShots;
    }
    
    public void AddWhipShots(int amount)
    {
        remainingWhipShots += amount;
        remainingWhipShots = Mathf.Clamp(remainingWhipShots, 0, maxWhipShots);
    }
    
    void OnDrawGizmosSelected()
    {
        if (whipSpawnPoint != null)
        {
            Gizmos.color = whipColor;
            Gizmos.DrawRay(whipSpawnPoint.position, whipSpawnPoint.forward * whipLength);
            Gizmos.DrawWireSphere(whipSpawnPoint.position, collisionRadius);
        }
    }
}
