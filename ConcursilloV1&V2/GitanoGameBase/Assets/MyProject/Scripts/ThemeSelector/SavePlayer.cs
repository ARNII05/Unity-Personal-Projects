using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SavePlayer : MonoBehaviour
{
    string[] playerNames = { "DylanThemes", "VeriThemes", "FeksThemes", "DaniThemes", "PatryThemes", "EmilioThemes", "AdeThemes" };

    public void SaveName(string playerName)
    {
        PlayerPrefs.SetString("Selected", playerName);
        PlayerPrefs.SetInt($"{playerName}Index", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(6);
    }
    
    public void SaveImg(int index)
    {
        PlayerPrefs.SetInt("imgIndex", index);
        PlayerPrefs.Save();
    }

    public void GamblePlayer()
    {
        if (DidAllPlayers())
            return;

        int index = Random.Range(0, playerNames.Length);

        while (PlayerPrefs.GetInt($"{playerNames[index]}Index") == 1)
            index = Random.Range(0, playerNames.Length);

        PlayerPrefs.SetInt($"{playerNames[index]}Index", 1);
        PlayerPrefs.SetInt("imgIndex", index);
        PlayerPrefs.SetString("Selected", playerNames[index]);
        SceneManager.LoadScene(6);
    }

    bool DidAllPlayers()
    {

        for (int i = 0; i < playerNames.Length; i++)
        {
            if (PlayerPrefs.GetInt($"{playerNames[i]}Index") == 0)
                return false;
        }

        return true;
    }

    public void DeletePlayerPrefs()
    {
        string[] playerNamesPoints = { "Dylan", "Veri", "Feks", "Dani", "Patry", "Emilio", "Ade" };

        for (int i = 0; i < playerNames.Length; i++)
        {
            PlayerPrefs.DeleteKey($"{playerNames[i]}Index");
            PlayerPrefs.DeleteKey($"{playerNamesPoints[i]}");
        }
    }

    public void Swap(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}