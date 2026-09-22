using Unity.Netcode;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public void OnOpen(Player player)
    {
        int itemsToGenerate = Random.Range(1, 4);

        for (int i = 0; i < itemsToGenerate; i++)
        {
            ItemType randomItemType = (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length);
            
            Items a = new Coin()
            {
                amount = Random.Range(1, 4)
            };

            player.inventory.AddItem(a.itemType, a.amount);

            Items generatedItem = ItemVault.GenerateItem(randomItemType);
            generatedItem.amount = Random.Range(1, 4);

            player.inventory.AddItem(generatedItem.itemType, generatedItem.amount);
        }
    }
}