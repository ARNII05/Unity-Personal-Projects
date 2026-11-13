using UnityEngine;

public class PanelManager : MonoBehaviour
{
    private void Awake()
    {
        foreach (Transform child in transform)
        {
            if (child.name.EndsWith("Panel"))
                child.gameObject.SetActive(true);
        }
    }
}