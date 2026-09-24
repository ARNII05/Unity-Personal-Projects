using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveName : MonoBehaviour
{
    public TMP_InputField teamNameText;

    public Image teamIcon;

    private void Start()
    {
        PlayerPrefs.DeleteKey("TeamName");
        teamIcon.sprite = Resources.Load<Sprite>($"ConcursilloV2/TeamIcons/TeamIcon{PlayerPrefs.GetInt("TeamImage", 0)}");
    }

    public void SaveTeamName()
    {
        PlayerPrefs.SetString("TeamName", teamNameText.text); 
        PlayerPrefs.Save();
    }

    public void GoBack(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
    
    public void Continue(int sceneIndex)
    {
        if (string.IsNullOrWhiteSpace(teamNameText.text)) return;
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    public void ErasePrefs()
    {
        ConcursilloV2Prefs.DeleteMiniGamePrefs();
    }
}
