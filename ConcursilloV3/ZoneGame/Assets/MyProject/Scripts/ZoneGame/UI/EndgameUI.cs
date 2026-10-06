using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndgameUI : MonoBehaviour
{
    public static EndgameUI Instance { get; private set; }
    
    [SerializeField] private GameObject endGamePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        endGamePanel.SetActive(false);
    }

    public void ShowEndGamePanel()
    {
        endGamePanel.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void GoToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
