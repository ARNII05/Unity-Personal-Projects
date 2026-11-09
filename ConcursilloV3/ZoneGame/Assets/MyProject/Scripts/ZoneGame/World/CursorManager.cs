
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
        Cursor.visible = state switch
        {
            PlayerState.BuffsUI
            or PlayerState.EndGame
            or PlayerState.GrandmaDialogueInteraction
            or PlayerState.Inventory
            or PlayerState.RabbitDialogueInteraction
            or PlayerState.RabbitMapUI
            or PlayerState.SelectingRole
            or PlayerState.TradeUI
            or PlayerState.Trading => true,

            _ => false
        };
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
