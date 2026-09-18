using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Unity.VisualScripting;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.Timeline;

public class ClassJson : MonoBehaviour
{

    //QUIZ

    public TextMeshProUGUI statement, topicText, playerName, adminStatement, adminTopicText;
    
    public GameObject[] answers, adminAnswers, helpers, adminHelpers;
    
    public Sprite[] imgs, roundedImgs;

    public AudioClip[] allMusic, baseMusic;

    public AudioSource actualMusic;

    public Image playerImg;
    public Image[] levels;
    
    int questionIndex;
    int topic;
    int dificulty;
    int randQuestion;
    int i;
    float totalQuestions;
    int answersCorrect;

    Color32 green;
    Color32 blue;
    Color32 red;
    Color32 baseAnswerColor;

    bool isResolved, isMarked;
    
    Dictionary<string, AudioClip> music;

    string player;

    Question[] question;
    QuizData data;

    //SUMMARY

    float timeSpended;

    int biggestStreak, currentStreak;
    int[] helpersIndex;

    string[] helperNames = {"Llamada", "Gitano", "Gamble", "GitanilloKing", "50/50"};
    
    public GameObject[] allObjects, summaryObjects;


    //HALL OF FAME
    
    public GameObject[] hallObjects;
    
    public Image[] hallPlayerImg;


    void Awake()
    {
        player = PlayerPrefs.GetString("Selected");
        TextAsset json = Resources.Load<TextAsset>(player);

        if (json == null)
        {
            Debug.LogError("NO ENCONTRADO. Archivos en Resources:");
            TextAsset[] all = Resources.LoadAll<TextAsset>("");
            foreach (var t in all)
                Debug.Log(t.name);
        }
        
        data = JsonUtility.FromJson<QuizData>(json.text);

        music = new Dictionary<string, AudioClip>();

        playerName.text = data.player;
        playerImg.sprite = imgs[PlayerPrefs.GetInt("imgIndex")];

        //FillBaseMusic();
        //FillPlayerThemes();
    }

    private void Start()
    {
        green = new Color32(36, 245, 50, 255);
        blue = new Color32(74, 250, 239, 255);
        red = new Color32(241, 15, 31, 255);
        baseAnswerColor = new Color32(156, 156, 86, 255);
        isResolved = true;
        helpersIndex = new int[helpers.Length];
        totalQuestions = 20;    
        timeSpended = 1;
        AudioSettingsManager.RegisterAudioSource(actualMusic, AudioChannel.Music);
        Innit();
    }

    //MUSIC

    void FillBaseMusic()
    {
        for (int j = 0; j < 5; j++)
            music[data.themes[j].name] = baseMusic[dificulty];
    }

    void FillPlayerThemes()
    {
        int index = PlayerPrefs.GetInt("imgIndex") * 5;
        for (int j = 0; j + 5 < data.themes.Length; j++)
        {
            music[data.themes[j + 5].name] = allMusic[index + j];
        }
    }

    void PlayMusic()
    {
        FillBaseMusic();
        
        bool canPlayMusic = music[data.themes[topic].name] != null;

        AudioClip newClip = (topic < 5 || !canPlayMusic) ? baseMusic[dificulty] : music[data.themes[topic].name];

        if (actualMusic.clip == newClip)
            return;

        actualMusic.clip = newClip;
        actualMusic.Play();
    }

    //BUILD QUESTIONS

    public void Innit()
    {
        if (!isResolved)
            return;

        if (questionIndex == totalQuestions)
        {
            StopAllCoroutines();
            actualMusic.Stop();
            ShowSummary();
            return;
        }

        try
        {
            MarkActualQuestion();
            CleanAnswersColor(adminAnswers);
            CleanAnswersColor(answers);
            CleanUserText();

            topic = SelectTheme();
            dificulty = SetDificulty();
            randQuestion = SelectQuestion();
            questionIndex++;

            FillAnswers();
            isResolved = false;
            isMarked = false;
        }
        catch (Exception e)
        {
            print(e);
        }
    }

    void MarkActualQuestion() 
    {
        Image nextImg = levels[questionIndex].GetComponent<Image>();
        nextImg.color = blue;
    }

    void CleanAnswersColor(GameObject[] aD)
    {
        for (int i = 0; i < aD.Length; i++)
        {
            Image img = aD[i].GetComponent<Image>();
            img.color = baseAnswerColor;
        }
    }

    void CleanUserText()
    {
        for (int i = 0; i < adminAnswers.Length; i++)
        {
            TextMeshProUGUI text = answers[i].GetComponentInChildren<TextMeshProUGUI>();
            text.text = "";
        }
        statement.text = "";
        topicText.text = "";
    }

    int SelectTheme()
    {
        int randTopicNbr = Random.Range(0, data.themes.Length);

        while (data.themes[randTopicNbr].timesUsed == 2)
            randTopicNbr = Random.Range(0, data.themes.Length);

        data.themes[randTopicNbr].timesUsed++;
        adminTopicText.text = data.themes[randTopicNbr].name;
        
        return randTopicNbr;
    }

    int SetDificulty()
    {
        if (questionIndex >= 0 && questionIndex <= 4)
            return 0;
        else if (questionIndex >= 5 && questionIndex <= 9)
            return 1;
        else if (questionIndex >= 10 && questionIndex <= 14)
            return 2;

        return 3;
    }

    int SelectQuestion()
    {
        question = GetQuestionsByDifficulty(data.themes[topic], dificulty);

        int randQuestion = Random.Range(0, question.Length);

        while (data.themes[topic].idUsed.Contains(question[randQuestion].id))
            randQuestion = Random.Range(0, question.Length);

        data.themes[topic].idUsed.Add(question[randQuestion].id);
        adminStatement.text = question[randQuestion].statement;
        return randQuestion;
    }

    void FillAnswers()
    {
        for (int i = 0; i < question[randQuestion].answers.Length; i++)
        {
            TextMeshProUGUI tmp = adminAnswers[i].GetComponentInChildren<TextMeshProUGUI>();
            tmp.text = question[randQuestion].answers[i];
        }

        int correct = question[randQuestion].correct;

        Image correctImage = adminAnswers[correct].GetComponentInChildren<Image>();

        correctImage.color = green;
    }

    Question[] GetQuestionsByDifficulty(Theme theme, int difficulty)
    {
        return difficulty switch
        {
            0 => theme.easy,
            1 => theme.normal,
            2 => theme.dificult,
            3 => theme.very_dificult,
            _ => null
        };
    }


    //RESOLVE USER ANSWER

    public void CheckAnswers()
    {
        if (!isMarked || isResolved)
            return;

        ShowAllAnswers();

        int correct = question[randQuestion].correct;
        
        Image correctImage = answers[correct].GetComponentInChildren<Image>();

        correctImage.color = green;

        if (question[randQuestion].correct != i)
        {
            Image image = answers[i].GetComponentInChildren<Image>();
            image.color = red;
            currentStreak = 0;
        }
        else
        {
            answersCorrect++;
            currentStreak++;
												biggestStreak = currentStreak > biggestStreak ? currentStreak : biggestStreak;
            PlayerPrefs.SetInt(data.player, PlayerPrefs.GetInt(data.player) + 1);
        }

        ResolveLevel();
        TryToActivateJordiWild();
        isResolved = true;
    }

    void TryToActivateJordiWild()
    {
        if (answersCorrect == 10)
        {
            adminHelpers[5].SetActive(true);
            helpers[5].SetActive(true);
        }
    }

    void ResolveLevel()
    {
        Image img = levels[questionIndex - 1].GetComponent<Image>();

        img.color = question[randQuestion].correct == i ? green : red;
    }

    //ADMIN CONTROL

    public void MarkOption(int index)
    {
        TextMeshProUGUI text = answers[index].GetComponentInChildren<TextMeshProUGUI>();
        
        if (text.text == "" || isResolved)
            return;

        SwapColor(blue, index);

        i = index;
    }

    void SwapColor(Color32 color, int a)
    {
        CleanAnswersColor(answers);

        if (i == a)
            isMarked = !isMarked;
        else
            isMarked = true;

        if (isMarked)
        {
            Image img = answers[a].GetComponent<Image>();

            img.color = color;
        }
    }

    public void ShowStatementToUser()
    {
        if (isResolved)
            return;
        
        statement.text = question[randQuestion].statement;
    }

    void ShowAllAnswers()
    {
        for (int j = 0; j < answers.Length; j++)
        {
            TextMeshProUGUI tmp = adminAnswers[j].GetComponentInChildren<TextMeshProUGUI>();
            TextMeshProUGUI userA = answers[j].GetComponentInChildren<TextMeshProUGUI>();
            userA.text = tmp.text;
        }
    }

    public void ShowTopicToUser()
    {
        if (isResolved)
            return;
        
        if (questionIndex == 1)
            StartCoroutine(TimeElapsed());
        
        topicText.text = data.themes[topic].name;
        //PlayMusic();
    }

    public void ShowAnswers(int answerIndex)
    {
        if (isResolved)
            return;

        TextMeshProUGUI tmp = adminAnswers[answerIndex].GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI userA = answers[answerIndex].GetComponentInChildren<TextMeshProUGUI>();
        
        if (isMarked && i == answerIndex)
            SwapColor(baseAnswerColor, i);
        
        userA.text = userA.text == "" ? tmp.text : "";
    }

    //HELPERS

    public void ActivateHelper(int helperIndex)
    {
        if (isResolved)
            return;

        Image[] adminImages = adminHelpers[helperIndex].GetComponentsInChildren<Image>();

        Image[] userImages = helpers[helperIndex].GetComponentsInChildren<Image>();

        Image adminImg = adminImages[1];
        Image userImg = userImages[1];

        bool baseStatus = adminImg.enabled;

        adminImg.enabled = !adminImg.enabled;
        userImg.enabled = !userImg.enabled;

        helpersIndex[helperIndex]++;

        if (!baseStatus)
            return;

        switch (helperIndex)
        {
            case 1:
                GitanoHelper(helperIndex);
                break;
            case 2:
                GambleHelper(helperIndex, adminImg, userImg);
                break;
            case 4:
                FiftyFifty();
                break;
            case 5:
                JordiWildHelper();
                break;
        }
    }

    void GitanoHelper(int helperIndex)
    {
        StatusHelpers(true, adminHelpers, helpers, helperIndex);
    }

    void GambleHelper(int helperIndex, Image adminImg, Image userImg)
    {

        if (HelpersLeft() == helpers.Length - 1)
        {
            adminImg.enabled = !adminImg.enabled;
            userImg.enabled = !userImg.enabled;
            return;
        }

        int gamble = Random.Range(0, 100);

        if (gamble <= 20)
        {
            adminImg.enabled = !adminImg.enabled;
            userImg.enabled = !userImg.enabled;
        }

        if (gamble >= 0 && gamble <= 49)
            StatusHelpers(true, adminHelpers, helpers, helperIndex);
        else if (gamble >= 50)
            StatusHelpers(false, adminHelpers, helpers, helperIndex);
    }

    void FiftyFifty()
    {
        int correct = question[randQuestion].correct;
        List<int> reps = new List<int>();
        
        for (int j = 0; j < 2; j++)
        {
            int actual = Random.Range(0, question[randQuestion].answers.Length);
            while (actual == correct || reps.Contains(actual))
                actual = Random.Range(0, question[randQuestion].answers.Length);
            TextMeshProUGUI tmp = answers[actual].GetComponentInChildren<TextMeshProUGUI>();
            reps.Add(actual);
            tmp.text = "";
        }
    }

    void JordiWildHelper()
    {
        i = question[randQuestion].correct;
        isMarked = true;
        CheckAnswers();
        adminHelpers[5].SetActive(false);
        helpers[5].SetActive(false);
    }

    void StatusHelpers(bool status, GameObject[] go, GameObject[] go1, int helperIndex)
    {
        List<int> h = new();
        Image[] img;

        for (int j = 0; j < go.Length - 1; j++)
        {
            if (j == helperIndex)
                continue;
            
            img = go[j].GetComponentsInChildren<Image>();
            Image aImg = img[1];
            
            if (aImg.enabled == status)
                h.Add(j);
        }

        if (h.Count == 0)
            return;

        int r = Random.Range(0, h.Count);

        img = go[h[r]].GetComponentsInChildren<Image>();
        img[1].enabled = !status;

        img = go1[h[r]].GetComponentsInChildren<Image>();
        img[1].enabled = !status;
    }


    //SUMARY
    IEnumerator TimeElapsed()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            timeSpended++;
        }
    }

    void ShowSummary()
    {
        allObjects[0].SetActive(false);
        allObjects[1].SetActive(true);

        int mostUsed = MostUsedhelper();
        int totalHelpers = TotalHelpers();

        TextMeshProUGUI[] summaryTexts = new TextMeshProUGUI[summaryObjects.Length];

        for (int j = 0; j < summaryObjects.Length; j++)
            summaryTexts[j] = summaryObjects[j].GetComponentInChildren<TextMeshProUGUI>();

        summaryTexts[0].text = $"Tiempo total usado -> {timeSpended / 60}m";
        summaryTexts[1].text = $"Total de preguntas correctas -> {answersCorrect}/{totalQuestions}";
        summaryTexts[2].text = $"Has hecho un {QuestionCorrectPercent()}% de las preguntas bien";
        summaryTexts[3].text = $"Racha m�s larga de aciertos -> {biggestStreak}";
        summaryTexts[4].text = $"Tiempo medio por pregunta -> {(timeSpended / 60) / totalQuestions}m";
        summaryTexts[5].text = $"Comodines usados -> {TotalHelpers()}";
        summaryTexts[6].text = $"Comodines sobrantes -> {HelpersLeft()}/6";
        summaryTexts[7].text = Title();

        if (totalHelpers == 0)
        {
            summaryTexts[8].text = "No has usado ninguno";
            return;
        }
            
        summaryTexts[8].text = $"Comod�n m�s usado -> {helperNames[mostUsed]}";
    }

    int HelpersLeft()
    {
        int count = 0;

        for (int j = 0; j < helpers.Length; j++)
        {
            Image[] img = helpers[j].GetComponentsInChildren<Image>();
            Image aImg = img[1];

            if (aImg.enabled == true)
                count++;
        }

        return count;
    }

    int MostUsedhelper()
    {
        int max = -1;
        int index = 0;

        for (int j = 0; j < helpersIndex.Length; j++)
        {
            if (max < helpersIndex[j])
            {
                max = helpersIndex[j];
                index = j;
            }
        }
        
        return index;
    }

    int TotalHelpers()
    {
        int total = 0;
        
        foreach (var item in helpersIndex)
            total += item;

        return total;
    }

    int QuestionCorrectPercent()
    {
        float percent = (answersCorrect / totalQuestions) * 100f;
        return Mathf.RoundToInt(percent);
    }

    string Title()
    {
        if (answersCorrect >= 0 && answersCorrect <= 4)
            return "Gitanillo Noob";
        else if (answersCorrect >= 5 && answersCorrect <= 9)
            return "Gitanillo Aprendiz";
        else if (answersCorrect >= 10 && answersCorrect <= 14)
            return "Gitanillo Experto";

        return "Gitanillo Final Boss";
    }

    //HALL OF FAME
    public void HallOfFame()
    {
        SwapItemStatus();

        List<PlayerData> playerDataArr = SortedPoints();

        TextMeshProUGUI[] hallTexts = new TextMeshProUGUI[hallObjects.Length];

        for (int j = 0; j < hallObjects.Length; j++)
            hallTexts[j] = hallObjects[j].GetComponentInChildren<TextMeshProUGUI>();

        for (int j = 0; j < playerDataArr.Count; j++)
        {
            string name = playerDataArr[j].name;
            int points = playerDataArr[j].points;
            Sprite hallImg = playerDataArr[j].img;

            hallTexts[j].text = $"{name} -> {points}/20 correctas";
            hallPlayerImg[j].sprite = hallImg;
        }
    }

    void SwapItemStatus()
    {
        allObjects[4].SetActive(!allObjects[4].activeSelf);

        if (allObjects[4].activeSelf)
        {
            allObjects[0].SetActive(false);
            allObjects[1].SetActive(false);
        }
        else
        {
            allObjects[0].SetActive(questionIndex < 20);
            allObjects[1].SetActive(questionIndex == 20);
        }

        bool playerInfoStatus = allObjects[2].activeSelf;

        allObjects[2].SetActive(!playerInfoStatus);
        allObjects[3].SetActive(!playerInfoStatus);
    }

    List<PlayerData> SortedPoints()
    {
        string[] playerNames = {"Dylan", "Veri", "Feks", "Dani", "Patry", "Emilio", "Ade"};

        List<PlayerData> playerDataArr = new();

        for (int j = 0; j < playerNames.Length; j++)
        {
            string name = playerNames[j];
            int points = PlayerPrefs.GetInt(name, 0);
            playerDataArr.Add(new PlayerData(name, points, roundedImgs[j]));
        }

        playerDataArr.Sort((a, b) => b.points.CompareTo(a.points));

        return playerDataArr;
    }
}

//PLAYER DATA

class PlayerData
{
    public string name;
    public int points;
    public Sprite img;

    public PlayerData(string name, int points, Sprite img)
    {
        this.name = name;
        this.points = points;
        this.img = img;
    }
}

//JSON CLASSES

[System.Serializable]
public class Question
{
    public int id;
    public string statement;
    public string[] answers;
    public int correct;
}

[System.Serializable]
public class Theme
{
    public string name;
    public int timesUsed;
    public List<int> idUsed;
    public Question[] easy;
    public Question[] normal;
    public Question[] dificult;
    public Question[] very_dificult;
}

[System.Serializable]
public class QuizData
{
    public string player;
    public Theme[] themes;
}
