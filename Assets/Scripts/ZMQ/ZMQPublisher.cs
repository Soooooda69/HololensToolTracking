using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

public class ZMQPublisher
{
    public PublisherSocket pubSocket;
    private string port;
    private ConcurrentQueue<(string Topic, byte[] Message)> messageQueue;
    private DataBuffer databuffer;
    public float Frequency = 30;
    private float millisecondsPerOperation;
    private DateTime lastOperationTime;
    
    public ZMQPublisher(string portNum, DataBuffer databuffer, float frequency,int BufferSize = 1000)
    {
        port = portNum;
        pubSocket = new PublisherSocket();
        pubSocket.SendReady += OnSendReady;
        pubSocket.Options.SendHighWatermark = BufferSize;
        pubSocket.Bind($"tcp://*:{port}");
        messageQueue = new ConcurrentQueue<(string, byte[])>();

        this.databuffer = databuffer;
        Frequency = frequency;
        lastOperationTime = DateTime.Now;
        millisecondsPerOperation = 1000 / Frequency;
    }

    public void EnqueueMessage()
    {
        messageQueue.Clear();
        foreach (var item in databuffer.topicMsg)
        {
            messageQueue.Enqueue((item.Key, item.Value));
        }
    }

    private void OnSendReady(object sender, NetMQSocketEventArgs e)
    {
        if ((DateTime.Now - lastOperationTime).TotalMilliseconds >= millisecondsPerOperation)
        {
            MainThreadDispatcher.Enqueue(() => EnqueueMessage());
            lastOperationTime = DateTime.Now;
            while (messageQueue.TryDequeue(out var message))
            {
                try
                {
                    e.Socket.SendMoreFrame(Encoding.UTF8.GetBytes(message.Topic)).SendFrame(message.Message);
                }
                catch (NetMQException)
                {
                    messageQueue.Enqueue(message);
                    break;
                }
                catch (Exception ex)
                {
                    messageQueue.Enqueue(message);
                    break;
                }
            }
        }
    }

    public void Cleanup()
    {
        pubSocket?.Close();
        pubSocket?.Dispose();
    }
}
