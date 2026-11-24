
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerNearbyObjects : MonoBehaviour
{
    public BorderZone CurrentBorderZone { get; private set; }
    public Chest NearbyChest { get; private set; }

    public GameObject Rabbit { get; private set; }
    public GameObject NearbyGrandma { get; private set; }
    public GameObject CurrentRiverInteraction { get; private set; }

    public GameObject tradeCollider { get; private set; }

    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Border":
                if (!player.IsOwner)
                    break;

                CurrentBorderZone = other.GetComponent<BorderZone>();

                if (player.LocalState == PlayerState.Normal)
                    player.ChangeZone();

                break;

            case "Chest":
                NearbyChest = other.GetComponentInParent<Chest>();
                break;

            case "RiverInteractor":
                CurrentRiverInteraction = other.gameObject;
                break;

            case "Grandma":
                NearbyGrandma = other.gameObject;
                break;

            case "Rabbit":
                Rabbit = other.gameObject;
                break;

            case "Trade":
                tradeCollider = other.gameObject;
                break;
        }

        UpdateInteractionTooltip();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Border":
                if (CurrentBorderZone == other.GetComponent<BorderZone>())
                    CurrentBorderZone = null;
                break;

            case "Chest":
            {
                Chest exitingChest = other.GetComponentInParent<Chest>();

                if (NearbyChest == exitingChest)
                    NearbyChest = null;

                break;
            }

            case "RiverInteractor":
                CurrentRiverInteraction = null;
                break;

            case "Grandma":
                NearbyGrandma = null;
                break;

            case "Rabbit":
                Rabbit = null;
                break;

            case "Trade":
                tradeCollider = null;
                break;
        }

        UpdateInteractionTooltip();
    }

    public void UpdateInteractionTooltip()
    {
        if (player == null || !player.IsOwner ||
            InteractionTooltipUI.Instance == null)
            return;

        string message = GetInteractionMessage();

        if (string.IsNullOrEmpty(message))
            InteractionTooltipUI.Instance.Clear();
        else
            InteractionTooltipUI.Instance.Show(message);
    }

    private string GetInteractionMessage()
    {
        if (CurrentRiverInteraction != null)
            return "E - Construir puente";

        if (NearbyChest != null)
            return "E - Abrir cofre";

        if (NearbyGrandma != null)
            return "E - Hablar con la abuela";

        if (Rabbit != null)
            return "E - Hablar con el conejo";

        if (tradeCollider != null)
            return "E - Comerciar";

        return null;
    }

    public void ClearNearbyObjects()
    {
        CurrentBorderZone = null;
        NearbyChest = null;
        Rabbit = null;
        NearbyGrandma = null;
        CurrentRiverInteraction = null;
        tradeCollider = null;

        UpdateInteractionTooltip();
    }
}