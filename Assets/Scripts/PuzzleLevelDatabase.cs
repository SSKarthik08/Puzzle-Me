using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PuzzleLevelDatabase",
    menuName = "Puzzle/Level Database"
)]
public class PuzzleLevelDatabase : ScriptableObject
{
    [Serializable]
    public class DifficultyTier
    {
        [Tooltip("First level number in this tier (inclusive).")]
        public int startLevel = 1;

        [Tooltip("Last level number in this tier (inclusive).")]
        public int endLevel = 5;

        [Tooltip("Grid size for this tier, e.g. 3 = 3x3, 4 = 4x4.")]
        public int gridSize = 3;

        [Tooltip("Optional label shown in UI, e.g. Easy / Medium / Hard.")]
        public string tierName = "Easy";

        [Tooltip("How many random swaps to shuffle with for this tier.")]
        public int shuffleMoves = 30;
    }


    [Header("DIFFICULTY TIERS")]
    [SerializeField]
    private DifficultyTier[] tiers = new DifficultyTier[]
    {
        new DifficultyTier
        {
            startLevel = 1,
            endLevel = 10,
            gridSize = 3,
            tierName = "Easy",
            shuffleMoves = 30
        },

        new DifficultyTier
        {
            startLevel = 11,
            endLevel = 20,
            gridSize = 4,
            tierName = "Medium",
            shuffleMoves = 60
        },
    };


    [Serializable]
    public class TimeLimitTier
    {
        [Tooltip("First level number in this tier (inclusive).")]
        public int startLevel = 1;

        [Tooltip("Last level number in this tier (inclusive).")]
        public int endLevel = 5;

        [Tooltip("Time limit for this tier, in seconds (e.g. 420 = 7:00).")]
        public int timeLimitSeconds = 420;
    }


    [Header("TIME LIMIT TIERS")]
    [Tooltip("Independent of the grid-size tiers above — lets the timer change on a different level breakdown than the grid size does.")]
    [SerializeField]
    private TimeLimitTier[] timeLimitTiers = new TimeLimitTier[]
    {
        new TimeLimitTier { startLevel = 1,  endLevel = 5,  timeLimitSeconds = 420 }, // 7:00
        new TimeLimitTier { startLevel = 6,  endLevel = 10, timeLimitSeconds = 360 }, // 6:00
        new TimeLimitTier { startLevel = 11, endLevel = 15, timeLimitSeconds = 300 }, // 5:00
        new TimeLimitTier { startLevel = 16, endLevel = 18, timeLimitSeconds = 240 }, // 4:00
        new TimeLimitTier { startLevel = 19, endLevel = 20, timeLimitSeconds = 180 }, // 3:00
    };


    // =========================================================
    // GET TIME LIMIT FOR LEVEL
    // =========================================================

    public int GetTimeLimitForLevel(int levelNumber)
    {
        for (int i = 0; i < timeLimitTiers.Length; i++)
        {
            if (levelNumber >= timeLimitTiers[i].startLevel &&
                levelNumber <= timeLimitTiers[i].endLevel)
            {
                return timeLimitTiers[i].timeLimitSeconds;
            }
        }


        // Past every defined tier — fall back to the last one.
        if (timeLimitTiers.Length > 0)
        {
            return timeLimitTiers[timeLimitTiers.Length - 1].timeLimitSeconds;
        }


        return 300;
    }


    public int TierCount => tiers.Length;


    // =========================================================
    // RESET TO SCRIPT DEFAULTS
    // ScriptableObject assets keep their OWN saved values once
    // created — editing the defaults above in code does NOT
    // retroactively update an existing asset. Right-click the
    // component header in the Inspector (or click the ⋮ menu)
    // and choose this whenever the defaults change, instead of
    // editing each tier field by hand.
    // =========================================================

    [ContextMenu("Reset Tiers To Script Defaults")]
    private void ResetTiersToDefaults()
    {
        tiers = new DifficultyTier[]
        {
            new DifficultyTier
            {
                startLevel = 1,
                endLevel = 10,
                gridSize = 3,
                tierName = "Easy",
                shuffleMoves = 30
            },

            new DifficultyTier
            {
                startLevel = 11,
                endLevel = 20,
                gridSize = 4,
                tierName = "Medium",
                shuffleMoves = 60
            },
        };

        timeLimitTiers = new TimeLimitTier[]
        {
            new TimeLimitTier { startLevel = 1,  endLevel = 5,  timeLimitSeconds = 420 },
            new TimeLimitTier { startLevel = 6,  endLevel = 10, timeLimitSeconds = 360 },
            new TimeLimitTier { startLevel = 11, endLevel = 15, timeLimitSeconds = 300 },
            new TimeLimitTier { startLevel = 16, endLevel = 18, timeLimitSeconds = 240 },
            new TimeLimitTier { startLevel = 19, endLevel = 20, timeLimitSeconds = 180 },
        };

        Debug.Log(
            "TIERS RESET TO SCRIPT DEFAULTS"
        );

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }


    // Highest level number covered by any configured tier.
    public int MaxDefinedLevel
    {
        get
        {
            int max = 0;

            for (int i = 0; i < tiers.Length; i++)
            {
                if (tiers[i].endLevel > max)
                {
                    max = tiers[i].endLevel;
                }
            }

            return max;
        }
    }


    // =========================================================
    // GET TIER FOR LEVEL
    // =========================================================

    public DifficultyTier GetTierForLevel(int levelNumber)
    {
        for (int i = 0; i < tiers.Length; i++)
        {
            if (levelNumber >= tiers[i].startLevel &&
                levelNumber <= tiers[i].endLevel)
            {
                return tiers[i];
            }
        }


        // Level number is past every defined tier —
        // fall back to the last tier instead of breaking.
        if (tiers.Length > 0)
        {
            Debug.LogWarning(
                "LEVEL " +
                levelNumber +
                " HAS NO DEFINED TIER — USING LAST TIER AS FALLBACK"
            );

            return tiers[tiers.Length - 1];
        }


        Debug.LogError(
            "NO DIFFICULTY TIERS CONFIGURED!"
        );

        return null;
    }


    // =========================================================
    // CONVENIENCE LOOKUPS
    // =========================================================

    public int GetGridSizeForLevel(int levelNumber)
    {
        DifficultyTier tier =
            GetTierForLevel(levelNumber);

        return tier != null ? tier.gridSize : 3;
    }


    public int GetShuffleMovesForLevel(int levelNumber)
    {
        DifficultyTier tier =
            GetTierForLevel(levelNumber);

        return tier != null ? tier.shuffleMoves : 30;
    }


    public string GetTierNameForLevel(int levelNumber)
    {
        DifficultyTier tier =
            GetTierForLevel(levelNumber);

        return tier != null ? tier.tierName : "Unknown";
    }
}