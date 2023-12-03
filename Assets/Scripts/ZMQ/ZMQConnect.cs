using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AsyncIO;

public class ZMQConnect : MonoBehaviour
{
    GameObject Client;

    void Start()
    {
        Client = GameObject.Find("TCPClient");
    }

    public void Connect()
    {
        Client.SetActive(true);
    }
}
