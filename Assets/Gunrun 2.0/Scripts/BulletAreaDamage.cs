using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Mermi bir yere çarptığında belirli çapta alan hasarı verir. Önce küre açılır, sonra içindekilere hasar verilir.
/// </summary>
[RequireComponent(typeof(Bullet))]
public class BulletAreaDamage : MonoBehaviour
{
    [Header("Area Damage")]
    [Tooltip("Alan hasarı yarıçapı")]
    public float radius = 2.5f;

    [Tooltip("Merkezdeki tam hasar (Bullet.damage ile aynı veya farklı olabilir)")]
    public float centerDamage = 25f;

    [Tooltip("Hasar mesafeyle nasıl azalsın: 0=linear, 1=quadratic (kenarda çok az), 2=cubic")]
    [Range(0f, 2f)]
    public float falloffPower = 1.2f;

    [Tooltip("Kenarda minimum hasar oranı (0–1). 0.2 = en uzaktakine %20 hasar")]
    [Range(0f, 1f)]
    public float minDamageRatio = 0.2f;

    [Header("Hit Feedback")]
    [Tooltip("Her vurulan düşmanda yuvarlak minimal hit effect çıksın mı?")]
    public bool hitEffectPerEnemy = true;

    [Tooltip("Merkez patlama + genişleyen küre (alan hasarı görseli)")]
    public bool explosionEffect = true;

    [Tooltip("Küre tam açıldıktan ne kadar sonra hasar verilsin (saniye)")]
    public float damageDelayAfterExpand = 0.15f;

    [Tooltip("Hasar verdikten 1-2 saniye sonra küre kapansın")]
    [Range(1f, 2f)]
    public float sphereCloseDelayAfterDamage = 1.5f;

    [Tooltip("Küre efektinin tekrar çıkması için beklenecek süre (saniye). 0 = her vuruşta çıkar.")]
    [Range(0f, 3f)]
    public float sphereCooldownSeconds = 1.2f;

    private static float _lastSphereSpawnTime = -999f;

    [Tooltip("Alan hasarı alan düşmanlarda hafif shake süresi")]
    public float enemyShakeDuration = 0.25f;

    [Tooltip("Shake şiddeti")]
    public float enemyShakeIntensity = 0.04f;

    [Tooltip("Alan içindeki düşmanlara yukarıdan yıldırım inmesi (hasar anlatımı)")]
    public bool lightningOnAreaTargets = true;

    [Header("Lightning Hasarı")]
    [Tooltip("Yıldırım hasarı. 0 = alan hasarı (centerDamage * falloff) kullanılır. >0 = bu değer kullanılır.")]
    public float lightningDamage = 0f;

    [Tooltip("Yıldırımın başladığı yükseklik")]
    public float lightningHeight = 8f;

    [Tooltip("Yıldırımla ölen düşmanda patlama efekti çıksın mı?")]
    public bool lightningKillExplosion = true;

    [Tooltip("Vurduktan sonra 'AREA DAMAGE' yazısı çıksın mı?")]
    public bool showAreaDamageText = true;

    private bool _triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        // Sadece düşman veya hedef tahtasına çarpınca tetikle (isteğe göre "her yere" de açılabilir)
        if (other.GetComponentInParent<EnemyHealth>() != null || other.GetComponentInParent<TargetBoardHealth>() != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            TriggerAreaDamage(hitPoint, other);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_triggered) return;
        Collider other = collision.collider;
        if (other.GetComponentInParent<EnemyHealth>() != null || other.GetComponentInParent<TargetBoardHealth>() != null)
        {
            Vector3 hitPoint = collision.GetContact(0).point;
            TriggerAreaDamage(hitPoint, other);
        }
    }

    private void TriggerAreaDamage(Vector3 center, Collider triggerCollider)
    {
        _triggered = true;

        Bullet bullet = GetComponent<Bullet>();
        int tutorialWeaponIndex = bullet != null && bullet.weaponIndex >= 0 ? bullet.weaponIndex : (bullet != null && bullet.isFromSecondary ? 8 : -1);
        bool fromSecondary = bullet != null && bullet.isFromSecondary;
        EnemyHealth primaryEnemy = triggerCollider.GetComponentInParent<EnemyHealth>();
        TargetBoardHealth primaryBoard = triggerCollider.GetComponentInParent<TargetBoardHealth>();

        if (explosionEffect)
        {
            float holdDur = damageDelayAfterExpand + sphereCloseDelayAfterDamage;
            SpawnExplosionEffect(center, holdDur, primaryEnemy, primaryBoard, tutorialWeaponIndex, fromSecondary);
        }

        float delay = 0.9f + damageDelayAfterExpand;
        var runner = new GameObject("AreaDamageDelayedRunner").AddComponent<AreaDamageDelayedRunner>();
        runner.Run(this, center, primaryEnemy, primaryBoard, tutorialWeaponIndex, fromSecondary, delay);
    }

    private void SpawnLightningFromAbove(Vector3 targetPos)
    {
        Vector3 from = targetPos + Vector3.up * lightningHeight + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
        GameObject root = new GameObject("AreaLightning");

        // FifthGunLaser tarzı: glow + core katmanları, kalın ve parlak
        Color lightningColor = new Color(0.4f, 0.6f, 1f, 1f);
        Color coreColor = new Color(1f, 1f, 1f, 1f);
        Color innerGlowColor = new Color(0.6f, 0.8f, 1f, 0.5f);

        for (int i = 0; i < 5; i++)
        {
            var go = new GameObject("Bolt");
            go.transform.SetParent(root.transform);

            float glowW = 0.38f - i * 0.055f;
            float coreW = 0.12f - i * 0.018f;
            var glowLr = CreateSmoothBoltLine(go.transform, glowW, innerGlowColor);
            var coreLr = CreateSmoothBoltLine(go.transform, coreW, coreColor);

            var anim = go.AddComponent<LightningBoltAnim>();
            anim.from = from + new Vector3((i - 2) * 0.15f, 0f, (Random.value - 0.5f) * 0.3f);
            anim.to = targetPos;
            anim.glowLineRenderer = glowLr;
            anim.coreLineRenderer = coreLr;
            anim.initialGlowWidth = glowW;
            anim.initialCoreWidth = coreW;
            anim.segments = 32;
            anim.jaggedAmount = 0.16f;
            anim.duration = 0.75f;
        }

        // Bitiş noktasında FifthGunLaser tarzı ışık
        var lightObj = new GameObject("LightningLight");
        lightObj.transform.SetParent(root.transform);
        lightObj.transform.position = targetPos;
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = lightningColor;
        light.intensity = 12f;
        light.range = 6f;
        light.shadows = LightShadows.None;
        Object.Destroy(lightObj, 0.85f);

        SpawnLightningImpactBurst(targetPos);
        SpawnLightningImpactFlash(targetPos);

        Destroy(root, 0.85f);
    }

    /// <summary>Yıldırımla ölen düşmanda elektrik temalı patlama efekti.</summary>
    private void SpawnLightningKillExplosion(Vector3 position)
    {
        GameObject root = new GameObject("LightningKillExplosion");
        root.transform.position = position;

        // Elektrik mavisi merkez flaşı
        GameObject flashGo = new GameObject("LightningExplosionFlash");
        flashGo.transform.SetParent(root.transform);
        flashGo.transform.localPosition = Vector3.zero;
        var ps = flashGo.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.2f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 1f),
            new Color(0.4f, 0.7f, 1f, 1f)
        );
        main.maxParticles = 25;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.duration = 0.08f;
        main.loop = false;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.4f, 0.75f, 1f), 0.5f), new GradientColorKey(new Color(0.2f, 0.5f, 1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.5f, 0.3f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = g;
        SetupParticleRenderer(flashGo);

        // Genişleyen elektrik halkası
        GameObject ringGo = new GameObject("LightningExplosionRing");
        ringGo.transform.SetParent(root.transform);
        ringGo.transform.localPosition = Vector3.zero;
        var ringPs = ringGo.AddComponent<ParticleSystem>();
        var ringMain = ringPs.main;
        ringMain.startLifetime = 0.4f;
        ringMain.startSpeed = 0f;
        ringMain.startSize = 0.15f;
        ringMain.startColor = new Color(0.5f, 0.85f, 1f, 0.9f);
        ringMain.maxParticles = 1;
        ringMain.simulationSpace = ParticleSystemSimulationSpace.World;
        ringMain.playOnAwake = true;
        var ringEm = ringPs.emission;
        ringEm.rateOverTime = 0f;
        ringEm.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        var sizeOL = ringPs.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.5f), new Keyframe(0.2f, 2f), new Keyframe(1f, 4f)
        ));
        var colOL = ringPs.colorOverLifetime;
        colOL.enabled = true;
        var ringG = new Gradient();
        ringG.SetKeys(
            new[] { new GradientColorKey(new Color(0.6f, 0.9f, 1f), 0f), new GradientColorKey(new Color(0.3f, 0.6f, 1f), 1f) },
            new[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colOL.color = ringG;
        var ringRend = ringPs.GetComponent<ParticleSystemRenderer>();
        ringRend.material = CreateSoftParticleMaterial();
        ringRend.renderMode = ParticleSystemRenderMode.Billboard;
        ringPs.Play();

        Destroy(root, 0.6f);
    }

    /// <summary>FifthGunLaser tarzı yıldırım çizgisi: width curve + gradient.</summary>
    private static LineRenderer CreateSmoothBoltLine(Transform parent, float width, Color color)
    {
        var obj = new GameObject("BoltLine");
        obj.transform.SetParent(parent);
        var lr = obj.AddComponent<LineRenderer>();

        // Additive = parlak glow (FifthGunLaser tarzı)
        var mat = new Material(Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Particles/Standard Unlit"));
        mat.color = color;
        if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", color);
        lr.material = mat;

        var widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.9f);
        widthCurve.AddKey(0.1f, 1f);
        widthCurve.AddKey(0.5f, 1.08f);
        widthCurve.AddKey(0.9f, 0.85f);
        widthCurve.AddKey(1f, 0.5f);
        lr.widthCurve = widthCurve;
        lr.widthMultiplier = width;

        lr.positionCount = 0;
        lr.useWorldSpace = true;
        lr.numCapVertices = 5;
        lr.numCornerVertices = 8;
        lr.textureMode = LineTextureMode.Stretch;

        var grad = new Gradient();
        Color bright = Color.Lerp(color, Color.white, 0.5f);
        grad.SetKeys(
            new[] { new GradientColorKey(bright, 0f), new GradientColorKey(color, 0.2f), new GradientColorKey(color, 0.8f), new GradientColorKey(bright, 1f) },
            new[] { new GradientAlphaKey(color.a * 0.95f, 0f), new GradientAlphaKey(color.a, 0.25f), new GradientAlphaKey(color.a, 0.75f), new GradientAlphaKey(color.a * 0.7f, 1f) }
        );
        lr.colorGradient = grad;
        return lr;
    }

    private void SpawnLightningImpactBurst(Vector3 pos)
    {
        GameObject go = new GameObject("LightningBurst");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 14f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 1f, 1f, 1f),
            new Color(0.4f, 0.75f, 1f, 1f)
        );
        main.maxParticles = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.duration = 0.08f;
        main.loop = false;
        main.gravityModifier = -0.05f;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 70) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.material = CreateSoftParticleMaterial();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.sortingOrder = 199;
        ps.Play();
        Destroy(go, 0.6f);
    }

    private void SpawnLightningImpactFlash(Vector3 pos)
    {
        GameObject go = new GameObject("LightningFlash");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 0f;
        main.startSize = 0.8f;
        main.startColor = new Color(1f, 1f, 1f, 0.95f);
        main.maxParticles = 1;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });
        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.15f), new Keyframe(0.08f, 1.3f), new Keyframe(0.35f, 1.5f), new Keyframe(1f, 0f)
        ));
        var colOL = ps.colorOverLifetime;
        colOL.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.5f, 0.85f, 1f), 0.5f), new GradientColorKey(new Color(0.3f, 0.6f, 1f), 1f) },
            new[] { new GradientAlphaKey(0.98f, 0f), new GradientAlphaKey(0.6f, 0.3f), new GradientAlphaKey(0f, 1f) }
        );
        colOL.color = g;
        var r = ps.GetComponent<ParticleSystemRenderer>();
        r.material = CreateSoftParticleMaterial();
        r.renderMode = ParticleSystemRenderMode.Billboard;
        r.sortingOrder = 201;
        ps.Play();
        Destroy(go, 0.4f);
    }

    private void ApplyEnemyShake(GameObject enemy)
    {
        if (enemy.GetComponent<AreaDamageShake>() != null) return;
        var shake = enemy.AddComponent<AreaDamageShake>();
        shake.duration = enemyShakeDuration;
        shake.intensity = enemyShakeIntensity;
    }

    private void ApplyAreaDamageInternal(Vector3 center, EnemyHealth primaryEnemy, TargetBoardHealth primaryBoard, int tutorialWeaponIndex, bool fromSecondary)
    {
        int areaDamageHitCount = 0;

        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (Collider col in hits)
        {
            EnemyHealth eh = col.GetComponentInParent<EnemyHealth>();
            if (eh != null)
            {
                if (eh == primaryEnemy) continue;

                Vector3 closest = col.ClosestPoint(center);
                float dist = Vector3.Distance(center, closest);
                float t = Mathf.Clamp01(dist / radius);
                float falloff = Mathf.Lerp(1f, minDamageRatio, Mathf.Pow(t, falloffPower));
                float damage = lightningDamage > 0f ? lightningDamage : (centerDamage * falloff);
                if (damage < 0.01f) continue;

                bool killed = eh.TakeDamage(damage, col, fromFlameSpray: false, fromSecondary: fromSecondary, tutorialWeaponIndex: tutorialWeaponIndex);
                areaDamageHitCount++;
                ApplyEnemyShake(eh.gameObject);
                if (lightningOnAreaTargets)
                    SpawnLightningFromAbove(closest);

                if (killed && lightningKillExplosion)
                    SpawnLightningKillExplosion(closest);

                if (hitEffectPerEnemy)
                    SpawnMinimalRoundHit(closest);
            }
            else
            {
                TargetBoardHealth board = col.GetComponentInParent<TargetBoardHealth>();
                if (board != null && board != primaryBoard)
                {
                    Vector3 closest = col.ClosestPoint(center);
                    float dist = Vector3.Distance(center, closest);
                    float t = Mathf.Clamp01(dist / radius);
                    float falloff = Mathf.Lerp(1f, minDamageRatio, Mathf.Pow(t, falloffPower));
                    float damage = centerDamage * falloff;
                    if (damage >= 0.01f)
                    {
                        board.TakeDamage(damage, closest);
                        areaDamageHitCount++;
                        if (lightningOnAreaTargets)
                            SpawnLightningFromAbove(closest);
                    }
                    if (hitEffectPerEnemy)
                        SpawnMinimalRoundHit(closest);
                }
            }
        }

        if (showAreaDamageText && areaDamageHitCount > 0)
            ShowAreaDamageTextAt(center);

        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.PlayHitSound();
        }
    }

    /// <summary>Elipsoid içindeki düşmanlara yıldırım hasarı verir (küre kapanırken).</summary>
    public void ApplyLightningDamageInEllipsoid(Vector3 center, float radiusX, float radiusY, float radiusZ, EnemyHealth primaryEnemy, TargetBoardHealth primaryBoard, int tutorialWeaponIndex, bool fromSecondary)
    {
        if (!lightningOnAreaTargets || (radiusX < 0.01f && radiusY < 0.01f && radiusZ < 0.01f)) return;

        Collider[] hits = Physics.OverlapSphere(center, Mathf.Max(radiusX, radiusY, radiusZ) * 2f);
        foreach (Collider col in hits)
        {
            EnemyHealth eh = col.GetComponentInParent<EnemyHealth>();
            if (eh == null || eh == primaryEnemy) continue;

            Vector3 closest = col.ClosestPoint(center);
            Vector3 p = closest - center;
            float nx = radiusX > 0.01f ? p.x / radiusX : 0f;
            float ny = radiusY > 0.01f ? p.y / radiusY : 0f;
            float nz = radiusZ > 0.01f ? p.z / radiusZ : 0f;
            if (nx * nx + ny * ny + nz * nz > 1f) continue;

            float damage = lightningDamage > 0f ? lightningDamage : centerDamage * minDamageRatio;
            if (damage < 0.01f) continue;

            bool killed = eh.TakeDamage(damage, col, fromFlameSpray: false, fromSecondary: fromSecondary, tutorialWeaponIndex: tutorialWeaponIndex);
            ApplyEnemyShake(eh.gameObject);
            SpawnLightningFromAbove(closest);
            if (killed && lightningKillExplosion)
                SpawnLightningKillExplosion(closest);
            if (hitEffectPerEnemy)
                SpawnMinimalRoundHit(closest);
        }
    }

    private class AreaDamageDelayedRunner : MonoBehaviour
    {
        public void Run(BulletAreaDamage source, Vector3 center, EnemyHealth primaryEnemy, TargetBoardHealth primaryBoard, int tutorialWeaponIndex, bool fromSecondary, float delay)
        {
            StartCoroutine(DelayedRoutine(source, center, primaryEnemy, primaryBoard, tutorialWeaponIndex, fromSecondary, delay));
        }

        private IEnumerator DelayedRoutine(BulletAreaDamage source, Vector3 center, EnemyHealth primaryEnemy, TargetBoardHealth primaryBoard, int tutorialWeaponIndex, bool fromSecondary, float delay)
        {
            yield return new WaitForSeconds(delay);
            source.ApplyAreaDamageInternal(center, primaryEnemy, primaryBoard, tutorialWeaponIndex, fromSecondary);
            Destroy(gameObject);
        }
    }

    /// <summary>Yuvarlak minimal hit: tek açılan halka.</summary>
    private void SpawnMinimalRoundHit(Vector3 position)
    {
        GameObject root = new GameObject("AreaHit_MinimalRing");
        root.transform.position = position;

        ParticleSystem ring = root.AddComponent<ParticleSystem>();
        var main = ring.main;
        main.startLifetime = 0.35f;
        main.startSpeed = 0f;
        main.startSize = 0.06f;
        main.startColor = new Color(1f, 0.85f, 0.5f, 0.75f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1;
        main.playOnAwake = true;

        var emission = ring.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

        var sizeOL = ring.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 4f)
        ));
        var colOL = ring.colorOverLifetime;
        colOL.enabled = true;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.9f, 0.6f), 0f), new GradientColorKey(new Color(1f, 0.6f, 0.2f), 1f) },
            new[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colOL.color = g;

        var rend = ring.GetComponent<ParticleSystemRenderer>();
        rend.material = CreateSoftParticleMaterial();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        ring.Play();
        Destroy(root, 0.5f);
    }

    /// <summary>2+3 Hibrit: Merkez patlama + parçacıklar + genişleyen/kapanan küre (cooldown ile).</summary>
    private void SpawnExplosionEffect(Vector3 position, float sphereHoldDuration, EnemyHealth primaryEnemy, TargetBoardHealth primaryBoard, int tutorialWeaponIndex, bool fromSecondary)
    {
        bool showSphere = sphereCooldownSeconds <= 0f || (Time.time - _lastSphereSpawnTime) >= sphereCooldownSeconds;
        if (showSphere)
            _lastSphereSpawnTime = Time.time;

        GameObject root = new GameObject("AreaDamage_Explosion");
        root.transform.position = position;

        CreateExplosionCoreFlash(root.transform);
        CreateExplosionParticles(root.transform);
        if (showSphere)
            CreateExpandingGroundRing(root.transform, sphereHoldDuration, position, primaryEnemy, primaryBoard, tutorialWeaponIndex, fromSecondary);

        float totalLife = 0.9f + sphereHoldDuration + 0.8f + 0.2f;
        Destroy(root, totalLife);
    }

    /// <summary>Option 3: Belirgin merkez flaşı - patlamanın kaynağı.</summary>
    private void CreateExplosionCoreFlash(Transform parent)
    {
        GameObject go = new GameObject("CoreFlash");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.28f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        main.startColor = new Color(1f, 1f, 0.95f, 1f);
        main.maxParticles = 35;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.duration = 0.1f;
        main.loop = false;

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 35) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.03f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g1 = new Gradient();
        g1.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.7f, 0.3f), 0.4f), new GradientColorKey(new Color(1f, 0.45f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.6f, 0.3f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = g1;
        SetupParticleRenderer(go);
    }

    private void CreateExplosionParticles(Transform parent)
    {
        GameObject go = new GameObject("ExplosionParticles");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.85f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 18f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.95f, 0.65f, 1f),
            new Color(1f, 0.55f, 0.2f, 1f)
        );
        main.maxParticles = 160;
        main.gravityModifier = -0.05f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.duration = 0.18f;
        main.loop = false;

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 120), new ParticleSystem.Burst(0.04f, 50) });
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.04f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1.3f), new Keyframe(0.25f, 1f), new Keyframe(0.6f, 0.5f), new Keyframe(1f, 0f)
        ));
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g2 = new Gradient();
        g2.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0f), new GradientColorKey(new Color(1f, 0.6f, 0.25f), 0.4f), new GradientColorKey(new Color(1f, 0.35f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.85f, 0.25f), new GradientAlphaKey(0.4f, 0.6f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = g2;
        SetupParticleRenderer(go);
    }

    /// <summary>Ortadan açılan 3D küre - açılır, bekler, yukarıdan aşağı kapanır. Kapanma sırasında içerdekiler yıldırım yer.</summary>
    private void CreateExpandingGroundRing(Transform parent, float holdDuration, Vector3 center, EnemyHealth primaryEnemy, TargetBoardHealth primaryBoard, int tutorialWeaponIndex, bool fromSecondary)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "AreaDamage_Sphere";
        sphere.transform.SetParent(parent);
        sphere.transform.localPosition = Vector3.zero;
        sphere.transform.localScale = Vector3.zero;
        Destroy(sphere.GetComponent<Collider>());

        Renderer rend = sphere.GetComponent<Renderer>();
        rend.material = CreateAreaSphereMaterialInstance();
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rend.receiveShadows = false;

        var anim = sphere.AddComponent<AreaSphereExpandAnim>();
        anim.targetDiameter = radius * 2f;
        anim.expandDuration = 0.9f;
        anim.holdDuration = holdDuration;
        anim.shrinkDuration = 0.6f;
        anim.source = this;
        anim.center = center;
        anim.primaryEnemy = primaryEnemy;
        anim.primaryBoard = primaryBoard;
        anim.tutorialWeaponIndex = tutorialWeaponIndex;
        anim.fromSecondary = fromSecondary;

        Destroy(sphere, 0.9f + holdDuration + 0.4f + 0.4f + 0.1f);
    }

    private static Texture2D _areaSphereTex;
    private static Material CreateAreaSphereMaterialInstance()
    {
        Shader sh = Shader.Find("Sprites/Default")
            ?? Shader.Find("Unlit/Color")
            ?? Shader.Find("Universal Render Pipeline/Unlit")
            ?? Shader.Find("Legacy Shaders/Transparent/Diffuse")
            ?? Shader.Find("Standard");
        if (sh == null) sh = Shader.Find("Standard");
        Material mat = new Material(sh ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default"));

        mat.color = new Color(0.98f, 0.82f, 0.45f, 0.68f);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", new Color(0.98f, 0.82f, 0.45f, 0.68f));
        mat.renderQueue = 3000;

        if (_areaSphereTex == null)
        {
            int size = 256;
            _areaSphereTex = new Texture2D(size, size);
            float cx = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / cx;
                    float dy = (y - cx) / cx;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a;
                    if (d <= 0.6f) a = 0.92f;
                    else if (d <= 0.82f) a = Mathf.Lerp(0.92f, 0.5f, (d - 0.6f) / 0.22f);
                    else if (d <= 0.95f) a = Mathf.Lerp(0.5f, 0.12f, (d - 0.82f) / 0.13f);
                    else a = Mathf.Lerp(0.12f, 0f, (d - 0.95f) / 0.05f);
                    float rim = d > 0.88f ? Mathf.Sin((d - 0.88f) * 40f) * 0.15f + 0.15f : 0f;
                    a = Mathf.Clamp01(a + rim);
                    _areaSphereTex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            _areaSphereTex.Apply();
            _areaSphereTex.filterMode = FilterMode.Bilinear;
        }
        if (mat.HasProperty("_MainTex"))
            mat.SetTexture("_MainTex", _areaSphereTex);
        else
            mat.mainTexture = _areaSphereTex;

        return mat;
    }

    private class AreaSphereExpandAnim : MonoBehaviour
    {
        public float targetDiameter = 5f;
        public float expandDuration = 0.9f;
        public float holdDuration = 1.65f;
        public float shrinkDuration = 0.6f;
        public BulletAreaDamage source;
        public Vector3 center;
        public EnemyHealth primaryEnemy;
        public TargetBoardHealth primaryBoard;
        public int tutorialWeaponIndex;
        public bool fromSecondary;
        private float _t;
        private float _lastLightningTick;

        private void Update()
        {
            _t += Time.deltaTime;
            float sx, sy, sz;
            float fade = 1f;
            float shrinkStart = expandDuration + holdDuration;
            float shrinkYDuration = shrinkDuration * 0.5f;
            float shrinkXZDuration = shrinkDuration * 0.5f;

            if (_t < expandDuration)
            {
                float p = _t / expandDuration;
                float eased = 1f - (1f - p) * (1f - p);
                float s = targetDiameter * eased;
                sx = sy = sz = s;
                fade = 1f - p * 0.3f;
            }
            else if (_t < shrinkStart)
            {
                sx = sy = sz = targetDiameter;
                fade = 0.95f;
            }
            else
            {
                float shrinkElapsed = _t - shrinkStart;
                if (shrinkElapsed < shrinkYDuration)
                {
                    float p = shrinkElapsed / shrinkYDuration;
                    float eased = 1f - p * p;
                    sx = sz = targetDiameter;
                    sy = targetDiameter * eased;
                    fade = 0.95f * (1f - p * 0.5f);
                }
                else
                {
                    float p = (shrinkElapsed - shrinkYDuration) / shrinkXZDuration;
                    float eased = 1f - p * p;
                    sx = sz = targetDiameter * eased;
                    sy = 0f;
                    fade = 0.5f * (1f - p);
                }

                if (source != null && source.lightningOnAreaTargets)
                {
                    if (_t - _lastLightningTick >= 0.2f)
                    {
                        _lastLightningTick = _t;
                        float r = 0.5f;
                        float rx = Mathf.Max(0.01f, sx * r);
                        float ry = Mathf.Max(0.01f, sy * r);
                        float rz = Mathf.Max(0.01f, sz * r);
                        source.ApplyLightningDamageInEllipsoid(center, rx, ry, rz, primaryEnemy, primaryBoard, tutorialWeaponIndex, fromSecondary);
                    }
                }
            }

            transform.localScale = new Vector3(sx, sy, sz);

            var mat = GetComponent<Renderer>()?.material;
            if (mat != null && !mat.shader.name.Contains("InternalError"))
            {
                Color c = new Color(0.98f, 0.78f, 0.4f, 0.65f * fade);
                mat.color = c;
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            }
        }
    }

    private class AreaDamageShake : MonoBehaviour
    {
        public float duration = 0.25f;
        public float intensity = 0.04f;
        private Vector3 _originalPos;
        private float _t;

        private void Start()
        {
            _originalPos = transform.position;
        }

        private void Update()
        {
            _t += Time.deltaTime;
            if (_t >= duration)
            {
                transform.position = _originalPos;
                Destroy(this);
                return;
            }
            float falloff = 1f - (_t / duration);
            float x = (Mathf.PerlinNoise(_t * 40f, 0f) - 0.5f) * 2f * intensity * falloff;
            float z = (Mathf.PerlinNoise(0f, _t * 40f) - 0.5f) * 2f * intensity * falloff;
            transform.position = _originalPos + new Vector3(x, 0f, z);
        }
    }

    private class LightningBoltAnim : MonoBehaviour
    {
        public Vector3 from, to;
        public LineRenderer glowLineRenderer;
        public LineRenderer coreLineRenderer;
        public float initialGlowWidth = 0.38f;
        public float initialCoreWidth = 0.12f;
        public int segments = 32;
        public float jaggedAmount = 0.16f;
        public float duration = 0.75f;
        private float _t;
        private int _seed;

        private void Start() { _seed = Random.Range(0, 10000); }

        private void Update()
        {
            _t += Time.deltaTime;
            if (_t >= duration) { Destroy(gameObject); return; }

            Vector3[] pts = GenerateSmoothLightningPoints(from, to, segments, jaggedAmount, _seed + (int)(_t * 60f));

            if (glowLineRenderer != null)
            {
                glowLineRenderer.positionCount = pts.Length;
                glowLineRenderer.SetPositions(pts);
            }
            if (coreLineRenderer != null)
            {
                coreLineRenderer.positionCount = pts.Length;
                coreLineRenderer.SetPositions(pts);
            }

            // FifthGunLaser tarzı: fade = width azalması
            float fade = 1f - (_t / duration) * (_t / duration);
            float widthMult = Mathf.SmoothStep(1f, 0f, 1f - fade);
            if (glowLineRenderer != null)
                glowLineRenderer.widthMultiplier = initialGlowWidth * widthMult;
            if (coreLineRenderer != null)
                coreLineRenderer.widthMultiplier = initialCoreWidth * widthMult;
        }
    }

    /// <summary>FifthGunLaser tarzı organik zigzag - Perlin + edge falloff.</summary>
    private static Vector3[] GenerateSmoothLightningPoints(Vector3 start, Vector3 end, int segmentCount, float jagged, int seed)
    {
        var pts = new Vector3[segmentCount + 1];
        pts[0] = start;
        pts[segmentCount] = end;

        Vector3 dir = (end - start).normalized;
        Vector3 perp1 = Vector3.Cross(dir, Vector3.up).normalized;
        if (perp1.sqrMagnitude < 0.01f) perp1 = Vector3.Cross(dir, Vector3.right).normalized;
        Vector3 perp2 = Vector3.Cross(dir, perp1).normalized;

        var oldState = Random.state;
        Random.InitState(seed);
        for (int i = 1; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            Vector3 basePt = Vector3.Lerp(start, end, t);
            float edgeFalloff = Mathf.Sin(t * Mathf.PI);
            edgeFalloff = Mathf.Pow(edgeFalloff, 0.5f);

            float nx = (Mathf.PerlinNoise(i * 0.3f + seed * 0.01f, 0f) - 0.5f) * 2f;
            float ny = (Mathf.PerlinNoise(0f, i * 0.3f + seed * 0.01f) - 0.5f) * 2f;
            nx = Mathf.Lerp(nx, Random.Range(-1f, 1f), 0.45f);
            ny = Mathf.Lerp(ny, Random.Range(-1f, 1f), 0.45f);

            float ox = nx * jagged * edgeFalloff;
            float oy = ny * jagged * edgeFalloff;
            pts[i] = basePt + perp1 * ox + perp2 * oy;
        }
        Random.state = oldState;
        return pts;
    }

    private static void SetupParticleRenderer(GameObject go)
    {
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.material = CreateSoftParticleMaterial();
        r.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private static Material _softMat;
    private static Material CreateSoftParticleMaterial()
    {
        if (_softMat != null) return _softMat;
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float center = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt(((x - center) / center) * ((x - center) / center) + ((y - center) / center) * ((y - center) / center));
                float a = Mathf.Clamp01(1f - d); a *= a;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Particles/Additive") ?? Shader.Find("Sprites/Default");
        _softMat = new Material(sh != null ? sh : Shader.Find("Sprites/Default"));
        _softMat.mainTexture = tex;
        _softMat.SetInt("_ZWrite", 0);
        return _softMat;
    }

    private void ShowAreaDamageTextAt(Vector3 position)
    {
        GameObject go = new GameObject("AreaDamage_Text");
        go.transform.position = position + Vector3.up * 0.3f;
        TextMeshPro tmp = go.AddComponent<TextMeshPro>();
        tmp.text = "AREA DAMAGE";
        tmp.fontSize = 3f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.9f, 0.5f, 1f);
        tmp.sortingOrder = 30;
        var anim = go.AddComponent<AreaDamageTextAnim>();
        anim.floatDistance = 1.2f;
        anim.duration = 1.4f;
    }

    private class AreaDamageTextAnim : MonoBehaviour
    {
        public float floatDistance = 1.2f;
        public float duration = 1.4f;
        private Vector3 _start;
        private float _t;

        private void Start() { _start = transform.position; }

        private void Update()
        {
            _t += Time.deltaTime;
            float p = Mathf.Clamp01(_t / duration);
            transform.position = _start + Vector3.up * (floatDistance * p);
            var tmp = GetComponent<TextMeshPro>();
            if (tmp != null)
                tmp.color = new Color(1f, 0.9f, 0.5f, 1f - p);
            if (Camera.main != null)
            {
                Vector3 dir = Camera.main.transform.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(-dir);
            }
            if (_t >= duration) Destroy(gameObject);
        }
    }
}
