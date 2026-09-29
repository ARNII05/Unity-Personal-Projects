using Unity.Netcode;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public Inventory inventory = new();
    public ChestUI chestUI;

    public void Init(ChestUI chestUI)
    {
        this.chestUI = chestUI;
    }

    public void OnOpen(Player player)
    {
        chestUI.InitPlayer(player);
        chestUI.ToggleInventory();
    }
}