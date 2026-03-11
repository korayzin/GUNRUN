using UnityEngine;

/// <summary>
/// ScriptableObject holding all game balance data from the design table.
/// Per-weapon and per-enemy (per-stage) parameters; single source of truth.
/// </summary>
[CreateAssetMenu(fileName = "GameBalance", menuName = "GUNRUN/Game Balance", order = 0)]
public class GameBalance : ScriptableObject
{
    [Header("Weapon unlock (kill count to unlock next weapon)")]
    [Tooltip("Index 0 = kill to unlock SecondGun(4), 1 = ThirdGun(12), 2 = Fourth(20), 3 = Fifth(32), 4 = Sixth(48), 5 = Seventh(68), 6 = Eight(82), 7 = LastGun(102). FirstGun = 0.")]
    public int[] killToUnlockPerWeapon = new int[] { 4, 12, 20, 32, 48, 68, 82, 102 };

    [Header("Weapon stats (index 0 = FirstGun, 8 = LastGun)")]
    public WeaponBalanceData[] weapons = new WeaponBalanceData[9];

    [Header("Stage thresholds")]
    [Tooltip("Use score for stage: Stage 2 when score >= this")]
    public int stage2ScoreThreshold = 1500;
    [Tooltip("Stage 3 when score >= this")]
    public int stage3ScoreThreshold = 8000;
    [Tooltip("Alternative: use kill count. Stage 2 when kill >= this")]
    public int stage2KillThreshold = 36;
    [Tooltip("Stage 3 when kill >= this")]
    public int stage3KillThreshold = 90;
    [Tooltip("If true, stage is determined by score; else by kill count")]
    public bool useScoreForStage = false;

    [Header("Spawn interval per stage (seconds)")]
    public float stage1SpawnInterval = 2.5f;
    public float stage2SpawnInterval = 2.0f;
    public float stage3SpawnInterval = 1.4f;

    [Header("Enemy speed per stage")]
    public float stage1EnemySpeed = 3.0f;
    public float stage2EnemySpeed = 3.5f;
    public float stage3EnemySpeed = 4.0f;

    [Header("Enemy HP per stage (Weak/Medium/Strong/Tank)")]
    public EnemyStageRow[] enemyHPByStage = new EnemyStageRow[3];
    [Header("Enemy score value per stage (Weak/Medium/Strong/Tank)")]
    public EnemyStageRowInt[] enemyScoreByStage = new EnemyStageRowInt[3];
    [Header("Enemy spawn weights per stage (percent: Weak, Medium, Strong, Tank)")]
    public EnemyStageRowInt[] enemySpawnWeights = new EnemyStageRowInt[3];

    [Header("Portal bias (A, B, C weights - e.g. 33,33,33 or 50,25,25)")]
    public Vector3 portalWeights = new Vector3(33f, 33f, 33f);

    [Header("Bag system")]
    public int bagSize = 12;
    public int bagRefillWhenSlotsBelow = 1;

    private void OnValidate()
    {
        if (killToUnlockPerWeapon != null && killToUnlockPerWeapon.Length != 8)
            System.Array.Resize(ref killToUnlockPerWeapon, 8);
        if (weapons != null && weapons.Length != 9)
            System.Array.Resize(ref weapons, 9);
        if (enemyHPByStage != null && enemyHPByStage.Length != 3)
            System.Array.Resize(ref enemyHPByStage, 3);
        if (enemyScoreByStage != null && enemyScoreByStage.Length != 3)
            System.Array.Resize(ref enemyScoreByStage, 3);
        if (enemySpawnWeights != null && enemySpawnWeights.Length != 3)
            System.Array.Resize(ref enemySpawnWeights, 3);
    }

    [System.Serializable]
    public struct WeaponBalanceData
    {
        public string weaponName;
        [Tooltip("Bullet damage (trigger hit)")]
        public float damage;
        [Tooltip("Bullet velocity/speed")]
        public float bulletVelocity;
        public int maxAmmo;
        [Tooltip("Fire cooldown in seconds")]
        public float fireCooldown;
        [Tooltip("Secondary damage (e.g. FifthGun lightning). 0 = use script default")]
        public float secondaryDamage;
        [Tooltip("Secondary max ammo/shots (e.g. FifthGun lightning count). 0 = use script default")]
        public int secondaryMaxAmmo;
    }

    [System.Serializable]
    public struct EnemyStageRow
    {
        public float weak;   // tur1
        public float medium; // tur2
        public float strong; // tur3
        public float tank;   // tur4
    }

    [System.Serializable]
    public struct EnemyStageRowInt
    {
        public int weak;
        public int medium;
        public int strong;
        public int tank;
    }
}
