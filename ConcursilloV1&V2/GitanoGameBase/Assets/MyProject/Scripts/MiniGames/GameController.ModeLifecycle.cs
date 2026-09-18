using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class GameController
{
    void NextGameModeStep(float delay = 1.75f)
    {
        if (inMiniPreview) return;

        if (!canInitTimer || gameStarted || canSwapMiniGame)
            return;

        StartCoroutine(PreviewAndAutomaticAdvance(delay));
    }

    private IEnumerator PreviewAndAutomaticAdvance(float delay = 1.75f)
    {
        SetIndications(textAnimator, "Cargando nivel...", 3f);

        inMiniPreview = true;

        if (isFirstLevel) isFirstLevel = false;
        else
        {
            LevelUp();
            LoadGameMode();
        }

        yield return new WaitForSecondsRealtime(delay);

        SetIndications(textAnimator, $"Elige un {(gameModeType == GameModeType.MapImg ? "Mapa" : "Héroe")} clicando sobre él", 3f);

        InitGame();

        inMiniPreview = false;
    }

    void LevelUp()
    {
        level++;

        if (level < totalLevels)
        {
            levelImgs[level].color = blue;
        }
    }

    void InitGame()
    {
        canPlayVideo = false;
        gameStarted = true;
        canInitTimer = false;
        canWrite = true;
        timeIsUp = false;
        overtimeAdvicePlayed = false;

        DecideWhatToLoad();
    }

    void LoadGameMode()
    {
        miScroll.content.anchoredPosition = new Vector2(0, 0);
        heroesSelected = 0;
        currentTime = levelTime[level];
        gameModeName.text = $"{gameMode + 1}. {gameModeList.gameModes[gameMode].name}";
        userAnswers.Clear();
        descartedOptions.Clear();
        incorrectOptions.Clear();
        ResetFilterColors();
        ResetCheckpoints();
        ManageGameMode();
        SetTimer();
        ClearColors(GetPanel());
        ResetChildren();
        userSearch.placeholder.GetComponent<TextMeshProUGUI>().text = GetBasePlaceHolderMsg();

        bool isModeWithOneHp = gameModeType == GameModeType.TfSound
            || gameModeType == GameModeType.ShuffledHeroes;

        if (!isModeWithOneHp) return;

        FillXHearts(1);
    }

    void DecideWhatToLoad()
    {
        musicManager.PlayMusic();

        switch (gameModeType)
        {
            case GameModeType.CharImg:
            case GameModeType.MapImg:
            case GameModeType.ShuffledHeroes:
                LoadImg();
                break;
            case GameModeType.CharDesc:
                InitImgSequence();
                break;
            case GameModeType.TfSound:
            case GameModeType.CharSound:
                LoadAudio();
                break;
        }
    }

    void InitImgSequence()
    {
        switch (actualCheckpoint)
        {
            case 0:
                LoadImgSequence(0, 1, 0);
                break;
            case 1:
                LoadImgSequence(2, 2, 1);
                break;
            case 2:
                LoadImgSequence(3, 4, 2);
                break;
        }
    }

    void ManageGameMode()
    {
        videoMode.SetActive(false);
        imgMode.SetActive(false);
        audioMode.SetActive(false);
        imgSequenceMode.SetActive(false);
        heroSelectionPanel.SetActive(false);
        mapSrollView.SetActive(false);
        shuffledHeroesEye.SetActive(false);

        gameModeType = GetGameModeType(gameModeList.gameModes[gameMode]);

        rolFilters.SetActive(gameModeType != GameModeType.MapImg);
        mapRoleFilter.SetActive(gameModeType == GameModeType.MapImg);
        wintonGif.SetActive(gameModeType == GameModeType.CharSound);
        selectedHeroes.SetActive(gameModeType == GameModeType.TfSound || gameModeType == GameModeType.ShuffledHeroes);
        CheckpointUI.SetActive(gameModeType != GameModeType.TfSound && gameModeType != GameModeType.ShuffledHeroes 
            && gameModeType != GameModeType.CharSound);

        if (selectedHeroes.activeSelf) ShowAndSwapSelectableHeroesText();

        LoadCorrectPanel();

        FillHearts();

        switch (gameModeType)
        {
            case GameModeType.CharImg:
            case GameModeType.MapImg:
            case GameModeType.ShuffledHeroes:
                imgMode.SetActive(true);
                mainImg.sprite = Resources.Load<Sprite>("ConcursilloV2/DefaultImg");
                if (gameModeType == GameModeType.ShuffledHeroes) 
                    selectedHeroes.GetComponent<RectTransform>().anchoredPosition = selectedHeroesModifiedPos;
                break;
            
            case GameModeType.CharDesc:
                ImgSequenceUI();
                break;
            
            case GameModeType.TfSound:
                audioMode.GetComponent<RectTransform>().anchoredPosition = audioModifiedPos;
                selectedHeroes.GetComponent<RectTransform>().anchoredPosition = selectedHeroesNormalPos;
                AudioUI();
                break;
            
            case GameModeType.CharSound:
                audioMode.GetComponent<RectTransform>().anchoredPosition = audioNormalPos;
                AudioUI();
                break;
        }
    }

    void AudioUI()
    {
        audioMode.SetActive(true);

        slider.value = 0;

        AudioClip localAudioSource = GetAudioSource();

        SetMediaTime(0, localAudioSource.length);
    }

    void ImgSequenceUI()
    {
        imgSequenceMode.SetActive(true);

        foreach (Transform child in imgSequenceMode.transform)
        {
            child.gameObject.SetActive(true);
            child.GetComponent<Image>().sprite = Resources.Load<Sprite>("ConcursilloV2/DefaultImg");
        }
    }
}
