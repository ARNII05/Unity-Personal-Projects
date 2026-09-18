using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public partial class GameController
{
    void RegisterDescartedAnswers<T>(T value, Transform childTransform) where T : Enum
    {
        if (!canWrite || !childTransform.parent.GetComponent<Button>().interactable) return;

        if (userAnswers.Remove(value))
        {
            childTransform.GetComponent<Image>().color = baseColor;
            heroesSelected--;
            ShowAndSwapSelectableHeroesText();
        }

        if (descartedOptions.Remove(value))
        {
            childTransform.parent.GetComponent<CanvasGroup>().alpha = 1f;
            return;
        }

        descartedOptions.Add(value);
        childTransform.parent.GetComponent<CanvasGroup>().alpha = 0.2f;
    }

    void RegisterUserAnswer<T>(T value, Transform childTransform) where T : Enum
    {
        if (descartedOptions.Remove(value))
            childTransform.parent.GetComponent<CanvasGroup>().alpha = 1f;

        int selectableHeroes = SelectableHeroesCount(gameModeType);

        if (!canWrite) return;

        if (userAnswers.Count == 1 && selectableHeroes == 1) OnlyOneAnswer();

        if (userAnswers.Remove(value))
        {
            childTransform.GetComponent<Image>().color = baseColor;
            heroesSelected--;
        }
        else if (heroesSelected < selectableHeroes)
        {
            userAnswers.Add(value);
            soundEffects.PlaySound(soundEffects.electionSound);
            childTransform.GetComponent<Image>().color = selectedColor;
            heroesSelected++;
        }

        ShowAndSwapSelectableHeroesText();
    }

    void OnlyOneAnswer()
    {
        Transform selectedChild = GetPanel().transform.Find(userAnswers.First().ToString()).GetChild(0);

        selectedChild.GetComponent<Image>().color = baseColor;

        userAnswers.Clear();

        heroesSelected--;
    }

    void ShowAndSwapSelectableHeroesText()
    {
        if (!selectedHeroes.activeSelf) return;

        selectedHeroesText.text = $"Héroes identificados: {heroesSelected}/{SelectableHeroesCount(gameModeType)}";
    }

    int SelectableHeroesCount(GameModeType gm)
    {
        return gm switch
        {
            _ when gm == GameModeType.TfSound => totalHeroesPerLevel[level],
            _ when gm == GameModeType.ShuffledHeroes => 5,
            _ => 1
        };
    }

    public void CheckAnswerWithActualMode()
    {
        if (!canWrite || (userAnswers.Count == 0 && !timeIsUp)) return;

        if (gameModeType == GameModeType.TfSound || gameModeType == GameModeType.ShuffledHeroes)
        {
            CheckMultipleOptionsModeAnswer();
            return;
        }

        bool isCorrect = userAnswers.Count > 0 && Equals(userAnswers.First(), GetCorrectAnswer());

        IsAnswerCorrect(
            isCorrect,
            isCorrect ? pointsEarned[maxCheckpoint] : 0,
            new HashSet<Enum>() { GetCorrectAnswer() }
        );
    }

    void CheckMultipleOptionsModeAnswer()
    {
        string normalizedCorrectAnswer = NormalizeKeepingCommas(gameModeList.gameModes[gameMode].levels[level].answer);
        string[] splitedCorrectAnswer = normalizedCorrectAnswer.Split(",", StringSplitOptions.RemoveEmptyEntries);

        HashSet<Enum> correctAnswers = splitedCorrectAnswer
            .Select(x => ParseAnswer(GetAnswerEnumType(gameModeType), x))
            .ToHashSet();

        int matches = CountMatchingWords(userAnswers, correctAnswers);
        int pointsEarned = matches * 100;

        IsAnswerCorrect(matches >= Mathf.CeilToInt(correctAnswers.Count / 2f), pointsEarned, correctAnswers);
    }

    int CountMatchingWords(HashSet<Enum> userWords, HashSet<Enum> correctWords)
    {
        if (userWords == null || correctWords == null) return 0;

        int matches = 0;

        foreach (Enum userWord in userWords)
        {
            if (correctWords.Contains(userWord))
                matches++;
        }

        return matches;
    }

    /*
    int CalculateMultipleOptionsModePoints(int matches, int totalCorrectWords)
    {
        return matches switch
        {
            _ when matches == totalCorrectWords => pointsEarned[0],
            _ when matches >= Mathf.CeilToInt(totalCorrectWords / 2f) => pointsEarned[1],
            _ when matches > 0 => pointsEarned[2],
            _ => 0
        };
    }
    */

    Enum GetCorrectAnswer()
    {
        string correctAnswerText = gameModeList.gameModes[gameMode].levels[level].answer;

        return ParseAnswer(GetAnswerEnumType(gameModeType), correctAnswerText);
    }

    void DistributePoints(int scoreEarned)
    {
        if (scoreEarned == 0) return;

        int pointsEarned = scoreEarned;
        int actualSocre = PlayerPrefs.GetInt($"GameMode{gameMode}Score", 0);

        points.text = $"+{scoreEarned} puntos";
        actualSocre += pointsEarned;
        StartCoroutine(PointsTextAnim(actualSocre));

        PlayerPrefs.SetInt($"GameMode{gameMode}Score", actualSocre);

        PlayerPrefs.Save();
    }

    IEnumerator PointsTextAnim(int actualSocre)
    {
        yield return new WaitForSeconds(2.25f);

        points.text = $"Puntos: {actualSocre}";
    }

    void SumTotalCorrectAnswers(int pointsEarned)
    {
        int actualCorrectAnswers = PlayerPrefs.GetInt($"GameMode{gameMode}CorrectAnswers", 0);
        PlayerPrefs.SetInt($"GameMode{gameMode}CorrectAnswers", actualCorrectAnswers + 1);

        PlayerPrefs.Save();
    }

    void IsAnswerCorrect(bool status, int pointsEarned, HashSet<Enum> correctAnswers)
    {
        FillIncorrectOptions(correctAnswers);

        ResetFilterColors();
        userSearch.text = "";
        OnSearchValueChanged();

        soundEffects.PlaySound(soundEffects.roundFinished);

        foreach (var item in userAnswers)
        {
            GetPanel().transform.Find(item.ToString()).GetComponent<Button>().interactable = false;
        }

        if (status)
        {
            SumTotalCorrectAnswers(pointsEarned);
            
            NextLevel(green, correctAnswers, pointsEarned);
            
            streak++;

            if (streak == 3) soundEffects.PlaySound(soundEffects.OnFireRandomClip());

            if (streak == 5) soundEffects.PlaySound(soundEffects.teamKill);

            return;
        }

        StartCoroutine(ChangePlaceHolderMsg("Cómo fallas eso...", GetBasePlaceHolderMsg()));

        StartCoroutine(FocusOnPhoto(userAnswers, correctAnswers, false));

        MarkOptionsWrong();

        heartImgs[^heartsLeft].sprite = hearts[1];

        heartsLeft--;

        if (heartsLeft == 0 || timeIsUp)
        {
            streak = 0;
            
            NextLevel(red, correctAnswers, pointsEarned);

            return;
        }

        if (gameModeType == GameModeType.CharSound && maxCheckpoint < 2)
            maxCheckpoint++;

        heroesSelected = 0;

        userAnswers.Clear();
    }

    void FillIncorrectOptions(HashSet<Enum> correctAnswers)
    {
        foreach (var item in userAnswers)
        {
            if (!correctAnswers.Contains(item))
                incorrectOptions.Add(item);
        }
    }

    IEnumerator FocusOnPhoto(HashSet<Enum> userHash, HashSet<Enum> correctHash, bool isLast = false)
    {
        if (gameModeType != GameModeType.MapImg) yield break;

        bool focusedUserAnswer = TryFocusOnFirstPhoto(userHash);

        if (isLast)
        {
            if (focusedUserAnswer)
                yield return new WaitForSeconds(1.7f);

            TryFocusOnFirstPhoto(correctHash);
        }
    }

    bool TryFocusOnFirstPhoto(HashSet<Enum> answerHash)
    {
        if (answerHash == null || answerHash.Count == 0)
            return false;

        if (miScroll == null || miScroll.content == null || miScroll.content.childCount == 0)
            return false;

        string answerName = answerHash.First().ToString();
        Transform targetPhoto = GetPanel().transform.Find(answerName);

        if (targetPhoto == null)
        {
            Debug.LogWarning($"Could not focus map photo '{answerName}' because it was not found in the map scroll content.");
            return false;
        }

        GoToPhoto(targetPhoto.GetComponent<RectTransform>());
        return true;
    }

    public void GoToPhoto(RectTransform targetFoto)
    {
        RectTransform contentPanel = miScroll.content;

        float targetY = targetFoto.anchoredPosition.y;

        contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, -targetY);
    }

    void MarkOptionsWrong()
    {
        if (gameModeType == GameModeType.TfSound ||
            gameModeType == GameModeType.ShuffledHeroes || userAnswers.Count == 0) return;

        foreach (var item in incorrectOptions)
        {
            GetPanel().transform.Find(item.ToString()).GetChild(0).GetComponent<Image>().color = red;
        }

        if (gameModeType == GameModeType.MapImg) return;

        foreach (var item in userAnswers)
        {
            GetPanel().transform.Find(item.ToString()).GetComponent<CanvasGroup>().alpha = 1f;
            GetPanel().transform.Find(item.ToString()).localScale = Vector3.one * 1.15f;
        }
    }
}
