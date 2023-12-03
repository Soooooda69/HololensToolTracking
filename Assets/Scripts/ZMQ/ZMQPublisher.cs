using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Text;

public class ZMQPublisher
{
    public PublisherSocket pubSocket;
    private string port;
    private ConcurrentQueue<(string Topic, byte[] Message)> messageQueue;
    private bool isReadyToSend = true;
    private DataBuffer databuffer;

    public ZMQPublisher(string portNum, DataBuffer databuffer)
    {
        port = portNum;
        pubSocket = new PublisherSocket();
        pubSocket.Bind($"tcp://*:{port}");
        messageQueue = new ConcurrentQueue<(string, byte[])>();

        pubSocket.SendReady += OnSendReady;
        this.databuffer = databuffer;
    }

    public void EnqueueMessage()
    {
        foreach (var item in databuffer.topicMsg)
        {
            messageQueue.Enqueue((item.Key, item.Value));
        }
    }

    private void OnSendReady(object sender, NetMQSocketEventArgs e)
    {
        MainThreadDispatcher.Enqueue(() => EnqueueMessage());
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
        isReadyToSend = messageQueue.IsEmpty;
    }

    public void Cleanup()
    {
        pubSocket?.Close();
        pubSocket?.Dispose();
    }
}