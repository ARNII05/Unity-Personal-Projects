using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Selector : MonoBehaviour
{
    public void SelectGameMode(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
