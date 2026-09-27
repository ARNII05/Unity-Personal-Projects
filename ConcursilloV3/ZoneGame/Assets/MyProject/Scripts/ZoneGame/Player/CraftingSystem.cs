using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    Player player;

    public const int flowersForBouquet = 5;
    public const int branchesForLog = 5;
    
    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private bool CanCraftBouquet()
    {
        return player.Role.Value == PlayerRole.Gardener &&
            player.inventory.GetItemAmount(ItemType.Flower) >= flowersForBouquet;
    }

    private bool CanCraftLog()
    {
        return player.Role.Value == PlayerRole.Builder &&
            player.inventory.GetItemAmount(ItemType.Branch) >= branchesForLog;
    }

    public void CraftBouquet()
    {
        if (!CanCraftBouquet())
            return;

        player.inventory.AddItem(ItemType.Bouquet, 1);
        player.inventory.RemoveItem(ItemType.Flower, flowersForBouquet);
    }

    public void CraftLog()
    {
        if (!CanCraftLog())
            return;

        player.inventory.AddItem(ItemType.Log, 1);
        player.inventory.RemoveItem(ItemType.Branch, branchesForLog);
    }
}
