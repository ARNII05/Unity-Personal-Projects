
using UnityEngine;

public class PlayerNearbyObjects : MonoBehaviour
{
    public BorderZone CurrentBorderZone { get; private set; }
    public Chest NearbyChest { get; private set; }

    public GameObject Rabbit { get; private set; }
    public GameObject NearbyGrandma { get; private set; }
    public GameObject CurrentRiverInteraction { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Border":
                CurrentBorderZone = other.GetComponent<BorderZone>();
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
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        switch (other.tag)
        {
            case "Border":
                CurrentBorderZone = null;
                break;

            case "Chest":
                NearbyChest = null;
                break;

            case "RiverInteractor":
                CurrentRiverInteraction = null;
                break;

            case "Grandma":
                NearbyGrandma = null;
                break;

            case "Rabbit":
                Rabbit = null;
                break;
        }
    }
}