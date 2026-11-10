
using UnityEngine;

[RequireComponent(typeof(Player))]
public class ChestTransferSystem : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public bool TrySendItemsToChest(
        ItemType item,
        int amount,
        out Vector2Int position)
    {
        position = player.NetworkMapPos.Value;

        if (!ItemVault.IsTransferable(item))
            return false;

        if (amount <= 0 ||
            player.inventory.GetAmount(item) < amount)
            return false;

        ZoneData zone = MapManager.Instance.map[
            position.y,
            position.x
        ];

        if (!zone.chestInventory.HasSpaceFor(item))
            return false;

        if (!zone.chestInventory.Add(item, amount))
            return false;

        if (!player.inventory.Remove(item, amount))
        {
            zone.chestInventory.Remove(item, amount);
            return false;
        }

        return true;
    }

    public bool TrySendItemsToPlayer(
    ItemType item,
    ref int amount,
    out Vector2Int position)
    {
        position = player.NetworkMapPos.Value;

        if (!ItemVault.IsTransferable(item) || amount <= 0)
            return false;

        bool isBuff = ItemVault.IsBuff(item);

        if (isBuff)
        {
            amount = 1;

            if (player.inventory.activeBuffs.Contains(item) ||
                player.inventory.activeBuffs.Count >= Inventory.maxBuffs)
                return false;
        }

        if (ZoneChestAmount(position, item) < amount)
            return false;

        ZoneData zone = MapManager.Instance.map[
            position.y,
            position.x
        ];

        if (!zone.chestInventory.Remove(item, amount))
            return false;

        if (isBuff)
        {
            if (!player.AddBuff(item))
            {
                zone.chestInventory.Add(item, amount);
                return false;
            }
        }
        else if (!player.inventory.Add(item, amount))
        {
            zone.chestInventory.Add(item, amount);
            return false;
        }

        return true;
    }


    public AncientRelicsResult TryToRemoveAncientRelics(
        ChestType chestType,
        Vector2Int position,
        out int amountOfAncientRelics)
    {
        amountOfAncientRelics = 0;

        if (position != player.NetworkMapPos.Value)
            return AncientRelicsResult.InvalidInteraction;

        ZoneData zone = MapManager.Instance.map[position.y, position.x];

        if (zone.type != ZoneType.Ruins)
            return AncientRelicsResult.InvalidInteraction;

        Ruins ruinsZone = (Ruins)zone;

        if (ruinsZone.ancientRelicsRemoved)
            return AncientRelicsResult.Success;

        if (chestType != ChestType.Ancient)
            return AncientRelicsResult.InvalidInteraction;

        Player otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        if (otherPlayer == null)
            return AncientRelicsResult.InvalidInteraction;

        amountOfAncientRelics =
            player.inventory.GetAmount(ItemType.AncientRelic) +
            otherPlayer.inventory.GetAmount(ItemType.AncientRelic);

        if (amountOfAncientRelics < CraftingSystem.ancientRelicsToOpenAncientChest)
            return AncientRelicsResult.NotEnoughRelics;

        RemoveAncientRelicsFromPlayers(otherPlayer);
        ruinsZone.ancientRelicsRemoved = true;

        return AncientRelicsResult.Success;
    }

    private void RemoveAncientRelicsFromPlayers(Player otherPlayer)
    {
        int amountToRemove = CraftingSystem.ancientRelicsToOpenAncientChest;
        
        int player1Relics = player.inventory.GetAmount(ItemType.AncientRelic);

        int relicsToRemoveFromPlayer1 = Mathf.Min(player1Relics, amountToRemove);
        int relicsToRemoveFromPlayer2 = amountToRemove - relicsToRemoveFromPlayer1;

        if (relicsToRemoveFromPlayer1 > 0)
            player.inventory.Remove(ItemType.AncientRelic, relicsToRemoveFromPlayer1);

        if (relicsToRemoveFromPlayer2 > 0)
            otherPlayer.inventory.Remove(ItemType.AncientRelic, relicsToRemoveFromPlayer2);
    }

    private int ZoneChestAmount(Vector2Int position, ItemType item)
    {
        ZoneData zone = MapManager.Instance.map[
            position.y,
            position.x
        ];

        return zone.chestInventory.GetAmount(item);
    }
}

public enum AncientRelicsResult
{
    Success,
    NotEnoughRelics,
    InvalidInteraction
}