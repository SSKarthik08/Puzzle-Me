using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Tracks and PERSISTS (to a real binary file, not PlayerPrefs):
//   - which level the player last exited on ("Current Level")
//   - the highest level unlocked
//   - which levels have been completed
//
// Every other script talks to THIS class only — nothing else
// touches the file directly. That keeps the save FORMAT free to
// change later without breaking any calling code.
public static class PuzzleProgress
{
    // Bump this if the binary layout ever changes, so old save
    // files can be detected/handled instead of crashing on load.
    private const int SaveVersion = 1;

    private static readonly string SaveFilePath =
        Path.Combine(
            Application.persistentDataPath,
            "puzzle_save.dat"
        );

    private static int currentLevel = 1;
    private static int highestUnlockedLevel = 1;
    private static readonly HashSet<int> completedLevels =
        new HashSet<int>();

    private static bool hasLoaded;

    // Set by a Level Map / Home screen right before loading the
    // puzzle scene, read by PuzzleBoard on Start(). NOT saved to
    // disk itself — CurrentLevel (below) is the persisted value.
    public static int LevelToLoad = 0;

    // Set right before loading the Loading scene, so ONE
    // Loading scene can serve as a hub for multiple flows
    // (Splash -> Loading -> Home, Home -> Loading -> Gameplay,
    // etc.) instead of needing a separate scene per destination.
    // LoadingController reads this and clears it after use; if
    // it's empty, LoadingController falls back to whatever
    // scene name is set in its own Inspector field.
    public static string NextSceneOverride = "";


    // =========================================================
    // AUTO-LOAD ON GAME START
    // =========================================================

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad
    )]
    private static void AutoLoadOnStartup()
    {
        LoadFromDisk();
    }


    // =========================================================
    // PUBLIC READ ACCESS
    // =========================================================

    public static int CurrentLevel
    {
        get
        {
            EnsureLoaded();
            return currentLevel;
        }
    }


    public static int HighestUnlockedLevel
    {
        get
        {
            EnsureLoaded();
            return highestUnlockedLevel;
        }
    }


    public static bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= HighestUnlockedLevel;
    }


    public static bool IsLevelCompleted(int levelNumber)
    {
        EnsureLoaded();
        return completedLevels.Contains(levelNumber);
    }


    // =========================================================
    // SET CURRENT LEVEL
    // Call this every time a level actually starts (not just on
    // completion) — so exiting mid-level and returning resumes
    // on that same level.
    // =========================================================

    public static void SetCurrentLevel(int levelNumber)
    {
        EnsureLoaded();

        currentLevel = levelNumber;

        SaveToDisk();
    }


    // =========================================================
    // MARK LEVEL COMPLETED
    // Unlocks the next level automatically if it wasn't already.
    // =========================================================

    public static void MarkLevelCompleted(int levelNumber)
    {
        EnsureLoaded();

        completedLevels.Add(levelNumber);

        int nextLevel =
            levelNumber + 1;

        if (nextLevel > highestUnlockedLevel)
        {
            highestUnlockedLevel = nextLevel;
        }

        SaveToDisk();

        Debug.Log(
            "LEVEL " +
            levelNumber +
            " MARKED COMPLETE — HIGHEST UNLOCKED IS NOW " +
            highestUnlockedLevel
        );
    }


    // =========================================================
    // RESET ALL PROGRESS ("Restart Levels" button)
    // =========================================================

    public static void ResetAllProgress()
    {
        currentLevel = 1;
        highestUnlockedLevel = 1;
        completedLevels.Clear();

        SaveToDisk();

        Debug.Log(
            "ALL PROGRESS RESET"
        );
    }


    // =========================================================
    // LOAD FROM DISK (BINARY)
    // =========================================================

    private static void EnsureLoaded()
    {
        if (!hasLoaded)
        {
            LoadFromDisk();
        }
    }


    private static void LoadFromDisk()
    {
        hasLoaded = true;

        if (!File.Exists(SaveFilePath))
        {
            Debug.Log(
                "NO SAVE FILE FOUND — STARTING FRESH " +
                "(CURRENT LEVEL 1)"
            );

            return;
        }

        try
        {
            using (
                FileStream stream =
                    new FileStream(
                        SaveFilePath,
                        FileMode.Open,
                        FileAccess.Read
                    )
            )
            using (
                BinaryReader reader =
                    new BinaryReader(stream)
            )
            {
                int version =
                    reader.ReadInt32();

                if (version != SaveVersion)
                {
                    Debug.LogWarning(
                        "SAVE FILE VERSION MISMATCH (" +
                        version +
                        " vs " +
                        SaveVersion +
                        ") — IGNORING OLD SAVE"
                    );

                    return;
                }

                currentLevel =
                    reader.ReadInt32();

                highestUnlockedLevel =
                    reader.ReadInt32();

                int completedCount =
                    reader.ReadInt32();

                completedLevels.Clear();

                for (int i = 0; i < completedCount; i++)
                {
                    completedLevels.Add(
                        reader.ReadInt32()
                    );
                }
            }

            Debug.Log(
                "SAVE LOADED — CURRENT LEVEL: " +
                currentLevel +
                ", HIGHEST UNLOCKED: " +
                highestUnlockedLevel +
                ", COMPLETED COUNT: " +
                completedLevels.Count
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "FAILED TO LOAD SAVE FILE — STARTING FRESH. " +
                "ERROR: " +
                e.Message
            );

            currentLevel = 1;
            highestUnlockedLevel = 1;
            completedLevels.Clear();
        }
    }


    // =========================================================
    // SAVE TO DISK (BINARY)
    // =========================================================

    private static void SaveToDisk()
    {
        try
        {
            using (
                FileStream stream =
                    new FileStream(
                        SaveFilePath,
                        FileMode.Create,
                        FileAccess.Write
                    )
            )
            using (
                BinaryWriter writer =
                    new BinaryWriter(stream)
            )
            {
                writer.Write(SaveVersion);
                writer.Write(currentLevel);
                writer.Write(highestUnlockedLevel);

                writer.Write(completedLevels.Count);

                foreach (int level in completedLevels)
                {
                    writer.Write(level);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "FAILED TO SAVE PROGRESS: " +
                e.Message
            );
        }
    }
}