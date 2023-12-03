using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class DebugConsole : MonoBehaviour
{
    public bool isDebugMode = true;
    public TMP_Text debugText;
    public int maxLines = 6; // Adjust this to set the maximum number of lines to show

    private Queue<string> messages = new Queue<string>();

    private void Start()
    {
        debugText = this.GetComponent<TMP_Text>();
        if (!isDebugMode)
        {
            gameObject.SetActive(false);
        }
    }

    public void Log(string message)
    {
        if (isDebugMode)
        {
            if (messages.Count >= maxLines)
            {
                messages.Dequeue(); // Remove the oldest message
            }

            messages.Enqueue(message);

            // Update the debugText with the contents of the messages queue
            debugText.text = string.Join("\n", messages.ToArray());
        }
    }
}