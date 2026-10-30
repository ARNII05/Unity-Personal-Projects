using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SwampDialogue))]
public class SwampNetwork : NetworkBehaviour
{
    public static SwampNetwork Instance { get; private set; }

    private SwampDialogue swampDialogue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        swampDialogue = GetComponent<SwampDialogue>();
    }

    public void TryToGetStolenItems(int probToRecoverItem, Vector2Int swampPos, ulong id, bool stolenItemsIsEmpty)
    {
        if (!IsServer)
            return;
        
        Player player = MapManager.Instance.GetPlayerById(id);

        if (player == null)
            return;

        ZoneData[,] map = MapManager.Instance.map;
        Swamp swamp = (Swamp)map[swampPos.y, swampPos.x];

        LootToRecover lootRecovered = LootRecovered(probToRecoverItem, swampPos, player, stolenItemsIsEmpty);

        if (lootRecovered.recoveryState == RecoveryState.ItemRecovered)
        {
            swamp.internalInventory[lootRecovered.item] -= lootRecovered.amount;

            if (swamp.internalInventory[lootRecovered.item] <= 0)
                swamp.internalInventory.Remove(lootRecovered.item);

            player.inventory.Add(lootRecovered.item, lootRecovered.amount);
            player.SetInventory();
        }

        ShowRecoveredItemToUserClientRpc(lootRecovered.item, lootRecovered.amount, lootRecovered.recoveryState, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));
    }

    private LootToRecover LootRecovered(int probToRecoverItem, Vector2Int swampPos, Player player, bool stolenItemsIsEmpty)
    {
        if (stolenItemsIsEmpty)
            return new LootToRecover(ItemType.None, 0, RecoveryState.NoItemsStolen);

        ZoneData[,] map = MapManager.Instance.map;
        Swamp swamp = (Swamp)map[swampPos.y, swampPos.x];

        if (swamp.internalInventory.Count == 0)
            return new LootToRecover(ItemType.None, 0, RecoveryState.NoItemsToRecover);

        int finalItemProb = Random.Range(0, 100);

        if (finalItemProb >= probToRecoverItem)
            return new LootToRecover(ItemType.None, 0, RecoveryState.ProbToRecoverFailed);

        ItemType[] validItems = swamp.internalInventory.Keys
                .Where(item => player.inventory.HasSpaceFor(item))
                .ToArray();

        if (validItems.Length == 0)
            return new LootToRecover(ItemType.None, 0, RecoveryState.NoSpaceInInventory);

        int itemIndex = Random.Range(0, validItems.Length);
        ItemType itemType = validItems[itemIndex];

        int amountToRecover = Random.Range(1, swamp.internalInventory[itemType] + 1);

        return new LootToRecover(itemType, amountToRecover, RecoveryState.ItemRecovered);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ShowRecoveredItemToUserClientRpc(ItemType item, int amount, RecoveryState recoveryState, RpcParams rpcParams = default)
    {
        swampDialogue.ShowRecoveredItem(item, amount, recoveryState);
    }

    public IEnumerator ShowUIToUserCoroutine(ItemType[] items, int[] amount, Player player)
    {
        while (player.State.Value == PlayerState.Transitioning)
        {
            yield return null;
        }

        ShowItemsStolenToUserClientRpc(items, amount, RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Temp));

        player.SetNewStateRpc(PlayerState.InSwamp);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void ShowItemsStolenToUserClientRpc(ItemType[] items, int[] amount, RpcParams rpcParams = default)
    {
        swampDialogue.ShowStolenItems(items, amount);
    }

    public void CloseUI(Player player)
    {
        swampDialogue.Close();
        
        player.SetNewStateRpc(PlayerState.Normal);
    }

    private struct LootToRecover
    {
        public ItemType item;
        public int amount;
        public RecoveryState recoveryState;

        public LootToRecover(ItemType item, int amount, RecoveryState recoveryState)
        {
            this.item = item;
            this.amount = amount;
            this.recoveryState = recoveryState;
        }
    }
}

public enum RecoveryState
{
    NoItemsStolen,
    NoItemsToRecover,
    ProbToRecoverFailed,
    NoSpaceInInventory,
    ItemRecovered
}