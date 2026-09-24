using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Chooser : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip chooseTeamSound;

    private void Start()
    {
        PlayerPrefs.DeleteKey("TeamPath");
    }

    public void ChooseTeam(int team)
    {
        SoundEffectsManager.EnsureInstance().PlaySound(chooseTeamSound);

        switch (team)
        {
            case 0:
                PlayerPrefs.SetString("TeamPath", "AdeTeam");
                break;
            case 1:
                PlayerPrefs.SetString("TeamPath", "DylanTeam");
                break;
            case 2:
                PlayerPrefs.SetString("TeamPath", "PatryTeam");
                break;
        }

        PlayerPrefs.SetInt("TeamImage", team + 1);
        PlayerPrefs.Save();
    }

    public void GoBack(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    public void Continue(int sceneIndex)
    {
        if (PlayerPrefs.GetString("TeamPath", "") == "") return;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    public void ErasePrefs()
    {
        ConcursilloV2Prefs.DeleteMiniGamePrefs();
    }
}
