using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGameStarted { get; private set; } = false;

    public bool gameEnded = false;

    public float gameTime = 0;

    private Player player;

    public void Init(Player player)
    {
        this.player = player;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Update()
    {
        if (player == null)
            return;

        if (gameEnded || !player.IsServer || !IsGameStarted)
            return;

        gameTime += Time.deltaTime;
    }

    public void MarkGameStarted()
    {
        IsGameStarted = true;
    }
}
