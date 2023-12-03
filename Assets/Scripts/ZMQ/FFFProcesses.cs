using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;


public class FFFProcesses : MonoBehaviour
{
    private Matrix4x4 Extrinsic;
    private static readonly Lazy<FFFProcesses> _instance = new Lazy<FFFProcesses>(() => new FFFProcesses());
    public static FFFProcesses Instance => _instance.Value;
    private static List<string> Property;
    private static byte[] cmdCallback;
    private static byte[] msgCallback;
    private static float[] _extrinsic;
    static DebugConsole debugConsole = GameObject.FindObjectOfType<DebugConsole>();
    private static byte[] incompleteData;
    private static bool isReceivingData = false;
    private static List<byte> receivedDataBuffer = new List<byte>();
    private static Slice slice;

    public void loadExtrinsic(Matrix4x4 mat)
    {
        Extrinsic = mat;
    }

    public static (byte[] cmdCallback, byte[] msgCallback) fffProcessesCallBackWait(byte[] msg)
    {
        cmdCallback = Encoding.UTF8.GetBytes("Extrinsic");
        msgCallback = Encoding.UTF8.GetBytes("None");
        return (cmdCallback, msgCallback);
    }

    public static (byte[] cmdCallback, byte[] msgCallback) fffProcessesCallBackChunking(byte[] msg)
    {
        if (!isReceivingData)
        {
            isReceivingData = true;
            receivedDataBuffer.Clear();
        }
        receivedDataBuffer.AddRange(msg);
        cmdCallback = Encoding.UTF8.GetBytes("Ready");
        msgCallback = new byte[] { };
        return (cmdCallback, msgCallback);
    }

    public static (byte[] cmdCallback, byte[] msgCallback) fffProcessesCallBackFinish(byte[] msg)
    {
        receivedDataBuffer.AddRange(msg);
        incompleteData = receivedDataBuffer.ToArray();
        receivedDataBuffer.Clear();
        isReceivingData = false;
        cmdCallback = Encoding.UTF8.GetBytes("Extrinsic");
        msgCallback = Encoding.UTF8.GetBytes("None");
        return (cmdCallback, msgCallback);
    }

}
