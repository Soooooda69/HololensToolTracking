using System;
using System.Threading;
using System.Threading.Tasks;
using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class ZMQClient : MonoBehaviour
{
    //Configuration done in Unity
    public TMP_Text inputFieldIP;
    public TMP_Text inputFieldPort;
    public string ip, port;
    public float HEARTBEAT_INTERVAL = 3;
    public float interv;
    public bool ManagerInUse;
    //Private built-in settings.
    private RequestSocket sock;
    private DebugConsole debugConsole;
    private List<byte[]> frames, heartbeatframes;
    public delegate (byte[] cmdCallback, byte[] msgCallback) CallbackFunction(byte[] msg);
    private byte[] heartbeat;
    private TimeSpan timeout = TimeSpan.FromSeconds(1);
    private DateTime lastHeartbeatReceived, lastHeartbeatSent, lastCommunicationSent;
    bool Connected = false;
    private byte[] cmdRequest, msgRequest;
    private bool Chunking;

    public void Cleanup()
    {
        sock?.Close();
        sock = null;
        Connected = false;
        if (!ManagerInUse)
        {
            NetMQConfig.Cleanup();
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    //You need to configure your own (cmd,msg) mapping here.
    public static Dictionary<string, CallbackFunction> CallBackFunc { get; set; } = new Dictionary<string, CallbackFunction>
    {   {"Relax", FFFProcesses.fffProcessesCallBackWait},
        { "Chunk", FFFProcesses.fffProcessesCallBackChunking},
        { "Continue", FFFProcesses.fffProcessesCallBackChunking},
        { "Finish",FFFProcesses.fffProcessesCallBackFinish}
    };

    void Start()
    {
        heartbeat = Encoding.UTF8.GetBytes("heartbeat");
        AsyncIO.ForceDotNet.Force();
        heartbeatframes = new List<byte[]> { Encoding.UTF8.GetBytes("heartbeat"), Encoding.UTF8.GetBytes("NONE") };
        frames = new List<byte[]> { Encoding.UTF8.GetBytes("Relax"), Encoding.UTF8.GetBytes("0 0 0 0 0 0") };
        cmdRequest = Encoding.UTF8.GetBytes("Extrinsic");
        msgRequest = Encoding.UTF8.GetBytes("None");
        Chunking = false;
        debugConsole = GameObject.FindObjectOfType<DebugConsole>();
    }

    void Update()
    {
        if (Connected)
        {
            _LoopingSync();
            lastCommunicationSent = DateTime.UtcNow;
            if (!Chunking)
            {
                if (DateTime.UtcNow - lastHeartbeatSent >= TimeSpan.FromSeconds(HEARTBEAT_INTERVAL))
                {
                    _HeartbeatLoopSync();
                    lastHeartbeatSent = DateTime.UtcNow;
                }
            }
            else
            {
                lastHeartbeatReceived = DateTime.UtcNow;
            }
            StillAlive();
        }
    }

    public void Connect()
    {
        if (inputFieldIP != null)
        {
            ip = inputFieldIP.text;
            ip = ip[..^1];
        }
        if (inputFieldPort != null)
        {
            port = inputFieldPort.text;
            port = port[..^1];
        }
        debugConsole?.Log($"[FFF INFO] Try at tcp://{ip}:{port}");

        if (sock != null)
        {
            throw new InvalidOperationException("Socket is already connected. Disconnect first.");
        }

        sock = new RequestSocket();
        sock.Connect($"tcp://{ip}:{port}");

        sock.SendMoreFrame(heartbeat).SendFrame(Encoding.UTF8.GetBytes("NONE"));
        var timeout = TimeSpan.FromSeconds(3);
        try
        {
            if (sock.TryReceiveMultipartBytes(timeout, ref frames))
            {
                debugConsole?.Log($"[FFF INFO] Connected to Serve at tcp://{this.ip}:{this.port}");
                debugConsole?.Log("[FFF INFO] Client Online.");
                Connected = true;
                lastHeartbeatSent = DateTime.UtcNow;
                lastHeartbeatReceived = DateTime.UtcNow;
            }
            else
            {
                sock?.Close();
                sock = null;
                debugConsole?.Log($"[FFF INFO] Cannot reach to Server at tcp://{this.ip}:{this.port}. Connection Aborted.");
                Connected = false;
            }
        }
        catch (NetMQException)
        {
            sock?.Close();
            sock = null;
            debugConsole?.Log($"[FFF INFO] Cannot reach to Server at tcp://{this.ip}:{this.port}. Connection Aborted.");
            Connected = false;
        }
        catch (Exception ex)
        {
            debugConsole?.Log(ex.ToString());
            Connected = false;
        }
    }

    private void _HeartbeatLoopSync()
    {
        sock.SendMoreFrame(heartbeat).SendFrame(Encoding.UTF8.GetBytes("NONE"));
        debugConsole?.Log("[FFF INFO] Heartbeat sent to server.");
        heartbeatframes[0] = Encoding.UTF8.GetBytes("NONE");
        try
        {
            if (sock.TryReceiveMultipartBytes(timeout, ref heartbeatframes) && heartbeatframes.Count == 2)
            {
                if (System.Text.Encoding.UTF8.GetString(heartbeatframes[0]) == "heartbeat_ack")
                {
                    lastHeartbeatReceived = DateTime.UtcNow;
                    debugConsole?.Log("[FFF INFO] Heartbeat acknowledgment received from server.");
                }
            }
        }
        catch (NetMQException)
        {
            return;
        }
        catch (Exception ex)
        {
            debugConsole.Log(ex.ToString());
        }
    }

    private void StillAlive()
    {
        if (DateTime.UtcNow - lastHeartbeatReceived > TimeSpan.FromSeconds(5))
        {
            debugConsole?.Log("[FFF ERROR] No acknowledgment from the server for 5 seconds. Disconnected.");
            Cleanup();
        }
    }

    private void _LoopingSync()
    {
        sock.SendMoreFrame(cmdRequest).SendFrame(msgRequest);
        try
        {
            if (sock.TryReceiveMultipartBytes(timeout, ref frames) && frames.Count == 2)
            {
                if (Encoding.UTF8.GetString(frames[0]) == "Chunk")
                {
                    Chunking = true;
                }
                else if (Encoding.UTF8.GetString(frames[0]) == "Finish")
                {
                    Chunking = false;
                }
                (cmdRequest, msgRequest) = CallBackFunc[Encoding.UTF8.GetString(frames[0])](frames[1]);
            }
        }
        catch (NetMQException)
        {
            return;
        }
        catch (Exception ex)
        {
            debugConsole?.Log(ex.ToString());
        }
    }
}