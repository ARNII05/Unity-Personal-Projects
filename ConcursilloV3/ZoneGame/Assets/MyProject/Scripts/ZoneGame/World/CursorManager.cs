
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UpdateCursor(PlayerState state)
    {
        bool showCursor = state switch
        {
            PlayerState.BuffsUI
            or PlayerState.EndGame
            or PlayerState.GrandmaDialogueInteraction
            or PlayerState.Inventory
            or PlayerState.RabbitDialogueInteraction
            or PlayerState.RabbitMapUI
            or PlayerState.SelectingRole
            or PlayerState.Trade
            or PlayerState.PauseMenu
            or PlayerState.ChestInventory => true,

            _ => false
        };

        Cursor.lockState = showCursor
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = showCursor;
    }

    public void ShowCursor()
    {
        Cursor.visible = true;
    }

    public void HideCursor()
    {
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
