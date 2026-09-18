using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class GameController : MonoBehaviour
{
    void Start()
    {
        quiz.SetActive(true);
        summary.SetActive(false);
        optionsPanel.SetActive(false);
        wintonAnimator = wintonGif.GetComponent<Animator>();
        wintonAnimator.speed = 0;
        wintonGif.GetComponent<Image>().preserveAspect = true;
        selectedHeroesText = selectedHeroes.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        basePath = $"ConcursilloV2/{PlayerPrefs.GetString("TeamPath", "")}";
        TextAsset jsonText = Resources.Load<TextAsset>(basePath);
        gameModeList = JsonUtility.FromJson<GameModeList>(jsonText.text);
        teamName.text = PlayerPrefs.GetString("TeamName", "");
        TeamIcon.sprite = Resources.Load<Sprite>($"ConcursilloV2/TeamIcons/TeamIcon{PlayerPrefs.GetInt("TeamImage", 0)}");
        ShuffleModes(gameModeList.gameModes);
        //ChooseFirstGameMode(gameModeList.gameModes, GameModeType.CharImg);
        FillGameModeNames();
        musicManager = MusicManager.EnsureInstance();
        soundEffects = SoundEffectsManager.EnsureInstance();
        AudioSettingsManager.RegisterAudioSource(audioSource, AudioChannel.Game);
        textAnimator = instructionsText.GetComponent<Animator>();
        SetIndications(textAnimator, "Espacio para iniciar el juego", 3f);
        LoadGameMode();
    }

    void Update()
    {
        if ((GetGameModeType(gameModeList.gameModes[gameMode]) == GameModeType.TfSound
            || GetGameModeType(gameModeList.gameModes[gameMode]) == GameModeType.CharSound) && canPlayVideo)
            VideoControl();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptions();
        }

        if (!userSearch.isFocused && Input.GetKeyDown(KeyCode.Space))
            NextGameModeStep();

        if (Input.GetKeyDown(KeyCode.Tab) && canSwapMiniGame)
            ResetAll();

        if (!gameStarted) return;

        //ControlWintonAnim();

        ManageKeys();

        if ((GetGameModeType(gameModeList.gameModes[gameMode]) == GameModeType.TfSound
            || GetGameModeType(gameModeList.gameModes[gameMode]) == GameModeType.CharSound) && !canPlayVideo)
            AudioControl();
         
        if (!timeIsUp) TimeControl(Time.deltaTime);
    }

}
