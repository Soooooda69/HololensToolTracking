using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class ZMQSubscriber
{
    public string[] Topics { get; private set; }
    public string IP { get; private set; }
    public string Port { get; private set; }
    public SubscriberSocket subSocket;
    private NetMQPoller poller;
    private List<byte[]> buffer;
    private DataBuffer databuffer;

    public ZMQSubscriber(string ip, string port, string[] topicArray, DataBuffer databuffer)
    {
        IP = ip;
        Port = port;
        Topics = topicArray;
        subSocket = new SubscriberSocket();
        subSocket.Options.ReceiveHighWatermark = 1000;
        subSocket.Connect($"tcp://{IP}:{Port}");
        foreach (string topic in Topics)
        {
            subSocket.Subscribe(Encoding.UTF8.GetBytes(topic));
        }
        subSocket.ReceiveReady += OnReceiveReady;
        this.databuffer = databuffer;
    }

    public SubscriberSocket GetSubscriberSocket()
    {
        return subSocket;
    }

    private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
    {
        if (subSocket.TryReceiveMultipartBytes(TimeSpan.FromSeconds(0.1), ref buffer))
        {
            var receivedTopic = Encoding.UTF8.GetString(buffer[0]);
            var receivedMessage = buffer[1];
            MainThreadDispatcher.Enqueue(() => ProcessMessage(receivedTopic, receivedMessage));
        }
    }

    private void ProcessMessage(string topic, byte[] message)
    {
        databuffer.UpdateOrAddMessage(topic, message);
    }

    public void Cleanup()
    {
        subSocket?.Close();
        subSocket?.Dispose();
    }
}
