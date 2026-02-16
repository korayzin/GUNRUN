using UnityEngine;

/// <summary>
/// Singleton that provides balance data by weapon index and (stage, enemy type).
/// Each weapon and each enemy gets its own row/cell from the balance table.
/// </summary>
public class GameBalanceManager : MonoBehaviour
{
    public static GameBalanceManager Instance { get; private set; }

    [SerializeField] [Tooltip("Assign GameBalance asset. If null, loads from Resources/GameBalance or uses table defaults.")]
    private GameBalance balance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (balance == null)
            balance = Resources.Load<GameBalance>("GameBalance");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>Kill count required to unlock weapon at index (1..8). Weapon 0 is always unlocked.</summary>
    public int GetKillToUnlock(int weaponIndex)
    {
        if (balance != null && balance.killToUnlockPerWeapon != null && weaponIndex >= 1 && weaponIndex <= 8)
            return balance.killToUnlockPerWeapon[weaponIndex - 1];
        int[] defaults = { 4, 12, 20, 32, 48, 68, 82, 102 }; // SecondGun..LastGun
        return weaponIndex >= 1 && weaponIndex <= 8 ? defaults[weaponIndex - 1] : 0;
    }

    /// <summary>Weapon balance for index 0..8 (FirstGun..LastGun). Returns per-weapon data.</summary>
    public GameBalance.WeaponBalanceData GetWeaponData(int weaponIndex)
    {
        if (balance != null && balance.weapons != null && weaponIndex >= 0 && weaponIndex < balance.weapons.Length)
            return balance.weapons[weaponIndex];
        return GetDefaultWeaponData(weaponIndex);
    }

    private static GameBalance.WeaponBalanceData GetDefaultWeaponData(int weaponIndex)
    {
        var d = new GameBalance.WeaponBalanceData { weaponName = "Weapon", damage = 25, bulletVelocity = 50, maxAmmo = 30, fireCooldown = 0.5f };
        switch (weaponIndex)
        {
            case 0: d.weaponName = "FirstGun";  d.damage = 25;  d.bulletVelocity = 10;  d.maxAmmo = 30;  d.fireCooldown = 0.5f; break;
            case 1: d.weaponName = "SecondGun"; d.damage = 30;  d.bulletVelocity = 200; d.maxAmmo = 30;  d.fireCooldown = 0.1f; break;
            case 2: d.weaponName = "ThirdGun";  d.damage = 25;  d.bulletVelocity = 100; d.maxAmmo = 10;  d.fireCooldown = 0.5f; break;
            case 3: d.weaponName = "FourthGun"; d.damage = 50;  d.bulletVelocity = 80;  d.maxAmmo = 100; d.fireCooldown = 0.05f; break;
            case 4: d.weaponName = "FifthGun";  d.damage = 50;  d.bulletVelocity = 20;  d.maxAmmo = 60;  d.fireCooldown = 0.01f; break;
            case 5: d.weaponName = "SixthGun";  d.damage = 25;  d.bulletVelocity = 15;  d.maxAmmo = 25;  d.fireCooldown = 0.5f; break;
            case 6: d.weaponName = "SeventhGun"; d.damage = 5;   d.bulletVelocity = 0;   d.maxAmmo = 25;  d.fireCooldown = 0.5f; break;
            case 7: d.weaponName = "EightGun";  d.damage = 50;  d.bulletVelocity = 20;  d.maxAmmo = 20;  d.fireCooldown = 0.5f; break;
            case 8: d.weaponName = "LastGun";  d.damage = 12;  d.bulletVelocity = 10;  d.maxAmmo = 100; d.fireCooldown = 0.08f; break;
        }
        return d;
    }

    /// <summary>Current stage (1, 2, or 3) by score if useScoreForStage, else by kill count.</summary>
    public int GetStageFromScore(int score)
    {
        if (balance == null) return score >= 8000 ? 3 : (score >= 1500 ? 2 : 1);
        if (score >= balance.stage3ScoreThreshold) return 3;
        if (score >= balance.stage2ScoreThreshold) return 2;
        return 1;
    }

    /// <summary>Current stage (1, 2, or 3) by kill count.</summary>
    public int GetStageFromKill(int killCount)
    {
        if (balance == null) return killCount >= 90 ? 3 : (killCount >= 36 ? 2 : 1);
        if (killCount >= balance.stage3KillThreshold) return 3;
        if (killCount >= balance.stage2KillThreshold) return 2;
        return 1;
    }

    public bool UseScoreForStage => balance != null && balance.useScoreForStage;

    public float GetSpawnInterval(int stage)
    {
        if (balance == null) return stage == 1 ? 2.5f : (stage == 2 ? 2f : 1.4f);
        return stage == 1 ? balance.stage1SpawnInterval : (stage == 2 ? balance.stage2SpawnInterval : balance.stage3SpawnInterval);
    }

    public float GetEnemySpeed(int stage)
    {
        if (balance == null) return stage == 1 ? 3f : (stage == 2 ? 3.5f : 4f);
        return stage == 1 ? balance.stage1EnemySpeed : (stage == 2 ? balance.stage2EnemySpeed : balance.stage3EnemySpeed);
    }

    /// <summary>Enemy type: 0=Weak(tur1), 1=Medium(tur2), 2=Strong(tur3), 3=Tank(tur4).</summary>
    public float GetEnemyHP(int stage, int enemyType)
    {
        if (balance != null && balance.enemyHPByStage != null && stage >= 1 && stage <= 3)
        {
            var row = balance.enemyHPByStage[stage - 1];
            switch (enemyType) { case 0: return row.weak; case 1: return row.medium; case 2: return row.strong; case 3: return row.tank; }
        }
        return GetDefaultEnemyHP(stage, enemyType);
    }

    private static float GetDefaultEnemyHP(int stage, int enemyType)
    {
        if (stage == 1) { float[] h = { 100, 150, 200, 0 }; return h[Mathf.Clamp(enemyType, 0, 3)]; }
        if (stage == 2) { float[] h = { 150, 250, 350, 400 }; return h[Mathf.Clamp(enemyType, 0, 3)]; }
        float[] h3 = { 250, 350, 500, 750 }; return h3[Mathf.Clamp(enemyType, 0, 3)];
    }

    /// <summary>Enemy type: 0=Weak, 1=Medium, 2=Strong, 3=Tank.</summary>
    public int GetEnemyScore(int stage, int enemyType)
    {
        if (balance != null && balance.enemyScoreByStage != null && stage >= 1 && stage <= 3)
        {
            var row = balance.enemyScoreByStage[stage - 1];
            switch (enemyType) { case 0: return row.weak; case 1: return row.medium; case 2: return row.strong; case 3: return row.tank; }
        }
        int[] scores = { 25, 50, 100, 150 };
        return scores[Mathf.Clamp(enemyType, 0, 3)];
    }

    /// <summary>Spawn weight (percent) for each enemy type in this stage. Returns (weak, medium, strong, tank).</summary>
    public void GetEnemySpawnWeights(int stage, out int weak, out int medium, out int strong, out int tank)
    {
        if (balance != null && balance.enemySpawnWeights != null && stage >= 1 && stage <= 3)
        {
            var row = balance.enemySpawnWeights[stage - 1];
            weak = row.weak; medium = row.medium; strong = row.strong; tank = row.tank;
            return;
        }
        if (stage == 1) { weak = 70; medium = 25; strong = 5; tank = 0; return; }
        if (stage == 2) { weak = 50; medium = 30; strong = 15; tank = 5; return; }
        weak = 10; medium = 20; strong = 45; tank = 25;
    }

    /// <summary>Portal A/B/C weights (e.g. 33,33,33). Use for weighted random portal choice.</summary>
    public Vector3 GetPortalWeights()
    {
        return balance != null ? balance.portalWeights : new Vector3(33f, 33f, 33f);
    }

    public int BagSize => balance != null ? balance.bagSize : 12;
    public int BagRefillWhenSlotsBelow => balance != null ? balance.bagRefillWhenSlotsBelow : 1;
}
