using UnityEngine;

/// <summary>
/// Birinci silah mermisi düşmana değince shotgun tarzı, boya sıçraması hissi veren, yuvarlak ve stilize hit efekti spawn eder.
/// Tamamen kod ile üretilir (prefab yok). FirstGun_Bullet prefab'ına ekle; Bullet.cs düşmana çarptığında SpawnHitEffect çağırır.
/// </summary>
public class FirstGunBulletHitEffect : MonoBehaviour
{
    [Tooltip("Düşmana değince hit efekti aktif mi?")]
    public bool enableHitEffect = true;

    [Header("Boya Sıçraması (Shotgun Tarzı)")]
    [Tooltip("Ana sıçrama parçacık sayısı")]
    [Range(30, 90)]
    public int splatterCount = 55;

    [Tooltip("Büyük blob parçacık sayısı (yuvarlak lekeler)")]
    [Range(8, 25)]
    public int blobCount = 14;

    [Tooltip("Parçacık hızı min-max (boya fışkırması)")]
    public Vector2 splatterSpeed = new Vector2(3.5f, 7f);

    [Tooltip("Parçacık boyutu min-max")]
    public Vector2 splatterSize = new Vector2(0.12f, 0.28f);

    [Tooltip("Blob boyutu min-max (daha büyük yuvarlak lekeler)")]
    public Vector2 blobSize = new Vector2(0.2f, 0.38f);

    [Tooltip("Yumuşak halka başlangıç boyutu")]
    public float ringStartSize = 0.08f;

    [Tooltip("Halka max boyut (açılma)")]
    public float ringMaxSize = 0.55f;

    [Header("Renk Paleti (Boya / Stilize)")]
    [Tooltip("Ana sıcak ton")]
    public Color paintMain = new Color(1f, 0.72f, 0.5f, 0.95f);

    [Tooltip("Açık / kremsi ton")]
    public Color paintLight = new Color(1f, 0.92f, 0.82f, 0.9f);

    [Tooltip("Koyu / yoğun boya tonu")]
    public Color paintDark = new Color(1f, 0.55f, 0.35f, 0.85f);

    [Tooltip("Efekt nesnesinin yok edilme süresi (sn)")]
    [Range(0.5f, 1.5f)]
    public float effectDuration = 0.95f;

    /// <summary>Bullet.cs düşmana çarptığında bu metodu çağırır.</summary>
    public void SpawnHitEffect(Vector3 position)
    {
        if (!enableHitEffect) return;

        GameObject root = new GameObject("FirstGun_PaintHitEffect");
        root.transform.position = position;

        Material softMat = CreatePaintBlobMaterial();

        // 1) Ana sıçrama patlaması – shotgun yoğunluğunda, her yöne yuvarlak saçılma
        CreateSplatterBurst(root.transform, softMat);

        // 2) Büyük yuvarlak blob’lar – boya lekeleri, daha yavaş yayılma
        CreateBlobLayer(root.transform, softMat);

        // 3) Yumuşak açılan halka – boya halkası
        CreatePaintRing(root.transform, softMat);

        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
            ps.Play();
        Destroy(root, effectDuration);
    }

    /// <summary>Shotgun tarzı ana sıçrama – yuvarlak, her yöne saçılma.</summary>
    private void CreateSplatterBurst(Transform parent, Material mat)
    {
        GameObject go = new GameObject("SplatterBurst");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.08f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(splatterSpeed.x, splatterSpeed.y);
        main.startSize = new ParticleSystem.MinMaxCurve(splatterSize.x, splatterSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(paintLight, paintMain);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = splatterCount;
        main.gravityModifier = 0.15f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, splatterCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.03f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.35f, 1.05f);
        sizeCurve.AddKey(0.7f, 0.7f);
        sizeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var colGrad = new Gradient();
        colGrad.SetKeys(
            new[] {
                new GradientColorKey(paintLight, 0f),
                new GradientColorKey(paintMain, 0.4f),
                new GradientColorKey(paintDark, 0.85f),
                new GradientColorKey(paintDark, 1f)
            },
            new[] {
                new GradientAlphaKey(0.95f, 0f),
                new GradientAlphaKey(0.9f, 0.4f),
                new GradientAlphaKey(0.6f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = colGrad;

        var velOL = ps.velocityOverLifetime;
        velOL.enabled = true;
        velOL.orbitalX = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
        velOL.orbitalY = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
        velOL.radial = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }

    /// <summary>Büyük yuvarlak blob’lar – boya lekeleri, soft ve yuvarlak.</summary>
    private void CreateBlobLayer(Transform parent, Material mat)
    {
        GameObject go = new GameObject("BlobLayer");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.06f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.75f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 2.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(blobSize.x, blobSize.y);
        main.startColor = new ParticleSystem.MinMaxGradient(paintLight, paintMain);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = blobCount;
        main.gravityModifier = -0.05f;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0.02f, blobCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.06f;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var blobSizeCurve = new AnimationCurve();
        blobSizeCurve.AddKey(0f, 0.6f);
        blobSizeCurve.AddKey(0.2f, 1.1f);
        blobSizeCurve.AddKey(0.6f, 1f);
        blobSizeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, blobSizeCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var blobGrad = new Gradient();
        blobGrad.SetKeys(
            new[] {
                new GradientColorKey(paintLight, 0f),
                new GradientColorKey(paintMain, 0.5f),
                new GradientColorKey(paintDark, 1f)
            },
            new[] {
                new GradientAlphaKey(0.9f, 0f),
                new GradientAlphaKey(0.85f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = blobGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }

    /// <summary>Yumuşak boya halkası – round ve stilize.</summary>
    private void CreatePaintRing(Transform parent, Material mat)
    {
        GameObject go = new GameObject("PaintRing");
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 0f;
        main.startSize = ringStartSize;
        main.startColor = new Color(paintLight.r, paintLight.g, paintLight.b, 0.7f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        var ringCurve = new AnimationCurve();
        ringCurve.AddKey(0f, 1f);
        ringCurve.AddKey(0.15f, 1.2f);
        ringCurve.AddKey(1f, ringMaxSize / ringStartSize);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, ringCurve);

        var colorOL = ps.colorOverLifetime;
        colorOL.enabled = true;
        var ringGrad = new Gradient();
        ringGrad.SetKeys(
            new[] {
                new GradientColorKey(paintLight, 0f),
                new GradientColorKey(paintMain, 0.5f),
                new GradientColorKey(paintDark, 1f)
            },
            new[] {
                new GradientAlphaKey(0.6f, 0f),
                new GradientAlphaKey(0.5f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOL.color = ringGrad;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.material = mat;
        rend.renderMode = ParticleSystemRenderMode.Billboard;
    }

    private static Material _paintBlobMat;

    /// <summary>Yumuşak, organik boya blob tekstürü – yuvarlak ama hafif irregular edge.</summary>
    private static Material CreatePaintBlobMaterial()
    {
        if (_paintBlobMat != null) return _paintBlobMat;

        int size = 128;
        Texture2D tex = new Texture2D(size, size);
        float cx = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cx) / cx;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float hash = ((x * 7 + y * 13) % 100) / 100f - 0.5f;
                float noise = hash * 0.14f;
                float edge = 0.86f + noise;
                float alpha;
                if (d <= 0.5f)
                    alpha = 0.98f;
                else if (d <= edge)
                    alpha = Mathf.Lerp(0.98f, 0.1f, (d - 0.5f) / (edge - 0.5f));
                else
                    alpha = Mathf.Lerp(0.1f, 0f, (d - edge) / (1f - edge));
                alpha = Mathf.Clamp01(alpha);
                alpha = alpha * alpha;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        Shader sh = Shader.Find("Legacy Shaders/Particles/Additive")
            ?? Shader.Find("Particles/Additive")
            ?? Shader.Find("Sprites/Default");
        _paintBlobMat = new Material(sh ?? Shader.Find("Sprites/Default"));
        _paintBlobMat.mainTexture = tex;
        _paintBlobMat.SetInt("_ZWrite", 0);
        return _paintBlobMat;
    }
}
