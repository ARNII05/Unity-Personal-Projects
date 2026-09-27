using Unity.Netcode;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public ChestItems OnOpen()
    {   
        int itemsToGenerate = Random.Range(1, 4);

        ChestItems result = new(new ItemType[itemsToGenerate], new int[itemsToGenerate]);

        for (int i = 0; i < itemsToGenerate; i++)
        {
            ItemType randomItemType = (ItemType)Random.Range(0, System.Enum.GetValues(typeof(ItemType)).Length);
            
            Items generatedItem = ItemVault.GenerateItem(randomItemType);
            generatedItem.amount = Random.Range(1, 4);

            result.itemTypes[i] = randomItemType;
            result.amounts[i] = generatedItem.amount;
        }

        return result;
    }
}

public struct ChestItems
{
    public ItemType[] itemTypes;
    public int[] amounts;

    public ChestItems(ItemType[] itemTypes, int[] amounts)
    {
        this.itemTypes = itemTypes;
        this.amounts = amounts;
    }
}