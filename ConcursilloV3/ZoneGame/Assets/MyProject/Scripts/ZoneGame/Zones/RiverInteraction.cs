using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverInteraction : MonoBehaviour
{
    public bool SendInteractToRiver(Player player)
    {
        River river = GetComponentInParent<River>();

        if (river == null)
            return false;

        return river.Interact(player);
    }
}
