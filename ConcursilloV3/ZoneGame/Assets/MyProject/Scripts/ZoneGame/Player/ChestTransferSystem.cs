
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
        int amount,
        out Vector2Int position)
    {
        position = player.NetworkMapPos.Value;

        if (!ItemVault.IsTransferable(item))
            return false;

        if (amount <= 0 ||
            ZoneChestAmount(position, item) < amount)
            return false;

        bool isBuff = ItemVault.IsBuff(item);

        if (!isBuff && !player.inventory.HasSpaceFor(item))
            return false;

        ZoneData zone = MapManager.Instance.map[
            position.y,
            position.x
        ];

        if (!zone.chestInventory.Remove(item, amount))
            return false;

        if (isBuff)
        {
            player.AddBuff(item);
        }
        else if (!player.inventory.Add(item, amount))
        {
            zone.chestInventory.Add(item, amount);
            return false;
        }

        return true;
    }


    public bool TryToRemoveAncientRelics(ChestType chestType, Vector2Int position)
    {
        if (position != player.NetworkMapPos.Value)
            return false;

        ZoneData zone = MapManager.Instance.map[position.y, position.x];

        if (zone.type != ZoneType.Ruins)
            return false;

        Ruins ruinsZone = (Ruins)zone;

        if (ruinsZone.ancientRelicsRemoved)
            return true;

        if (chestType != ChestType.Ancient)
            return false;

        Player otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        if (otherPlayer == null)
            return false;

        int amountOfAncientRelics = player.inventory.GetAmount(ItemType.AncientRelic) +
            otherPlayer.inventory.GetAmount(ItemType.AncientRelic);

        if (amountOfAncientRelics < CraftingSystem.ancientRelicsToOpenAncientChest)
        {
            Debug.Log("Necesitas al menos 3 Reliquias Antiguas para abrir este cofre.");
            return false;
        }

        RemoveAncientRelicsFromPlayers(otherPlayer);
        ruinsZone.ancientRelicsRemoved = true;

        return true;
    }

    private void RemoveAncientRelicsFromPlayers(Player otherPlayer)
    {
        int player1Relics = player.inventory.GetAmount(ItemType.AncientRelic);
        int player2Relics = otherPlayer.inventory.GetAmount(ItemType.AncientRelic);

        if (player1Relics > 0)
            player.inventory.Remove(ItemType.AncientRelic, player1Relics);

        if (player2Relics > 0)
            otherPlayer.inventory.Remove(ItemType.AncientRelic, player2Relics);
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