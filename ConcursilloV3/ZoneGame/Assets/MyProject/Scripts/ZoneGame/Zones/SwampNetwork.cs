using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class SwampNetwork : NetworkBehaviour
{
    public static SwampNetwork Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    [Rpc(SendTo.Server)]
    public void TryToGetStolenItemsRpc(int probToRecoverItem, Vector2Int swampPos, RpcParams rpcParams = default)
    {
        ulong id = rpcParams.Receive.SenderClientId;
        Player player = MapManager.Instance.GetPlayerById(id);

        if (player == null)
            return;

        ZoneData[,] map = MapManager.Instance.map;
        Swamp swamp = (Swamp)map[swampPos.y, swampPos.x];

        if (!player.CanRecoverSwampItems())
        {
            Debug.Log("No puedes recuperar más items");
            //Mostrar que no tiene más intentos para recuperar
            return;
        }

        player.SetCanRecoverSwampItems(false);

        if (swamp.internalInventory.Count == 0)
        {
            Debug.Log("No quedan objetos por recuperar");
            // Mostrar que no quedan objetos por recuperar.
            return;
        }

        int finalItemProb = Random.Range(0, 100);

        if (finalItemProb >= probToRecoverItem)
        {
            Debug.Log($"{player.name} no ha recuperado ningun item");
            //Mensaje de que no ha recuperado ningun item
            return;
        }

        ItemType[] validItems = swamp.internalInventory.Keys
                .Where(item => player.inventory.HasSpaceFor(item))
                .ToArray();

        if (validItems.Length == 0)
        {
            Debug.Log("No hay espacio en el inventario");
            //Mostrar que no hay espacio en el inventario
            return;
        }

        int itemIndex = Random.Range(0, validItems.Length);
        ItemType itemType = validItems[itemIndex];

        int amountToRecover = Random.Range(1, swamp.internalInventory[itemType] + 1);

        swamp.internalInventory[itemType] -= amountToRecover;

        if (swamp.internalInventory[itemType] <= 0)
            swamp.internalInventory.Remove(itemType);

        player.inventory.Add(itemType, amountToRecover);
        player.SetInventory();

        ShowRecoveredItemToUserClientRpc(itemType, amountToRecover, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ShowRecoveredItemToUserClientRpc(ItemType item, int amount, RpcParams rpcParams = default)
    {
        Debug.Log($"Has recuperado: {amount} {item}");
        //Enseñar lo que ha recuperado
    }

    /*
    private bool SpaceInSwamp(ItemType item, Vector2Int swampPos)
    {
        ZoneData[,] map = MapManager.Instance.map;

        ZoneData swamp = map[swampPos.y, swampPos.x];

        return swamp.chestInventory.HasSpaceFor(item);
    }
    
    [Rpc(SendTo.Everyone)]
    private void UpdateSwampZoneClientRpc(ItemType item, int amount, Vector2Int position)
    {
        ZoneData[,] map = MapManager.Instance.map;
        
        Swamp swamp = (Swamp)map[position.y, position.x];

        swamp.chestInventory.Add(item, amount);
    }
    */

    public IEnumerator ShowUIToUserCoroutine(ItemType[] items, int[] amount, Player player)
    {
        while (player.State.Value == PlayerState.Transitioning)
        {
            yield return null;
        }
        
        ShowItemsStolenToUserClientRpc(items, amount, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ShowItemsStolenToUserClientRpc(ItemType[] items, int[] amount, RpcParams rpcParams = default)
    {
        for (int i = 0; i < items.Length; i++)
        {
            Debug.Log($"Robado por pantano: {amount[i]} {items[i]}");
        }
        //Enseñar lo que le ha robado
    }
}