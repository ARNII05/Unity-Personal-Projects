using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grandma : MonoBehaviour
{
    public void Interact(Player player)
    {
        int bouquetCount = player.inventory.GetItemAmount(ItemType.Bouquet);

        if (bouquetCount == 0)
        {
            Debug.Log("No bouquets.");
            return;
        }

        player.inventory.RemoveItem(ItemType.Bouquet, bouquetCount);

        player.UpdateZoneBouquetsServerRpc(bouquetCount);

        CanWin(player);
    }

    public void CanWin(Player player)
    {
        Player otherPlayer = MapManager.Instance.GetOtherPlayer(player);

        ZoneData zoneData = MapManager.Instance.map[player.NetworkMapPos.Value.y, player.NetworkMapPos.Value.x];

        if (zoneData.bouquetsInInventory < CraftingSystem.BouquetsToWin)
        {
            Debug.Log($"Not enough bouquets in Grandma's inventory to win." +
                $"\nZone bouquets inventory: {zoneData.bouquetsInInventory}" +
                $"\nBouquets to win: {CraftingSystem.BouquetsToWin}");
            return;
        }

        if (player.NetworkMapPos.Value != otherPlayer.NetworkMapPos.Value)
        {
            Debug.Log("Players are not in the same position, cannot win.");
            return;
        }

        player.ShowEndGanePanelServerRpc();
    }
}
