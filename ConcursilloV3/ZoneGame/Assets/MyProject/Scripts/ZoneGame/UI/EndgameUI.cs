using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        PersistentNetworkManager.Instance.ApplicationQuit();
    }

    public void GoToMenu()
    {
        PersistentNetworkManager.Instance.ReturnToMainMenu();
    }
}
