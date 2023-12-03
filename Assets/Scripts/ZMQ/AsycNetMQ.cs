using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;
using NetMQ.Monitoring;

public class AsycNetMQ
{
    private static AsycNetMQ _instance;
    private Dictionary<string, ZMQPublisher> publishers = new Dictionary<string, ZMQPublisher>();
    private Dictionary<string, ZMQSubscriber> subscribers = new Dictionary<string, ZMQSubscriber>();
    private NetMQPoller poller;
    private Dictionary<string, NetMQMonitor> monitors = new Dictionary<string, NetMQMonitor>();
    public static AsycNetMQ Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AsycNetMQ();
            }
            return _instance;
        }
    }

    private AsycNetMQ()
    {
        poller = new NetMQPoller();
    }

    public void StartPoller()
    {
        poller.RunAsync();
    }

    public void CreatePublisherSocket(string socketName, string port, DataBuffer buffer, float frequency, int buffersize)
    {
        var pubSocket = new ZMQPublisher(port, buffer, frequency, buffersize);
        publishers[socketName] = pubSocket;
        poller.Add(pubSocket.pubSocket);
    }

    public void CreateSubscriberSocket(string socketName, string ip, string port, string[] topic, DataBuffer buffer, float frequency, int buffersize)
    {
        var subSocket = new ZMQSubscriber(ip, port, topic, buffer, frequency, buffersize);
        subscribers[socketName] = subSocket;
        poller.Add(subSocket.subSocket);
    }

    public void CreateSubscriberSocketFromSample(SubscriberSample sample)
    {
        CreateSubscriberSocket(sample.Name, sample.IP, sample.Port, sample.Topics,sample.Buffer, sample.Frequency, sample.BufferSize);
    }

    public void CreatePublisherSocketFromSample(PublisherSample sample)
    {
        CreatePublisherSocket(sample.Name, sample.Port, sample.Buffer, sample.Frequency, sample.BufferSize);
    }

    private void StartMonitoring(NetMQSocket socket, string monitorAddress)
    {
        socket.Monitor(monitorAddress);
        var monitorSocket = new NetMQMonitor(socket, monitorAddress, SocketEvents.All);
        monitorSocket.Connected += (s, e) =>
        {
            Debug.Log($"Socket {monitorAddress} connected.");
        };
        monitorSocket.Disconnected += (s, e) =>
        {
            Debug.Log($"Socket {monitorAddress} disconnected.");
        };
        monitors[monitorAddress] = monitorSocket;
        monitorSocket.AttachToPoller(poller);
        monitorSocket.StartAsync();
    }

    public void Cleanup()
    {
        poller?.StopAsync();
        poller?.Dispose();
        foreach (var publisher in publishers.Values)
        {
            publisher.Cleanup();
        }
        foreach (var subscriber in subscribers.Values)
        {
            subscriber.Cleanup();
        }
        foreach (var monitor in monitors.Values)
        {
            monitor.Stop();
            monitor.Dispose();
        }
    }
}
