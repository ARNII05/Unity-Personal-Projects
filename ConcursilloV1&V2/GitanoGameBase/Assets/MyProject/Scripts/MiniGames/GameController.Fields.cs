using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public partial class GameController
{
    private GameModeList gameModeList;

    [Header("Mode Containers")]
    [SerializeField]
    private GameObject imgMode, audioMode, videoMode,
        imgSequenceMode, CheckpointUI,
        HeartsBox, heroTemplate, mapSelectionTemplate, heroSelectionPanel, mapSelectionPanel,
        mapSrollView, quiz, optionsPanel, summary, instructionsText, selectedHeroes, rolFilters, mapRoleFilter,
        shuffledHeroesEye, wintonGif;

    [Header("Images")]
    [SerializeField] private Image TeamIcon, mainImg, sumRankImg;
    [SerializeField] private Image[] levelImgs, checkpointsImgs, heartImgs;
    [SerializeField] private Sprite[] hearts;

    [Header("Media")]
    [SerializeField] private RawImage rawImage;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Slider slider;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI teamName;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI gameModeName;
    [SerializeField] private TextMeshProUGUI points;
    [SerializeField] private TextMeshProUGUI audioTime;
    [SerializeField] private TextMeshProUGUI[] summaryTexts;
    private TextMeshProUGUI selectedHeroesText;

    [SerializeField] private TMP_InputField userSearch;

    [SerializeField] private MusicManager musicManager;
    [SerializeField] private SoundEffectsManager soundEffects;

    [SerializeField] private AudioClip overTimeMusic;

    private string basePath;
    private readonly string[] checkpointPaths = { "Cp1", "Cp2", "Cp3" };
    private string[] gameModeNames;
    
    private readonly int[] pointsEarned = { 500, 300, 100 };
    private readonly int[] totalHeroesPerLevel = { 2, 4, 6, 8, 10 };
    private const int totalLevels = 5;
    private const int totalGames = 6;
    private const int totalHearts = 3;
    private const int totalCheckpoints = 3;
    private int level = 0;
    private int heartsLeft;
    private int gameMode = 0;
    private int actualCheckpoint = 0;
    private int summaryGameIndex = 0;
    private int heroesSelected = 0;
    private int streak = 0;
    private int maxCheckpoint = 0;
    private int actualRoleIndex = -1;

    private Vector2 audioNormalPos = new(115.82f, -130f);
    private Vector2 audioModifiedPos = new(115.82f, -81f);

    private Vector2 selectedHeroesNormalPos = new(557.4153f, -169.1574f);
    private Vector2 selectedHeroesModifiedPos = new(1202, -109);

    private Coroutine instructionsCoroutine;

    private Animator textAnimator, wintonAnimator;

    private readonly Color32 baseColor = Color.white;
    private readonly Color32 backgroundBaseColor = new(236, 145, 21, 168);
    private readonly Color32 selectedColor = Color.magenta;
    private readonly Color32 filtersBackGroundBaseColor = new(61, 255, 244, 255);
    private readonly Color32 cpBorderBaseColor = new(229, 190, 46, 255);
    private readonly Color32 green = new(36, 245, 50, 255);
    private readonly Color32 blue = new(74, 250, 239, 255);
    private readonly Color32 red = new(241, 15, 31, 255);
    private readonly Color32 purple = new(255, 16, 219, 255);
    private readonly Color cyan = new(0, 1, 1);

    private readonly HashSet<System.Enum> userAnswers = new();
    private readonly HashSet<System.Enum> descartedOptions = new();
    private readonly HashSet<System.Enum> incorrectOptions = new();

    public ScrollRect miScroll;

    private float currentTime;
    private readonly float[] levelTime = { 60f, 90f, 120f, 120f, 120f };

    private bool gameStarted = false;
    private bool canWrite = false;
    private bool canInitTimer = true;
    private bool isDragging = false;
    private bool canPlayVideo = false;
    private bool timeIsUp = false;
    private bool overtimeAdvicePlayed = false;
    private bool inQuiz = true;
    private bool canSwapMiniGame = false;
    private bool inMiniPreview = false;
    private bool isFirstLevel = true;

    private GameModeType activePanel = GameModeType.None;
    private GameModeType gameModeType = GameModeType.None;

    private enum GameModeType
    {
        CharImg,
        MapImg,
        CharDesc,
        TfSound,
        CharSound,
        ShuffledHeroes,
        None
    }

    private enum MapType
    {
        Assault,
        Control,
        Escort,
        Hybrid,
        Push,
        Clash,
        FlashPoint,
        None
    }

    private enum HeroRole
    {
        Tank,
        Dps,
        Support,
        None
    }

    private enum Maps
    {
        Colosseo,
        Esperanca,
        NewQueenStreet,
        Runasapi,
        Hanaoka,
        TronoDeAnubis,
        Busan,
        Ilios,
        Nepal,
        Oasis,
        PeninsulaAntartica,
        Samoa,
        TorreLijiang,
        CircuitRoyal,
        Dorado,
        JunkerTown,
        LaHabana,
        MonasterioShambali,
        ObservatorioGibraltar,
        Rialto,
        Ruta66,
        BlizzardWorld,
        Eichenwalde,
	    Hanamura,
        Hollywood,
        KingsRow,
        Midtown,
        Numbani,
        Paraiso,
        Aatlis,
        NewJunkCity,
        Suravasa,
        IndustriasVolskaya,
        ColoniaLunarHorizon,
        None
    }

    private enum Chars
    {
       Ana,
       Anran,
       Ashe,
       Baptiste,
       Bastion,
       Brigitte,
       Cassidy,
       Domina,
       Doomfist,
       Dva,
       Echo,
       Emre,
       Freja,
       Genji,
       Hanzo,
       Hazard,
       Illari,
       JetpackCat,
       JunkerQueen,
       Junkrat,
       Juno,
       Kiriko,
       LifeWeaver,
       Lucio,
       Mauga,
       Mei,
       Mercy,
       Mizuki,
       Moira,
       Orisa,
       Pharah,
       Ramattra,
       Reaper,
       Reinhardt,
       Roadhog,
       Shion,
       Sierra,
       Sigma,
       Sojourn,
       Soldado76,
       Sombra,
       Symmetra,
       Torbjorn,
       Tracer,
       Vendetta,
       Venture,
       Widowmaker,
       Winston,
       WreckingBall,
       Wuyang,
       Zarya,
       Zenyatta,
       None
    }
}
