using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class GameController
{
    void ShowTeamSummary()
    {
        sumRankImg.preserveAspect = true;
        
        int pointsEarned = PlayerPrefs.GetInt($"GameMode{summaryGameIndex}Score", 0);

        quiz.SetActive(false);
        summary.SetActive(true);

        summaryTexts[0].text = $"{gameModeNames[summaryGameIndex]}: {pointsEarned}/{MaxPointsPerGameMode()} puntos";
        summaryTexts[1].text = $"Tiempo usado: {MinutesSpended(CalculateTotalTime())}:{SecondsSpended(CalculateTotalTime()):00} min";
        summaryTexts[2].text = $"Niveles correctos: {PlayerPrefs.GetInt($"GameMode{summaryGameIndex}CorrectAnswers", 0)}";
        summaryTexts[3].text = $"Tiempo medio usado: {AverageTimeUsed()}";
        sumRankImg.sprite = Resources.Load<Sprite>(RankImg());
    }

    int MaxPointsPerGameMode()
    {
        return GetGameModeType(gameModeList.gameModes[summaryGameIndex]) switch
        {
            GameModeType.TfSound => 3000,
            _ => 2500
        };
    }

    string RankImg()
    {
        int maxPoints = MaxPointsPerGameMode();
        const string basePath = "ConcursilloV2/RankImgs";

        return $"{basePath}/{GetRankImgPath(RankCp(maxPoints / 8, maxPoints))}";
    }

    int RankCp(int cpDivided, int maxPoints)
    {
        int pointsEarned = PlayerPrefs.GetInt($"GameMode{summaryGameIndex}Score", 0);
        int actualCp = 0;

        if (pointsEarned <= cpDivided)
            return actualCp;

        actualCp = 1;
        
        for (int i = cpDivided * 2; i < maxPoints; i += cpDivided)
        {
            if (pointsEarned >= i)
                actualCp++;
            else
                break;
        }

        return actualCp;
    }

    string GetRankImgPath(int index)
    {
        return index switch
        {
            0 => "Bronze",
            1 => "Silver",
            2 => "Gold",
            3 => "Plat",
            4 => "Diamond",
            5 => "Master",
            6 => "GrandMaster",
            _ => "Champion",
        };
    }

    /*
    string LevelsFirstTry()
    {
        List<int> firtsTimeLevels = new();
        string levels = "";

        for (int i = 0; i < totalLevels; i++)
        {
            if (PlayerPrefs.GetInt($"GameMode{summaryGameIndex}Level{i}CheckpointsUsed", 1) == 0)
                firtsTimeLevels.Add(i);
        }
        
        for (int i = 0; i < firtsTimeLevels.Count; i++)
        {
            levels += $"{firtsTimeLevels[i] + 1}";
            
            if (i < firtsTimeLevels.Count - 1)
                levels += ", ";
        }

        return levels;
    }
    */

    /*
    int FastestLevel()
    {
        List<float> times = new();

        for (int i = 0; i < totalGames; i++)
        {
            times.Add(PlayerPrefs.GetFloat($"GameMode{summaryGameIndex}Level{i}TimeLeft", 0));
        }

        float minTime = Mathf.Min(times.ToArray());

        if (times[0] == minTime) return 0;

        for (int i = 1; i < times.Count; i++)
        {
            if (times[i] == minTime) return i + 1;
        }

        return -1;
    }
    */

    string AverageTimeUsed()
    {
        int correctAnswers = PlayerPrefs.GetInt($"GameMode{summaryGameIndex}CorrectAnswers", 0);
        
        if (correctAnswers == 0)
            return $"{0}:{0:00} min";
        
        float averageTime = CalculateTotalTime() / totalLevels;

        int miniutes = MinutesSpended(averageTime);
        int seconds = SecondsSpended(averageTime);

        return $"{miniutes}:{seconds:00} min";
    }

    int MinutesSpended(float total)
    {
        return (int)(total / 60f);
    }

    int SecondsSpended(float total)
    {
        return (int)(total % 60f);
    }

    float CalculateTotalTime()
    {
        float levelTimeUsed = 0;
        
        for (int i = 0; i < totalLevels; i++)
        {
            float timeLeft = PlayerPrefs.GetFloat($"GameMode{summaryGameIndex}Level{i}TimeLeft", 0);

            levelTimeUsed = levelTimeUsed + (levelTime[i] - timeLeft);
        }

        return levelTimeUsed;
    }

    public void NextMiniGame()
    {
        if (summaryGameIndex < totalGames - 1)
        {
            summaryGameIndex++;
            ShowTeamSummary();
        }
    }

    public void PreviousMiniGame()
    {
        if (summaryGameIndex > 0)
        {
            summaryGameIndex--;
            ShowTeamSummary();
        }
    }

    public void SwapScene(int scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
}
