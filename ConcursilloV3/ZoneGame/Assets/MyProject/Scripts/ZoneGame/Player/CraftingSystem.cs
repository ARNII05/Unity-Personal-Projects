using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    Player player;

    public const int flowersForBouquet = 20;
    public const int branchesForLog = 12;
    public const int BouquetsToWin = 2;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public bool CanCraftBouquet()
    {
        return player.Role.Value == PlayerRole.Gardener &&
            player.inventory.GetItemAmount(ItemType.Flower) >= flowersForBouquet &&
            player.inventory.HasSpaceForItem(ItemType.Bouquet);
    }

    public bool CanCraftLog()
    {
        return player.Role.Value == PlayerRole.Builder &&
            player.inventory.GetItemAmount(ItemType.Branch) >= branchesForLog &&
            player.inventory.HasSpaceForItem(ItemType.Log);
    }

    public void CraftBouquet()
    {
        if (!CanCraftBouquet())
        {
            Debug.Log("Can't build Bouquet");
            return;
        }

        player.inventory.AddItem(ItemType.Bouquet, 1);
        player.inventory.RemoveItem(ItemType.Flower, flowersForBouquet);
    }

    public void CraftLog()
    {
        if (!CanCraftLog())
        {
            Debug.Log("Can't build Log");
            return;
        }

        player.inventory.AddItem(ItemType.Log, 1);
        player.inventory.RemoveItem(ItemType.Branch, branchesForLog);
    }
}
