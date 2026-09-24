using UnityEngine;

public partial class GameController
{
    void TimeControl(float deltaTime)
    {
        currentTime -= deltaTime;

        if (currentTime <= 0)
        {
            timeIsUp = true;
            CheckAnswerWithActualMode();
            return;
        }

        SetTimer();

        AdviceAt(26);
    }

    void SetTimer()
    {
        float minutes = Mathf.Floor(currentTime / 60f);
        float seconds = Mathf.Floor(currentTime % 60f);

        timer.text = $"{minutes}:{seconds:00} min";
    }

    void AdviceAt(float seconds)
    {
        if (overtimeAdvicePlayed || currentTime > seconds) return;

        overtimeAdvicePlayed = true;
        musicManager.PlayMusic(overTimeMusic);
    }
}
