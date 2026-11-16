using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInputHandler : MonoBehaviour
{
    private Player player;
    private ZoneGameControls controls;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public void Initialize(ZoneGameControls playerControls)
    {
        controls = playerControls;
    }

    public void HandleInput()
    {
        if (controls == null)
            return;

        if (ClosePanelsWithEsc())
            return;

        switch (player.State.Value)
        {
            case PlayerState.Normal:
                HandleNormalInput();
                break;

            case PlayerState.ChestInventory:
                if (controls.Gameplay.Interact.WasPressedThisFrame())
                    player.CloseChest();
                break;

            case PlayerState.Inventory:
                if (controls.Gameplay.Inventory.WasPressedThisFrame())
                    player.CloseInventory();
                break;

            case PlayerState.BuffsUI:
                if (controls.Gameplay.Buffs.WasPressedThisFrame())
                    player.CloseBuffsUI();
                break;

            case PlayerState.UsingMap:
                if (controls.Gameplay.InitialObj.WasPressedThisFrame())
                    MapUI.Instance.ToggleMap();
                break;

            case PlayerState.UsingRadar:
                if (controls.Gameplay.InitialObj.WasPressedThisFrame())
                    RadarUI.Instance.ToggleRadar();
                break;
        }
    }

    private bool ClosePanelsWithEsc()
    {
        if (!controls.Gameplay.Esc.WasPressedThisFrame())
            return false;

        switch (player.State.Value)
        {
            case PlayerState.Normal:
                player.OpenPauseMenu();
                break;

            case PlayerState.PauseMenu:
                player.ClosePauseMenu();
                break;

            case PlayerState.ChestInventory:
                player.CloseChest();
                break;

            case PlayerState.Inventory:
                player.CloseInventory();
                break;

            case PlayerState.BuffsUI:
                player.CloseBuffsUI();
                break;

            case PlayerState.UsingMap:
                MapUI.Instance.ToggleMap();
                break;

            case PlayerState.UsingRadar:
                RadarUI.Instance.ToggleRadar();
                break;

            case PlayerState.RabbitMapUI:
                RabbitDialogue.Instance.CloseMap();
                break;

            case PlayerState.CaughtByWolf:
                player.CloseWolfUI();
                break;

            case PlayerState.Trade:
                TradeUI.Instance.CloseUI();
                break;

            case PlayerState.InSwamp:
                SwampNetwork.Instance.CloseUI(player);
                break;

            case PlayerState.AncientChestUI:
                AncientChestUI.Instance.HidePanel();
                break;
        }

        return true;
    }

    private void HandleNormalInput()
    {
        if (controls.Gameplay.InitialObj.WasPressedThisFrame())
            HandleTabInput();

        if (controls.Gameplay.Interact.WasPressedThisFrame())
            HandleInteractionInput();

        if (controls.Gameplay.Inventory.WasPressedThisFrame())
            player.OpenInventory();

        if (controls.Gameplay.Buffs.WasPressedThisFrame())
            player.OpenBuffsUI();
    }

    private void HandleTabInput()
    {
        if (player.inventory.GetAmount(ItemType.Radar) > 0)
        {
            RadarUI.Instance.ToggleRadar();
            return;
        }

        if (player.inventory.GetAmount(ItemType.Map) > 0)
        {
            MapUI.Instance.ToggleMap();
        }
    }

    private void HandleInteractionInput()
    {
        if (player.NearbyObjects.CurrentRiverInteraction != null)
        {
            player.InteractWithRiver();
            return;
        }

        if (player.NearbyObjects.CurrentBorderZone != null)
        {
            player.ChangeZone();
            return;
        }

        if (player.NearbyObjects.NearbyChest != null)
        {
            player.OpenChest();
            return;
        }

        if (player.NearbyObjects.NearbyGrandma != null)
        {
            player.InteractWithGrandma();
            return;
        }

        if (player.NearbyObjects.Rabbit != null)
        {
            RabbitDialogue.Instance.OpenDialogue();
        }

        if (player.NearbyObjects.tradeCollider != null)
        {
            player.InteractWithTrade();
            return;
        }
    }
}