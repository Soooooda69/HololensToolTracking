using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ZMQPublisherHelper : MonoBehaviour
{
    public TMP_Text Port;

    public string[] GetPublisherInfo()
    {
        return new string[] { Port.text[..^1] };
    }
}
