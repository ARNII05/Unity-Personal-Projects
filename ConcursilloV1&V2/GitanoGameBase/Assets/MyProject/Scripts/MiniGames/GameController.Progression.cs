using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public partial class GameController
{
    string GetFullDirectory()
    {
        string baseDirectory = gameModeList.gameModes[gameMode].basePath;

        return $"{basePath}/{baseDirectory}/Level{level + 1}";
    }
    
    void LoadImg()
    {
        string cpResource = checkpointPaths[actualCheckpoint];

        string levelPath = GetFullDirectory();

        Sprite sprite = Resources.Load<Sprite>($"{levelPath}/{cpResource}/{cpResource}");
        mainImg.sprite = sprite;
    }

    void LoadImgSequence(int from, int to, int cp)
    {
        string levelPath = GetFullDirectory();

        int imgIndex = 1;
            
        for (int j = from; j <= to; j++)
        {
            GameObject gm = imgSequenceMode.transform.GetChild(j).gameObject;
                
            Image gmImage = gm.GetComponent<Image>();

            gmImage.sprite = Resources.Load<Sprite>($"{levelPath}/{checkpointPaths[cp]}/Img{imgIndex}");
            imgIndex++;
        }
    }

    void NextLevel(Color32 color, HashSet<Enum> correctAnswers, int pointsEarned)
    {
        DistributePoints(pointsEarned);
        userSearch.DeactivateInputField();
        levelImgs[level].color = color;
        PlayerPrefs.SetFloat($"GameMode{gameMode}Level{level}TimeLeft", currentTime);

        ResetLevel(correctAnswers);

        if (level == totalLevels - 1)
        {
            gameStarted = false;
            canSwapMiniGame = true;
            string msg = gameMode == totalGames - 1 ? "Todos los juegos completados!! Tab para ver tu resumen!!" : $"Tab para jugar a \"{gameModeList.gameModes[gameMode + 1].name}\"";
            SetIndications(textAnimator, msg, 3f);
            return;
        }

        SetIndications(textAnimator, "Espacio para cambiar de nivel", 3f);
    }

    void ResetLevel(HashSet<Enum> correctAnswers)
    {        
        ResetFilterColors();
        OnSearchValueChanged();
        ShowCorrectAnswersMode(userAnswers, correctAnswers);
        ApplyFinalFilter(correctAnswers);
        musicManager.StopMusic();
        LoadLevelPause();
        audioSource.clip = null;
        canWrite = false;
        gameStarted = false;
        canInitTimer = true;

        StartCoroutine(
        FocusOnPhoto(userAnswers, correctAnswers, true));
    }
    
    void ShowCorrectAnswersMode(HashSet<Enum> userWords, HashSet<Enum> correctWords)
    {
        if (userWords == null) return;

        GameObject panel = GetPanel();

        foreach (Enum correctWord in correctWords)
        {
            panel.transform.Find(correctWord.ToString()).GetChild(0).GetComponent<Image>().color = green;
        }

        foreach (Enum userWord in userWords)
        {
            if (!correctWords.Contains(userWord))
            {
                panel.transform.Find(userWord.ToString()).GetChild(0).GetComponent<Image>().color = red;
            }
        }
    }

    void ApplyFinalFilter(HashSet<Enum> correctAnswers)
    {
        if (gameModeType == GameModeType.MapImg) return;

        foreach (Transform child in GetPanel().transform)
        {
            if (child.name.Contains("Template")) continue;

            CanvasGroup cg = child.GetComponent<CanvasGroup>();

            Enum childNameEnum = ParseAnswer(GetAnswerEnumType(gameModeType), child.name);

            bool isInUserAnswers = userAnswers.Contains(childNameEnum);
            bool isInCorrectAnswers = correctAnswers.Contains(childNameEnum);
            bool isAnswerIncorrect = incorrectOptions.Contains(childNameEnum);

            switch (isInUserAnswers, isInCorrectAnswers, isAnswerIncorrect)
            {
                case (true, true, _):
                    ChangeImgPropierties(cg, Vector3.one * 1.15f, 1f, child);
                    child.GetComponent<Image>().color = cyan;
                    break;

                case var _ when (isInCorrectAnswers || isAnswerIncorrect):
                    ChangeImgPropierties(cg, Vector3.one * 1.15f, 1f, child);
                    break;

                default:
                    ChangeImgPropierties(cg, Vector3.one, 0.2f, child);
                    break;
            }
        }
    }

    void ChangeImgPropierties(CanvasGroup cg, Vector3 localScale, float alpha, Transform child)
    {
        cg.alpha = alpha;
        child.localScale = localScale;
    }

    void LoadLevelPause()
    {
        switch (gameModeType)
        {
            case GameModeType.CharImg:
                mainImg.sprite = Resources.Load<Sprite>($"{GetFullDirectory()}/FullImg");
                break;
            
            case GameModeType.MapImg:
                mainImg.sprite = Resources.Load<Sprite>($"{GetFullDirectory()}/Cp3/Cp3");
                break;
            
            case GameModeType.CharDesc:
                LoadImgSequence(2, 2, 1);
                LoadImgSequence(3, 4, 2);
                break;

            case GameModeType.ShuffledHeroes:
                ShowXImgSequence(0, 4);
                LoadShuffledHeroImgs();
                shuffledHeroesEye.SetActive(true);
                shuffledHeroesEye.GetComponent<Image>().sprite = Resources.Load<Sprite>("ConcursilloV2/EyeIcons/OpenEye");
                break;

            case GameModeType.TfSound:
                PausedAudioMode();
                break;
            
            case GameModeType.CharSound:
                PausedAudioMode();
                wintonAnimator.speed = 0;
                wintonAnimator.Play("WintonAnim", 0, 0f);
                break;
        }
    }

    void LoadShuffledHeroImgs()
    {
        imgMode.SetActive(false);
        imgSequenceMode.SetActive(true);
        
        string path = GetFullDirectory();

        for (int i = 0; i < imgSequenceMode.transform.childCount; i++)
        {
            GameObject gm = imgSequenceMode.transform.GetChild(i).gameObject;

            Image gmImage = gm.GetComponent<Image>();

            gmImage.sprite = Resources.Load<Sprite>($"{path}/Img{i + 1}");
        }
    }

    void ShowXImgSequence(int begin, int end)
    {
        for (int i = begin; i < end; i++)
            imgSequenceMode.transform.GetChild(i).gameObject.SetActive(true);
    }

    void PausedAudioMode()
    {
        canPlayVideo = true;
        imgMode.SetActive(false);
        audioMode.SetActive(false);
        videoMode.SetActive(true);
        slider.value = 0;

        VideoClip localVideoClip = GetVideoSource();
        
        if (localVideoClip == null)
        {
            Debug.LogWarning($"Could not load video for {GetFullDirectory()}.");
            SetMediaTime(0, 0);
            return;
        }

        SetMediaTime(0, localVideoClip.length);

        LoadVideo(localVideoClip);
    }

    void ResetAll()
    {
        canSwapMiniGame = false;
        
        if (gameMode == totalGames - 1)
        {
            soundEffects.PlaySound(soundEffects.payload);
            inQuiz = false;
            ShowTeamSummary();
            return;
        }

        streak = 0;
        canWrite = false;
        canInitTimer = true;
        gameMode++;
        points.text = $"Puntos: 0";
        ResetLevelColors();
        NextGameModeStep(3f);
    }

    void SwapCheckpoint(KeyCode key, Color actualCp)
    {
        if (!gameStarted || !CheckpointUI.activeSelf)
            return;
        
        var canSwapCp = key switch
        {
            KeyCode.RightArrow => IncrementCp(actualCp),
            KeyCode.LeftArrow => ReduceCp(actualCp),
            _ => false,
        };
        
        if (!canSwapCp) return;

        maxCheckpoint = Mathf.Max(maxCheckpoint, actualCheckpoint);

        soundEffects.PlaySound(soundEffects.electionSound);
        checkpointsImgs[actualCheckpoint].color = blue;
        checkpointsImgs[maxCheckpoint].gameObject.transform.GetChild(0).GetComponent<Image>().color = red;
        DecideWhatToLoad();
    }

    public void SwapEyeImgStatus()
    {
        string eyeImgPath = "ConcursilloV2/EyeIcons";

        Image eyeImg = shuffledHeroesEye.GetComponent<Image>();

        if (eyeImg.sprite.name == "OpenEye")
        {
            imgMode.SetActive(true);
            imgSequenceMode.SetActive(false);
        }
        else
        {
            imgMode.SetActive(false);
            imgSequenceMode.SetActive(true);
        }

        eyeImg.sprite = eyeImg.sprite.name ==
            "OpenEye" ? Resources.Load<Sprite>($"{eyeImgPath}/ClosedEye") : Resources.Load<Sprite>($"{eyeImgPath}/OpenEye");
    }

    bool ReduceCp(Color actualCp)
    {
        if (actualCheckpoint == 0 || gameModeType == GameModeType.CharDesc) return false;
        
        checkpointsImgs[actualCheckpoint].color = actualCp;
        actualCheckpoint--;

        return true;
    }
    
    bool IncrementCp(Color actualCp)
    {
        if (actualCheckpoint == totalCheckpoints - 1) return false;

        checkpointsImgs[actualCheckpoint].color = actualCp;
        checkpointsImgs[actualCheckpoint].gameObject.transform.GetChild(0).GetComponent<Image>().color = cpBorderBaseColor;
        actualCheckpoint++;
        
        return true;
    }

    void FillHearts()
    {
        heartsLeft = totalHearts;

        for (int i = 0; i < heartImgs.Length; i++)
        {
            heartImgs[i].sprite = hearts[0];
        }
    }

    void FillXHearts(int x)
    {
        heartsLeft = x;
        
        for (int i = heartImgs.Length; i > heartImgs.Length - x; i--)
        {
            heartImgs[i - 1].sprite = hearts[0];
        }

        for (int i = 0; i < heartImgs.Length - x; i++)
        {
            heartImgs[i].sprite = hearts[1];
        }
    }

    void ResetCheckpoints()
    {
        actualCheckpoint = 0;
        maxCheckpoint = 0;
        checkpointsImgs[0].color = blue;
        checkpointsImgs[0].gameObject.transform.GetChild(0).GetComponent<Image>().color = red;

        for (int i = 1; i < checkpointsImgs.Length; i++)
        {
            checkpointsImgs[i].color = purple;
            checkpointsImgs[i].gameObject.transform.GetChild(0).GetComponent<Image>().color = cpBorderBaseColor;
        }
    }

    void ResetFilterColors()
    {
        GameObject activeFilterPanel = GetActiveFilterPanel();

        for (int i = 0; i < activeFilterPanel.transform.childCount; i++)
        {
            Transform child = activeFilterPanel.transform.GetChild(i);
            
            child.GetComponent<Image>().color = filtersBackGroundBaseColor;
        }

        actualRoleIndex = -1;
    }

    void ResetLevelColors()
    {
        level = -1;

        levelImgs[0].color = blue;

        for (int i = 1; i < levelImgs.Length; i++)
        {
            levelImgs[i].color = purple;
        }
    }
}
