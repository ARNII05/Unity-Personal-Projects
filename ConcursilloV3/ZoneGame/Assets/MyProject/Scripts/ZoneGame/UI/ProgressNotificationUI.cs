using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ProgressNotificationUI : NetworkBehaviour
{
    public static ProgressNotificationUI Instance { get; private set; }

    private bool bouquetNotificationShown;
    private bool bridgeNotificationShown;

    private Player player, otherPlayer;

    private GameObject progressionNotificationUI;

    private TMP_Text notiText;

    private readonly Queue<(string msg, float time)> notificationQueue = new();
    private Coroutine notificationCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        progressionNotificationUI = transform.Find("ProgressionNotificationUI").gameObject;

        notiText = progressionNotificationUI.transform.Find("NotiText").GetComponent<TMP_Text>();

        progressionNotificationUI.SetActive(false);
    }

    public void Init(Player player)
    {
        if (!IsServer)
            return;

        this.player = player;

        player.inventory.OnInventoryChanged += CheckProgressNotifications;

        StartCoroutine(WaitForOtherPlayer());
    }

    private IEnumerator WaitForOtherPlayer()
    {
        yield return new WaitUntil(() =>
        {
            if (MapManager.Instance == null)
                return false;

            otherPlayer = MapManager.Instance.GetOtherPlayer(player);

            return otherPlayer != null;
        });

        otherPlayer.inventory.OnInventoryChanged += CheckProgressNotifications;
    }

    private void CheckProgressNotifications()
    {
        if (!bouquetNotificationShown && HasEnoughBouquets())
        {
            bouquetNotificationShown = true;
            ShowProgressNotificationRpc(ProgressNotification.BouquetsReady);
        }

        if (!bridgeNotificationShown && HasEnoughLogsForBridge())
        {
            bridgeNotificationShown = true;
            ShowProgressNotificationRpc(ProgressNotification.BridgeReady);
        }
    }

    private bool HasEnoughBouquets()
    {
        int necessaryBouquets = CraftingSystem.BouquetsToWin;

        int bouquets = player.inventory.GetAmount(ItemType.Bouquet) +
            otherPlayer.inventory.GetAmount(ItemType.Bouquet);

        int flowers = player.inventory.GetAmount(ItemType.Flower) +
            otherPlayer.inventory.GetAmount(ItemType.Flower);

        int totalBouquets = bouquets + flowers / CraftingSystem.flowersForBouquet;

        return totalBouquets >= necessaryBouquets;
    }

    private bool HasEnoughLogsForBridge()
    {
        int necessaryLogs = CraftingSystem.logsForBridge;

        int logs = player.inventory.GetAmount(ItemType.Log) +
            otherPlayer.inventory.GetAmount(ItemType.Log);

        int branches = player.inventory.GetAmount(ItemType.Branch) +
            otherPlayer.inventory.GetAmount(ItemType.Branch);

        int totalBouquets = logs + branches / CraftingSystem.branchesForLog;

        return totalBouquets >= necessaryLogs;
    }

    [Rpc(SendTo.Everyone)]
    private void ShowProgressNotificationRpc(ProgressNotification progressNotification)
    {
        ShowProgressNotification(progressNotification);
    }

    private void ShowProgressNotification(ProgressNotification progressNotification)
    {
        string text = progressNotification switch
        {
            ProgressNotification.BouquetsReady => "¡Ya tenéis los dos ramos!\nBuscad la casa de la abuela y entregádselos.",
            _ => "¡Ya podéis construir un puente!\nEl Constructor tiene los troncos necesarios para cruzar un río."
        };

        ShowNotificationMsg(text);
    }

    private void ShowNotificationMsg(string msg, float time = 4f)
    {
        notificationQueue.Enqueue((msg, time));

        notificationCoroutine ??= StartCoroutine(ProcessNotificationQueue());
    }

    private IEnumerator ProcessNotificationQueue()
    {
        while (notificationQueue.Count > 0)
        {
            var (msg, time) = notificationQueue.Dequeue();

            progressionNotificationUI.SetActive(true);
            notiText.text = msg;

            yield return new WaitForSeconds(time);

            progressionNotificationUI.SetActive(false);
            notiText.text = string.Empty;
        }

        notificationCoroutine = null;
    }

    private enum ProgressNotification
    {
        BouquetsReady,
        BridgeReady
    }
}
