using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// EightGun Toy yardımcısı: A tuşu ile Toy yanına gelir, süre boyunca sadece düşmanlara lazer atar, sonra silaha geri döner.
/// Düşman yoksa ateş etmez. Toy ters dönmeden hareket eder, havada salınır. Lazer atma animasyonu vardır.
/// </summary>
[RequireComponent(typeof(GunFire))]
public class ToyHelper : MonoBehaviour
{
    public enum State { Idle, MovingToPlayer, Active, MovingBack }

    [Header("=== REFERANSLAR (Sadece Inspector'dan atanır) ===")]
    [Tooltip("Toy Transform - zorunlu, Inspector'da atanır")]
    public Transform toy;

    [Tooltip("Sol göz noktası - boşsa Toy merkezinden offset ile hesaplanır")]
    public Transform eyeLeft;

    [Tooltip("Sağ göz noktası - boşsa Toy merkezinden offset ile hesaplanır")]
    public Transform eyeRight;

    [Tooltip("Yanıma pozisyonu için VR kamera rig - boşsa Start'ta bulunur")]
    public OVRCameraRig ovrCameraRig;

    [Tooltip("İlk çıkış yeri (halihazırdaki yuva) - Toy'un silah üzerindeki konumu. EightGun prefab içinden atanır.")]
    public Transform toySlotTransform;

    [Tooltip("Animasyonun oynayacağı hedef - Toy bu transform'un pozisyonuna spline ile gider. Tek pozisyon.")]
    public Transform toySideTargetTransform;

    [Tooltip("Farklı atış pozisyonları - doldurulursa Toy ara sıra bu transform'lar arasında yer değiştirir, farklı düşmanlara ateş eder.")]
    public Transform[] toySideTargetTransforms;

    [Header("=== HAREKET ===")]
    [Tooltip("Yanına geliş / geri dönüş süresi (saniye)")]
    public float moveDuration = 0.6f;

    [Tooltip("Spline yüksekliği (hedef transform'a giderken eğrinin yukarı bombe miktarı)")]
    public float splineArcHeight = 0.15f;

    [Tooltip("CenterEyeAnchor'dan itibaren offset - sadece toySideTargetTransform boşsa kullanılır")]
    public Vector3 sideOffset = new Vector3(0.35f, -0.15f, 0.2f);

    [Tooltip("Hareket eğrisi (ease-in-out). Boşsa varsayılan S eğri kullanılır.")]
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("=== SÜRE ===")]
    [Tooltip("Yanında aktif kalma süresi (saniye) - Inspector'dan ayarlanabilir")]
    public float activeDuration = 15f;

    [Tooltip("Cooldown süresi (saniye)")]
    public float cooldownDuration = 12f;

    [Header("=== HASAR & LAZER ===")]
    [Tooltip("Tick başına hasar")]
    public float damagePerTick = 8f;

    [Tooltip("İkili atış aralığı (saniye) - her çift atıştan sonra bu süre beklenir")]
    public float damageTickInterval = 0.12f;

    [Tooltip("İkili atışlar arası boşluk (saniye)")]
    public float pairGap = 0.22f;

    [Tooltip("Toy'un farklı pozisyonlar arasında yer değiştirme aralığı (saniye). 0 = yer değiştirme yok.")]
    public float positionSwitchInterval = 4f;

    [Tooltip("Yer değiştirme animasyon süresi (saniye)")]
    public float positionSwitchDuration = 0.5f;

    [Tooltip("Atış menzili - Inspector'dan ayarlanabilir. Düşmanlara max bu mesafeye kadar atış yapılır.")]
    public float laserRange = 22f;

    [Tooltip("Düşman layer mask")]
    public LayerMask enemyLayerMask = -1;

    [Header("=== LAZER GÖRSEL ===")]
    public Color laserColor = new Color(1f, 0.3f, 0.1f, 0.9f);
    public Color laserGlowColor = new Color(1f, 0.5f, 0.2f, 0.35f);
    public float laserWidth = 0.02f;
    [Tooltip("Lazerin gözden hedefe uzanma animasyon süresi (saniye)")]
    [Range(0.04f, 0.25f)]
    public float laserFireAnimDuration = 0.14f;
    [Tooltip("Uzanma animasyonu eğrisi (0-1). Solda dik = hızlı fırlama, sağda yatay = yavaş isabet")]
    public AnimationCurve lineExtendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("=== GİDİŞ ANİMASYONU (Y 360° + büyüme) ===")]
    [Tooltip("Yerinden ayrılırken scale çarpanı (1 = aynı, 1.5 = %50 büyük)")]
    [Range(1f, 2f)]
    public float deployScaleMultiplier = 1.5f;

    [Header("=== HAVADA SALINMA ===")]
    [Tooltip("Salınma hızı")]
    public float swaySpeed = 2f;
    [Tooltip("Salınma genliği (x, y, z) - düşük değer yeterli")]
    public Vector3 swayAmount = new Vector3(0.015f, 0.02f, 0.01f);

    [Header("=== HAPTİK (VR optimize) ===")]
    [Range(0f, 1f)]
    public float hapticDeployReturn = 0.4f;
    [Range(0f, 1f)]
    public float hapticHit = 0.3f;
    [Tooltip("Aynı düşmana haptic en fazla bu aralıkla (saniye)")]
    public float hapticHitThrottle = 0.18f;

    [Header("=== SES (VR optimize) ===")]
    [Tooltip("Boşsa bu objede AudioSource aranır veya eklenir")]
    public AudioSource audioSource;
    public AudioClip deploySfx;
    public AudioClip returnSfx;
    public AudioClip laserHitSfx;
    [Tooltip("Lazer isabet sesi en fazla bu sıklıkta (saniye)")]
    public float laserHitSfxThrottle = 0.25f;

    [Header("=== COOLDOWN UI (VR optimize - minimal) ===")]
    [Tooltip("Cooldown UI konumu - boşsa Toy veya transform kullanılır")]
    public Transform cooldownUIPosition;
    public float cooldownUISize = 0.05f;
    public Vector3 cooldownUIOffset = new Vector3(0.05f, 0.08f, 0f);
    public Color cooldownUIColor = new Color(1f, 0.5f, 0.2f, 1f);
    public Color cooldownUIBgColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
    [Range(0.05f, 0.5f)]
    public float cooldownUIArcThickness = 0.2f;

    // State
    private State state = State.Idle;
    private float cooldownRemaining;
    private float activeTimer;
    private float damageTickTimer;
    private float activeStartTime;

    // Stored when deploying
    private Transform originalParent;
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private Vector3 originalLocalScale;
    private Vector3 originalWorldScale;
    private Vector3 worldTargetPosition;
    private Vector3 returnLocalScaleForSlot;

    // Lazer atma animasyonu (gözden hedefe uzanma)
    private float leftLineAnimT = 1f;
    private float rightLineAnimT = 1f;
    private bool lastFrameHadEnemies;
    private float pairGapRemaining;
    private float positionSwitchTimer;
    private int currentSideIndex = -1;
    private bool isSwitchingPosition;

    private Coroutine moveCoroutine;
    private LineRenderer lineLeftGlow;
    private LineRenderer lineRightGlow;

    // References
    private GunFire gunFire;
    private Transform centerEye;
    private LineRenderer lineLeft;
    private LineRenderer lineRight;
    private GameObject lineContainer;
    private Dictionary<EnemyHealth, float> lastHapticByEnemy = new Dictionary<EnemyHealth, float>();
    private float lastLaserHitSfxTime = -99f;

    // Cooldown UI - minimal (1 bg + 1 fill), update only when changed or low freq
    private GameObject cooldownUIContainer;
    private Canvas cooldownCanvas;
    private Image cooldownBgImage;
    private Image cooldownFillImage;
    private float lastCooldownPercent = -1f;
    private float lastCooldownUIUpdate;

    private void Awake()
    {
        gunFire = GetComponent<GunFire>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (toy == null)
        {
            Debug.LogWarning("ToyHelper: Toy atanmamış! Inspector'dan Toy transform'unu atayın.");
            enabled = false;
            return;
        }

        if (ovrCameraRig == null)
            ovrCameraRig = FindObjectOfType<OVRCameraRig>();
        if (ovrCameraRig != null)
            centerEye = ovrCameraRig.centerEyeAnchor;

        CreateLaserLines();
        CreateCooldownUI();
    }

    private void Update()
    {
        if (GameManager.IsRetryScreenActive) return; // Retry ekranında sadece HandRayUIInteractor ile butonlara tıklanabilir
        if (toy == null) return;

        if (state == State.Idle)
        {
            bool tutorialUnlimited = TutorialIntroController.TutorialEighthWeaponPhase && !TutorialIntroController.TutorialCompleteFreehand; // 8. silah tutorial: toy sürekli kullanılabilir (freehand'de normal)
            if (!tutorialUnlimited && cooldownRemaining > 0f)
            {
                cooldownRemaining -= Time.deltaTime;
                if (cooldownRemaining < 0f) cooldownRemaining = 0f;
            }
            if (OVRInput.GetDown(OVRInput.Button.One) && (tutorialUnlimited || cooldownRemaining <= 0f))
            {
                if (tutorialUnlimited)
                    TutorialIntroController.NotifyTutorialEighthWeaponToyUsed();
                StartDeploy();
            }
            UpdateCooldownUI();
            return;
        }

        if (state == State.Active)
        {
            activeTimer -= Time.deltaTime;
            if (!isSwitchingPosition)
                ApplySway();
            if (positionSwitchInterval > 0f && toySideTargetTransforms != null && toySideTargetTransforms.Length > 1 && !isSwitchingPosition)
            {
                positionSwitchTimer += Time.deltaTime;
                if (positionSwitchTimer >= positionSwitchInterval)
                {
                    positionSwitchTimer = 0f;
                    int nextIndex = GetRandomSideTargetIndexExcluding(currentSideIndex);
                    if (nextIndex != currentSideIndex)
                        moveCoroutine = StartCoroutine(SwitchPositionCoroutine(GetSideTargetPosition(nextIndex), nextIndex));
                }
            }
            damageTickTimer += Time.deltaTime;
            if (pairGapRemaining > 0f)
                pairGapRemaining -= Time.deltaTime;
            if (pairGapRemaining <= 0f && damageTickTimer >= damageTickInterval)
            {
                damageTickTimer = 0f;
                pairGapRemaining = pairGap;
                DoDamageTick();
            }
            UpdateLaserLines();
            if (activeTimer <= 0f)
                StartReturn();
            return;
        }

        UpdateCooldownUI();
    }

    /// <summary>LastGunFlameSpray A tuşunu tüketmeden önce çağırır.</summary>
    public bool WouldConsumeA()
    {
        return state == State.Idle && cooldownRemaining <= 0f && toy != null;
    }

    /// <summary>Toy aktif veya hareket halindeyse A ile ateş edilmesin.</summary>
    public bool IsIdle()
    {
        return state == State.Idle;
    }

    private Vector3 GetSideTargetPosition(int index)
    {
        if (toySideTargetTransforms != null && toySideTargetTransforms.Length > 0)
        {
            index = Mathf.Clamp(index, 0, toySideTargetTransforms.Length - 1);
            Transform t = toySideTargetTransforms[index];
            if (t != null) return t.position;
            return toySideTargetTransforms[0] != null ? toySideTargetTransforms[0].position : worldTargetPosition;
        }
        if (toySideTargetTransform != null)
            return toySideTargetTransform.position;
        return worldTargetPosition;
    }

    private int GetRandomSideTargetIndexExcluding(int exclude)
    {
        if (toySideTargetTransforms == null || toySideTargetTransforms.Length <= 1) return 0;
        int count = 0;
        foreach (Transform t in toySideTargetTransforms) if (t != null) count++;
        if (count <= 1) return 0;
        int pick = Random.Range(0, toySideTargetTransforms.Length);
        for (int i = 0; i < toySideTargetTransforms.Length; i++)
        {
            if (toySideTargetTransforms[pick] == null) { pick = (pick + 1) % toySideTargetTransforms.Length; continue; }
            if (pick != exclude) return pick;
            pick = (pick + 1) % toySideTargetTransforms.Length;
        }
        return 0;
    }

    private void StartDeploy()
    {
        if (toySideTargetTransform != null)
            worldTargetPosition = toySideTargetTransform.position;
        else if (toySideTargetTransforms != null && toySideTargetTransforms.Length > 0)
        {
            currentSideIndex = 0;
            worldTargetPosition = GetSideTargetPosition(0);
        }
        else if (centerEye == null)
        {
            if (Camera.main != null)
                worldTargetPosition = Camera.main.transform.position + Camera.main.transform.TransformDirection(sideOffset);
            else
                worldTargetPosition = toy.position + Vector3.right * 0.5f + Vector3.up * 0.2f;
        }
        else
        {
            worldTargetPosition = centerEye.position + centerEye.TransformDirection(sideOffset);
        }

        originalParent = toy.parent;
        originalLocalPos = toy.localPosition;
        originalLocalRot = toy.localRotation;
        originalLocalScale = toy.localScale;
        originalWorldScale = toy.lossyScale;
        if (toySlotTransform != null)
        {
            Vector3 slotLossy = toySlotTransform.lossyScale;
            returnLocalScaleForSlot = new Vector3(
                Mathf.Approximately(slotLossy.x, 0f) ? 1f : originalWorldScale.x / slotLossy.x,
                Mathf.Approximately(slotLossy.y, 0f) ? 1f : originalWorldScale.y / slotLossy.y,
                Mathf.Approximately(slotLossy.z, 0f) ? 1f : originalWorldScale.z / slotLossy.z
            );
        }

        toy.SetParent(null, true);

        state = State.MovingToPlayer;
        if (lineContainer != null) lineContainer.SetActive(false);

        // Haptic: deploy bir kez
        TriggerHaptic(hapticDeployReturn);
        if (deploySfx != null && audioSource != null)
            audioSource.PlayOneShot(deploySfx);

        moveCoroutine = StartCoroutine(MoveToPlayerCoroutine());
    }

    private static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }

    private IEnumerator MoveToPlayerCoroutine()
    {
        Vector3 startPos = toy.position;
        Quaternion startRot = toy.rotation;
        Vector3 targetWorldScale = new Vector3(
            originalWorldScale.x * deployScaleMultiplier,
            originalWorldScale.y * deployScaleMultiplier,
            originalWorldScale.z * deployScaleMultiplier
        );
        Vector3 p0 = startPos;
        Vector3 p2 = worldTargetPosition;
        Vector3 mid = (p0 + p2) * 0.5f + Vector3.up * splineArcHeight;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            if (t > 1f) t = 1f;
            float curveT = moveCurve != null && moveCurve.keys.Length > 0 ? moveCurve.Evaluate(t) : t;
            toy.position = QuadraticBezier(p0, mid, p2, curveT);
            toy.rotation = startRot * Quaternion.Euler(0f, 360f * curveT, 0f);
            toy.localScale = Vector3.Lerp(originalWorldScale, targetWorldScale, curveT);
            yield return null;
        }

        toy.position = worldTargetPosition;
        toy.rotation = startRot * Quaternion.Euler(0f, 180f, 0f);
        toy.localScale = targetWorldScale;
        state = State.Active;
        activeTimer = activeDuration;
        damageTickTimer = 0f;
        pairGapRemaining = 0f;
        activeStartTime = Time.time;
        lastHapticByEnemy.Clear();
        lastFrameHadEnemies = false;
        moveCoroutine = null;
        positionSwitchTimer = 0f;
    }

    private IEnumerator SwitchPositionCoroutine(Vector3 newTargetPos, int newIndex)
    {
        isSwitchingPosition = true;
        Vector3 startPos = worldTargetPosition;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, positionSwitchDuration);
            if (t > 1f) t = 1f;
            float curveT = moveCurve != null && moveCurve.keys.Length > 0 ? moveCurve.Evaluate(t) : t;
            worldTargetPosition = Vector3.Lerp(startPos, newTargetPos, curveT);
            toy.position = worldTargetPosition;
            yield return null;
        }
        worldTargetPosition = newTargetPos;
        toy.position = worldTargetPosition;
        currentSideIndex = newIndex;
        isSwitchingPosition = false;
        moveCoroutine = null;
    }

    private void ApplySway()
    {
        if (toy == null) return;
        float t = Time.time;
        Vector3 sway = new Vector3(
            Mathf.Sin(t * swaySpeed) * swayAmount.x,
            Mathf.Sin(t * swaySpeed * 1.15f) * swayAmount.y,
            Mathf.Sin(t * swaySpeed * 0.9f) * swayAmount.z
        );
        toy.position = worldTargetPosition + sway;
    }

    private void StartReturn()
    {
        state = State.MovingBack;
        if (lineContainer != null) lineContainer.SetActive(false);

        TriggerHaptic(hapticDeployReturn);
        if (returnSfx != null && audioSource != null)
            audioSource.PlayOneShot(returnSfx);

        moveCoroutine = StartCoroutine(MoveBackCoroutine());
    }

    private IEnumerator MoveBackCoroutine()
    {
        Vector3 startPos = toy.position;
        Quaternion startRot = toy.rotation;
        Vector3 startScale = toy.localScale;
        float t = 0f;

        Vector3 targetWorldPos;
        Quaternion targetWorldRot;
        Transform returnParent;
        Vector3 returnLocalPos;
        Quaternion returnLocalRot;
        Vector3 returnLocalScale;

        if (toySlotTransform != null)
        {
            targetWorldPos = toySlotTransform.position;
            targetWorldRot = toySlotTransform.rotation;
            returnParent = toySlotTransform;
            returnLocalPos = Vector3.zero;
            returnLocalRot = Quaternion.identity;
            returnLocalScale = returnLocalScaleForSlot;
        }
        else
        {
            targetWorldPos = originalParent != null ? originalParent.TransformPoint(originalLocalPos) : startPos + Vector3.forward;
            targetWorldRot = originalParent != null ? originalParent.rotation * originalLocalRot : originalLocalRot;
            returnParent = originalParent;
            returnLocalPos = originalLocalPos;
            returnLocalRot = originalLocalRot;
            returnLocalScale = originalLocalScale;
        }

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            if (t > 1f) t = 1f;
            float curveT = moveCurve != null && moveCurve.keys.Length > 0 ? moveCurve.Evaluate(t) : t;
            toy.position = Vector3.Lerp(startPos, targetWorldPos, curveT);
            toy.rotation = Quaternion.Slerp(startRot, targetWorldRot, curveT);
            toy.localScale = Vector3.Lerp(startScale, originalWorldScale, curveT);
            yield return null;
        }

        if (returnParent != null)
        {
            toy.SetParent(returnParent, true);
            toy.localPosition = returnLocalPos;
            toy.localRotation = returnLocalRot;
            toy.localScale = returnLocalScale;
        }

        state = State.Idle;
        if (!TutorialIntroController.TutorialEighthWeaponPhase || TutorialIntroController.TutorialCompleteFreehand)
            cooldownRemaining = cooldownDuration;
        lastCooldownPercent = -1f;
        moveCoroutine = null;
    }

    private void ForceReturn()
    {
        if (toy == null) return;
        if (state == State.Idle) return;

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        if (lineContainer != null) lineContainer.SetActive(false);

        Vector3 targetWorldPos;
        Quaternion targetWorldRot;
        Transform returnParent;
        Vector3 returnLocalPos;
        Quaternion returnLocalRot;
        Vector3 returnLocalScale;

        if (toySlotTransform != null)
        {
            returnParent = toySlotTransform;
            returnLocalPos = Vector3.zero;
            returnLocalRot = Quaternion.identity;
            returnLocalScale = returnLocalScaleForSlot;
        }
        else if (originalParent != null)
        {
            returnParent = originalParent;
            returnLocalPos = originalLocalPos;
            returnLocalRot = originalLocalRot;
            returnLocalScale = originalLocalScale;
        }
        else
        {
            state = State.Idle;
            return;
        }

        toy.SetParent(returnParent, true);
        toy.localPosition = returnLocalPos;
        toy.localRotation = returnLocalRot;
        toy.localScale = returnLocalScale;
        state = State.Idle;
    }

    private void OnDisable()
    {
        ForceReturn();
    }

    private void DoDamageTick()
    {
        if (toy == null) return;

        Vector3 origin = toy.position;
        float range = (TutorialIntroController.TutorialEighthWeaponPhase && !TutorialIntroController.TutorialCompleteFreehand) ? 50f : laserRange;
        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayerMask);
        List<EnemyHealth> enemies = new List<EnemyHealth>();
        foreach (Collider c in hits)
        {
            EnemyHealth eh = c.GetComponentInParent<EnemyHealth>();
            if (eh != null && !enemies.Contains(eh))
                enemies.Add(eh);
        }
        if (enemies.Count == 0) return;

        leftLineAnimT = 0f;
        rightLineAnimT = 0f;

        enemies.Sort((a, b) =>
        {
            float da = (a.transform.position - origin).sqrMagnitude;
            float db = (b.transform.position - origin).sqrMagnitude;
            return da.CompareTo(db);
        });
        int maxTargets = (TutorialIntroController.TutorialEighthWeaponPhase && !TutorialIntroController.TutorialCompleteFreehand) ? enemies.Count : Mathf.Min(2, enemies.Count);
        bool didHitAny = false;

        Vector3 leftEyePos = GetEyePosition(true);
        Vector3 rightEyePos = GetEyePosition(false);

        for (int i = 0; i < maxTargets; i++)
        {
            EnemyHealth target = enemies[i];
            Vector3 aimPoint = GetEnemyAimPoint(target);
            Vector3 toTarget = aimPoint - (i % 2 == 0 ? leftEyePos : rightEyePos);
            if (toTarget.sqrMagnitude < 0.01f) continue;
            toTarget.Normalize();

            bool useLeftEye = (i % 2 == 0);
            Vector3 eyePos = useLeftEye ? leftEyePos : rightEyePos;
            RaycastHit rayHit;
            if (Physics.Raycast(eyePos, toTarget, out rayHit, range, enemyLayerMask))
            {
                EnemyHealth eh = rayHit.collider.GetComponentInParent<EnemyHealth>();
                if (eh != null)
                {
                    eh.TakeDamage(damagePerTick, rayHit.collider, fromFlameSpray: false, fromSecondary: true, tutorialWeaponIndex: 7);
                    didHitAny = true;
                    float now = Time.time;
                    if (now - lastLaserHitSfxTime >= laserHitSfxThrottle && laserHitSfx != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(laserHitSfx);
                        lastLaserHitSfxTime = now;
                    }
                    float lastHaptic = -99f;
                    lastHapticByEnemy.TryGetValue(eh, out lastHaptic);
                    if (now - lastHaptic >= hapticHitThrottle)
                    {
                        TriggerHaptic(hapticHit);
                        lastHapticByEnemy[eh] = now;
                    }
                }
            }
        }
    }

    private Vector3 GetEyePosition(bool left)
    {
        if (left && eyeLeft != null) return eyeLeft.position;
        if (!left && eyeRight != null) return eyeRight.position;
        float sign = left ? -1f : 1f;
        return toy.position + toy.right * (sign * 0.05f) + toy.forward * 0.02f;
    }

    private Vector3 GetEnemyAimPoint(EnemyHealth eh)
    {
        if (eh == null) return Vector3.zero;
        Collider[] colliders = eh.GetComponentsInChildren<Collider>();
        if (colliders != null && colliders.Length > 0)
        {
            bool first = true;
            Bounds combined = new Bounds(eh.transform.position, Vector3.zero);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (!colliders[i].enabled || !colliders[i].gameObject.activeInHierarchy) continue;
                if (first) { combined = colliders[i].bounds; first = false; }
                else combined.Encapsulate(colliders[i].bounds);
            }
            if (!first) return combined.center;
        }
        return eh.transform.position + Vector3.up * 0.5f;
    }

    private void CreateLaserLines()
    {
        if (toy == null) return;

        lineContainer = new GameObject("ToyLaserLines");
        lineContainer.transform.SetParent(toy);
        lineContainer.transform.localPosition = Vector3.zero;
        lineContainer.transform.localRotation = Quaternion.identity;
        lineContainer.transform.localScale = Vector3.one;
        lineContainer.SetActive(false);

        GameObject leftGlowObj = new GameObject("LineLeftGlow");
        leftGlowObj.transform.SetParent(lineContainer.transform, false);
        lineLeftGlow = leftGlowObj.AddComponent<LineRenderer>();
        SetupLine(lineLeftGlow, laserWidth * 2.5f, laserGlowColor);

        GameObject leftObj = new GameObject("LineLeft");
        leftObj.transform.SetParent(lineContainer.transform, false);
        lineLeft = leftObj.AddComponent<LineRenderer>();
        SetupLine(lineLeft, laserWidth, laserColor);

        GameObject rightGlowObj = new GameObject("LineRightGlow");
        rightGlowObj.transform.SetParent(lineContainer.transform, false);
        lineRightGlow = rightGlowObj.AddComponent<LineRenderer>();
        SetupLine(lineRightGlow, laserWidth * 2.5f, laserGlowColor);

        GameObject rightObj = new GameObject("LineRight");
        rightObj.transform.SetParent(lineContainer.transform, false);
        lineRight = rightObj.AddComponent<LineRenderer>();
        SetupLine(lineRight, laserWidth, laserColor);
    }

    private void SetupLine(LineRenderer line, float width, Color color)
    {
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width * 0.5f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        if (line.material.shader.name == "Hidden/InternalErrorShader")
            line.material = new Material(Shader.Find("Unlit/Color"));
        line.material.color = color;
        line.material.SetColor("_Color", color);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.numCapVertices = 6;
        line.numCornerVertices = 6;
        line.enabled = true;
    }

    private void UpdateLaserLines()
    {
        if (lineLeft == null || lineRight == null || toy == null) return;

        Vector3 origin = toy.position;
        float range = (TutorialIntroController.TutorialEighthWeaponPhase && !TutorialIntroController.TutorialCompleteFreehand) ? 50f : laserRange;
        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayerMask);
        List<EnemyHealth> sorted = new List<EnemyHealth>();
        foreach (Collider c in hits)
        {
            EnemyHealth eh = c.GetComponentInParent<EnemyHealth>();
            if (eh != null && !sorted.Contains(eh)) sorted.Add(eh);
        }
        sorted.Sort((a, b) => (a.transform.position - origin).sqrMagnitude.CompareTo((b.transform.position - origin).sqrMagnitude));

        if (sorted.Count == 0)
        {
            if (lineContainer != null) lineContainer.SetActive(false);
            lastFrameHadEnemies = false;
            return;
        }

        if (lineContainer != null) lineContainer.SetActive(true);
        if (!lastFrameHadEnemies)
        {
            leftLineAnimT = 0f;
            rightLineAnimT = 0f;
        }
        lastFrameHadEnemies = true;

        Vector3 leftEye = GetEyePosition(true);
        Vector3 rightEye = GetEyePosition(false);
        Vector3 aimLeft = GetEnemyAimPoint(sorted[0]);
        Vector3 aimRight = sorted.Count > 1 ? GetEnemyAimPoint(sorted[1]) : aimLeft;
        Vector3 dirLeft = (aimLeft - leftEye).normalized;
        Vector3 dirRight = (aimRight - rightEye).normalized;

        RaycastHit hitL, hitR;
        Vector3 endL = leftEye + dirLeft * range;
        Vector3 endR = rightEye + dirRight * range;
        if (Physics.Raycast(leftEye, dirLeft, out hitL, range, enemyLayerMask))
            endL = hitL.point;
        if (Physics.Raycast(rightEye, dirRight, out hitR, range, enemyLayerMask))
            endR = hitR.point;

        float dt = Time.deltaTime;
        leftLineAnimT = Mathf.MoveTowards(leftLineAnimT, 1f, dt / Mathf.Max(0.02f, laserFireAnimDuration));
        rightLineAnimT = Mathf.MoveTowards(rightLineAnimT, 1f, dt / Mathf.Max(0.02f, laserFireAnimDuration));

        float curveL = lineExtendCurve != null && lineExtendCurve.keys.Length > 0 ? lineExtendCurve.Evaluate(leftLineAnimT) : leftLineAnimT;
        float curveR = lineExtendCurve != null && lineExtendCurve.keys.Length > 0 ? lineExtendCurve.Evaluate(rightLineAnimT) : rightLineAnimT;
        Vector3 currentEndL = Vector3.Lerp(leftEye, endL, curveL);
        Vector3 currentEndR = Vector3.Lerp(rightEye, endR, curveR);

        float animPulse = Mathf.Clamp01(1f - Mathf.Max(leftLineAnimT, rightLineAnimT));
        float burst = animPulse > 0.8f ? Mathf.InverseLerp(0.8f, 1f, animPulse) : 0f;
        float startW = laserWidth * (1f + animPulse * 0.6f + burst * 1.2f);
        float endW = laserWidth * (0.6f + burst * 0.4f);
        lineLeft.startWidth = startW;
        lineLeft.endWidth = endW;
        lineRight.startWidth = startW;
        lineRight.endWidth = endW;
        if (lineLeftGlow != null)
        {
            lineLeftGlow.startWidth = laserWidth * 2.5f * (1f + animPulse);
            lineLeftGlow.endWidth = laserWidth * 1.2f * curveL;
            lineLeftGlow.SetPosition(0, leftEye);
            lineLeftGlow.SetPosition(1, Vector3.Lerp(leftEye, currentEndL, 0.85f));
        }
        if (lineRightGlow != null)
        {
            lineRightGlow.startWidth = laserWidth * 2.5f * (1f + animPulse);
            lineRightGlow.endWidth = laserWidth * 1.2f * curveR;
            lineRightGlow.SetPosition(0, rightEye);
            lineRightGlow.SetPosition(1, Vector3.Lerp(rightEye, currentEndR, 0.85f));
        }

        lineLeft.SetPosition(0, leftEye);
        lineLeft.SetPosition(1, currentEndL);
        lineRight.SetPosition(0, rightEye);
        lineRight.SetPosition(1, currentEndR);
    }

    private void TriggerHaptic(float strength)
    {
        bool left = gunFire != null && gunFire.isLeftHanded;
        OVRInput.Controller c = left ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
        OVRInput.SetControllerVibration(strength, strength, c);
        // Kısa süre sonra kapat
        StartCoroutine(StopHapticAfter(0.05f, c));
    }

    private IEnumerator StopHapticAfter(float delay, OVRInput.Controller c)
    {
        yield return new WaitForSeconds(delay);
        OVRInput.SetControllerVibration(0f, 0f, c);
    }

    private void CreateCooldownUI()
    {
        Transform anchor = cooldownUIPosition != null ? cooldownUIPosition : (toy != null ? toy : transform);
        cooldownUIContainer = new GameObject("ToyCooldownUI");
        cooldownUIContainer.transform.SetParent(anchor, false);
        cooldownUIContainer.transform.localPosition = cooldownUIOffset;
        cooldownUIContainer.transform.localRotation = Quaternion.identity;
        cooldownUIContainer.transform.localScale = Vector3.one;

        cooldownCanvas = cooldownUIContainer.AddComponent<Canvas>();
        cooldownCanvas.renderMode = RenderMode.WorldSpace;
        UICameraStackSetup.Instance?.RegisterWorldSpaceCanvas(cooldownCanvas);
        RectTransform rect = cooldownUIContainer.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2, 2);
        rect.localScale = Vector3.one * cooldownUISize * 0.01f;

        cooldownBgImage = CreateCooldownArc("Bg", cooldownUIBgColor, 1f);
        cooldownFillImage = CreateCooldownArc("Fill", cooldownUIColor, 0f);
    }

    private Image CreateCooldownArc(string name, Color color, float fillAmount)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(cooldownUIContainer.transform, false);
        Image img = obj.AddComponent<Image>();
        img.sprite = CreateRingSprite(64, cooldownUIArcThickness);
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Radial360;
        img.fillOrigin = (int)Image.Origin360.Top;
        img.fillClockwise = true;
        img.color = color;
        img.fillAmount = fillAmount;
        img.raycastTarget = false;
        RectTransform r = obj.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(100, 100);
        r.anchoredPosition = Vector2.zero;
        return img;
    }

    private Sprite CreateRingSprite(int res, float thickness)
    {
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = res / 2f;
        float outer = center - 2f;
        float inner = outer * (1f - thickness);
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (d >= inner && d <= outer)
                    tex.SetPixel(x, y, new Color(1, 1, 1, 1));
                else
                    tex.SetPixel(x, y, clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100);
    }

    private void UpdateCooldownUI()
    {
        if (cooldownUIContainer == null || cooldownFillImage == null) return;

        bool tutorialUnlimited = TutorialIntroController.TutorialEighthWeaponPhase && !TutorialIntroController.TutorialCompleteFreehand;
        cooldownUIContainer.SetActive(!tutorialUnlimited);
        if (tutorialUnlimited) return;

        float percent = cooldownDuration > 0f ? 1f - (cooldownRemaining / cooldownDuration) : 1f;
        // VR optimize: sadece değiştiğinde veya 0.1 s'de bir güncelle
        float now = Time.time;
        if (state != State.Idle && state != State.MovingBack) percent = 0f;
        if (Mathf.Abs(percent - lastCooldownPercent) < 0.01f && now - lastCooldownUIUpdate < 0.1f)
            return;
        lastCooldownPercent = percent;
        lastCooldownUIUpdate = now;

        cooldownFillImage.fillAmount = percent;
        if (Camera.main != null)
        {
            Vector3 look = cooldownUIContainer.transform.position - Camera.main.transform.position;
            if (look.sqrMagnitude > 0.01f)
                cooldownUIContainer.transform.rotation = Quaternion.LookRotation(look);
        }
    }

    private void OnDestroy()
    {
        if (lineContainer != null) Destroy(lineContainer);
        if (cooldownUIContainer != null) Destroy(cooldownUIContainer);
    }
}
