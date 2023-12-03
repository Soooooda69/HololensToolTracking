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
    private List<byte[]> buffer;
    private DataBuffer databuffer;
    public float Frequency = 30;
    private float millisecondsPerOperation;
    private DateTime lastOperationTime;
    private string receivedTopic;
    private byte[] receivedMessage;
    public ZMQSubscriber(string ip, string port, string[] topicArray, DataBuffer databuffer, float frequency, int BufferSize = 1000)
    {
        IP = ip;
        Port = port;
        Topics = topicArray;
        subSocket = new SubscriberSocket();
        subSocket.Options.ReceiveHighWatermark = BufferSize;
        subSocket.ReceiveReady += OnReceiveReady;
        subSocket.Connect($"tcp://{IP}:{Port}");

        foreach (string topic in Topics)
        {
            subSocket.Subscribe(Encoding.UTF8.GetBytes(topic));
        }

        this.databuffer = databuffer;
        Frequency = frequency;
        lastOperationTime = DateTime.Now;
        millisecondsPerOperation = 1000 / Frequency;
/*        timer = new NetMQTimer(millisecondsPerOperation);
        timer.Elapsed += OnTimerElapsed;*/
    }

/*    private void OnTimerElapsed(object sender, NetMQTimerEventArgs e)
    {

        if (subscriberSocket.TryReceiveFrameString(out string message))
        {
            Console.WriteLine($"Received: {message}");
        }
    }*/

    public SubscriberSocket GetSubscriberSocket()
    {
        return subSocket;
    }

    private void OnReceiveReady(object sender, NetMQSocketEventArgs e)
    {
        if ((DateTime.Now - lastOperationTime).TotalMilliseconds >= millisecondsPerOperation)
        {
            //while (subSocket.TryReceiveMultipartBytes(TimeSpan.FromSeconds(0.001), ref buffer))
            while (subSocket.TryReceiveMultipartBytes(ref buffer))
            {
                receivedTopic = Encoding.UTF8.GetString(buffer[0]);
                receivedMessage = buffer[1];
            }
            MainThreadDispatcher.Enqueue(() => ProcessMessage(receivedTopic, receivedMessage));
            lastOperationTime = DateTime.Now;
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
