using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataBuffer : MonoBehaviour
{
    public Dictionary<string, byte[]> topicMsg = new Dictionary<string, byte[]> { };

    public void UpdateOrAddMessage(string topic, byte[] msg)
    {
        if (topicMsg.ContainsKey(topic))
        {
               topicMsg[topic] = msg;
        }
        else
        {
            topicMsg.Add(topic, msg);
        }
    }

    public byte[] GetMessage(string topic)
    {
        if (topicMsg.TryGetValue(topic, out byte[] msg))
        {
            return msg;
        }
        return null;
    }

}
