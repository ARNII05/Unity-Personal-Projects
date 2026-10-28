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

    public const int logsForBridge = 1;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public bool CanCraftBouquet()
    {
        return player.Role.Value == PlayerRole.Gardener &&
            player.inventory.GetAmount(ItemType.Flower) >= flowersForBouquet &&
            player.inventory.HasSpaceFor(ItemType.Bouquet);
    }

    public bool CanCraftLog()
    {
        return player.Role.Value == PlayerRole.Builder &&
            player.inventory.GetAmount(ItemType.Branch) >= branchesForLog &&
            player.inventory.HasSpaceFor(ItemType.Log);
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

        if (!player.inventory.Add(itemToAdd, 1))
            return false;

        if (!player.inventory.Remove(itemToRemove, amountToRemove))
        {
            player.inventory.Remove(itemToAdd, 1);
            return false;
        }

        return true;
    }
}