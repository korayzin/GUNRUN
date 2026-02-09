#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CreateDefaultGameBalance
{
    [MenuItem("GUNRUN/Create Default Game Balance Asset")]
    static void Create()
    {
        var balance = ScriptableObject.CreateInstance<GameBalance>();
        balance.killToUnlockPerWeapon = new int[] { 4, 12, 20, 32, 48, 68, 82, 102 };
        balance.weapons = new GameBalance.WeaponBalanceData[9];
        balance.weapons[0] = new GameBalance.WeaponBalanceData { weaponName = "FirstGun", damage = 25, bulletVelocity = 10, maxAmmo = 30, fireCooldown = 0.5f };
        balance.weapons[1] = new GameBalance.WeaponBalanceData { weaponName = "SecondGun", damage = 30, bulletVelocity = 200, maxAmmo = 30, fireCooldown = 0.1f };
        balance.weapons[2] = new GameBalance.WeaponBalanceData { weaponName = "ThirdGun", damage = 25, bulletVelocity = 100, maxAmmo = 10, fireCooldown = 0.5f };
        balance.weapons[3] = new GameBalance.WeaponBalanceData { weaponName = "FourthGun", damage = 50, bulletVelocity = 80, maxAmmo = 100, fireCooldown = 0.05f };
        balance.weapons[4] = new GameBalance.WeaponBalanceData { weaponName = "FifthGun", damage = 50, bulletVelocity = 20, maxAmmo = 60, fireCooldown = 0.01f };
        balance.weapons[5] = new GameBalance.WeaponBalanceData { weaponName = "SixthGun", damage = 25, bulletVelocity = 15, maxAmmo = 25, fireCooldown = 0.5f };
        balance.weapons[6] = new GameBalance.WeaponBalanceData { weaponName = "SeventhGun", damage = 5, bulletVelocity = 0, maxAmmo = 25, fireCooldown = 0.5f };
        balance.weapons[7] = new GameBalance.WeaponBalanceData { weaponName = "EightGun", damage = 50, bulletVelocity = 20, maxAmmo = 20, fireCooldown = 0.5f };
        balance.weapons[8] = new GameBalance.WeaponBalanceData { weaponName = "LastGun", damage = 12, bulletVelocity = 10, maxAmmo = 100, fireCooldown = 0.08f };

        balance.stage2ScoreThreshold = 1500;
        balance.stage3ScoreThreshold = 8000;
        balance.stage2KillThreshold = 36;
        balance.stage3KillThreshold = 90;
        balance.useScoreForStage = false;
        balance.stage1SpawnInterval = 2.5f;
        balance.stage2SpawnInterval = 2.0f;
        balance.stage3SpawnInterval = 1.4f;
        balance.stage1EnemySpeed = 3f;
        balance.stage2EnemySpeed = 3.5f;
        balance.stage3EnemySpeed = 4f;

        balance.enemyHPByStage = new GameBalance.EnemyStageRow[3];
        balance.enemyHPByStage[0] = new GameBalance.EnemyStageRow { weak = 100, medium = 150, strong = 200, tank = 0 };
        balance.enemyHPByStage[1] = new GameBalance.EnemyStageRow { weak = 150, medium = 250, strong = 350, tank = 400 };
        balance.enemyHPByStage[2] = new GameBalance.EnemyStageRow { weak = 250, medium = 350, strong = 500, tank = 750 };

        balance.enemyScoreByStage = new GameBalance.EnemyStageRowInt[3];
        balance.enemyScoreByStage[0] = new GameBalance.EnemyStageRowInt { weak = 25, medium = 50, strong = 100, tank = 150 };
        balance.enemyScoreByStage[1] = new GameBalance.EnemyStageRowInt { weak = 25, medium = 50, strong = 100, tank = 150 };
        balance.enemyScoreByStage[2] = new GameBalance.EnemyStageRowInt { weak = 25, medium = 50, strong = 100, tank = 150 };

        balance.enemySpawnWeights = new GameBalance.EnemyStageRowInt[3];
        balance.enemySpawnWeights[0] = new GameBalance.EnemyStageRowInt { weak = 70, medium = 25, strong = 5, tank = 0 };
        balance.enemySpawnWeights[1] = new GameBalance.EnemyStageRowInt { weak = 50, medium = 30, strong = 15, tank = 5 };
        balance.enemySpawnWeights[2] = new GameBalance.EnemyStageRowInt { weak = 10, medium = 20, strong = 45, tank = 25 };

        balance.portalWeights = new Vector3(33f, 33f, 33f);
        balance.bagSize = 12;
        balance.bagRefillWhenSlotsBelow = 1;

        string path = "Assets/Resources/GameBalance.asset";
        var dir = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);
        AssetDatabase.CreateAsset(balance, path);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created {path}. Assign it to GameBalanceManager in the scene.");
    }
}
#endif
