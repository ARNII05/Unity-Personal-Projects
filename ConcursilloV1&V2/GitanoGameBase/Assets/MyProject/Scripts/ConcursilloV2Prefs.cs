using UnityEngine;

public static class ConcursilloV2Prefs
{
    private const int TotalGameModes = 5;
    private const int TotalLevels = 5;

    public static void DeleteMiniGamePrefs()
    {
        PlayerPrefs.DeleteKey("TeamPath");
        PlayerPrefs.DeleteKey("TeamImage");
        PlayerPrefs.DeleteKey("TeamName");

        for (int gameMode = 0; gameMode < TotalGameModes; gameMode++)
        {
            PlayerPrefs.DeleteKey($"GameMode{gameMode}Score");
            PlayerPrefs.DeleteKey($"GameMode{gameMode}CorrectAnswers");

            for (int level = 0; level < TotalLevels; level++)
            {
                PlayerPrefs.DeleteKey($"GameMode{gameMode}Level{level}TimeLeft");
                PlayerPrefs.DeleteKey($"GameMode{gameMode}Level{level}CheckpointsUsed");
            }
        }

        PlayerPrefs.Save();
    }
}
