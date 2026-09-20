using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; private set; }
    public GameObject logs;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<UIManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void WriteOnLogs(string text)
    {
        TextMeshProUGUI logsText = logs.GetComponent<TextMeshProUGUI>();

        logsText.text = text;
    }
}
