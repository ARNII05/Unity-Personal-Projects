
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
            player.inventory.GetItemAmount(item) < amount)
            return false;

        ZoneData zone = MapManager.Instance.map[
            position.y,
            position.x
        ];

        if (!zone.chestInventory.HasSpaceForItem(item))
            return false;

        if (!zone.chestInventory.AddItem(item, amount))
            return false;

        if (!player.inventory.RemoveItem(item, amount))
        {
            zone.chestInventory.RemoveItem(item, amount);
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

        if (!player.inventory.HasSpaceForItem(item))
            return false;

        ZoneData zone = MapManager.Instance.map[
            position.y,
            position.x
        ];

        if (!player.inventory.AddItem(item, amount))
            return false;

        if (!zone.chestInventory.RemoveItem(item, amount))
        {
            player.inventory.RemoveItem(item, amount);
            return false;
        }

        return true;
    }

    private int ZoneChestAmount(Vector2Int position, ItemType item)
    {
        ZoneData zone = MapManager.Instance.map[
            position.y,
            position.x
        ];

        return zone.chestInventory.GetItemAmount(item);
    }
}