using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
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
            return;

        player.CraftCoreItemServerRpc();
    }

    public void CraftLog()
    {
        if (!CanCraftLog())
            return;

        player.CraftCoreItemServerRpc();
    }

    public bool TryCraftCoreItem()
    {
        if (player.Role.Value != PlayerRole.Gardener &&
            player.Role.Value != PlayerRole.Builder)
            return false;

        ItemType itemToRemove = player.Role.Value == PlayerRole.Gardener
            ? ItemType.Flower
            : ItemType.Branch;

        int amountToRemove = player.Role.Value == PlayerRole.Gardener
            ? flowersForBouquet
            : branchesForLog;

        ItemType itemToAdd = player.Role.Value == PlayerRole.Gardener
            ? ItemType.Bouquet
            : ItemType.Log;

        bool canCraft = player.Role.Value == PlayerRole.Gardener
            ? CanCraftBouquet()
            : CanCraftLog();

        if (!canCraft)
            return false;

        if (!player.inventory.AddItem(itemToAdd, 1))
            return false;

        if (!player.inventory.RemoveItem(itemToRemove, amountToRemove))
        {
            player.inventory.RemoveItem(itemToAdd, 1);
            return false;
        }

        return true;
    }
}