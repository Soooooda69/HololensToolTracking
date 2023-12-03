using System.Threading.Tasks;
using UnityEngine;
using System.Text;
using TMPro;
using NetMQ;
using NetMQ.Sockets;
using NetMQ.Monitoring;
using System.Collections;
using System.Collections.Generic;

public class NetMQManager : MonoBehaviour
{
    private AsycNetMQ AsycNetMQInstance;
    public static DebugConsole debugConsole;
    //public GameObject goSocketInfo;
    //private TMP_Text SocketInfo;
    private bool AsyncIOon = false;
    private bool Connected = false;
    private List<GameObject> AllNodes;
    private List<PublisherSample> PubNodes = new List<PublisherSample> { };
    private List<SubscriberSample> SubNodes = new List<SubscriberSample> { };

    void Start()
    {
        debugConsole = GameObject.FindObjectOfType<DebugConsole>();
        AllNodes = GetAllChildren(this.gameObject);
        //SocketInfo = goSocketInfo.GetComponent<TextMeshPro>();
    }

    public List<GameObject> GetAllChildren(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            children.Add(parent.transform.GetChild(i).gameObject);
        }

        return children;
    }

    public void Connect()
    {
        foreach(GameObject node in AllNodes)
        {
            if (node.tag == "PubNode")
            {
                string[] info = node.GetComponent<ZMQPublisherHelper>().GetPublisherInfo();
                PubNodes.Add(new PublisherSample(node.name, info[0], node.GetComponent<DataBuffer>()));
            }
            else if (node.tag == "SubNode")
            {
                string[] SocketInfo, Topics;
                (SocketInfo,Topics) = node.GetComponent<ZMQSubscriberHelper>().GetSubscriberInfo();
                SubNodes.Add(new SubscriberSample(node.name, SocketInfo[0], SocketInfo[1], Topics, node.GetComponent<DataBuffer>()));
            }
        }
        AsyncIO.ForceDotNet.Force();
        AsyncIOon = true;
        Task.Run(() =>
        {
/*            try
            {*/
                AsycNetMQInstance = AsycNetMQ.Instance;
                foreach(PublisherSample sample in PubNodes)
                {
                    AsycNetMQInstance.CreatePublisherSocketFromSample(sample);
                }
                foreach (SubscriberSample sample in SubNodes)
                {
                    AsycNetMQInstance.CreateSubscriberSocketFromSample(sample);
                }
                AsycNetMQInstance.StartPoller();
/*            }
            catch
            {
                Cleanup();
            }*/
        });
    }

    private void Cleanup()
    {
        AsycNetMQInstance?.Cleanup();
        if (AsyncIOon)
        {
            NetMQConfig.Cleanup();
        }
        AsyncIOon = false;
        Connected = false;
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}

public struct PublisherSample
{
    public PublisherSample(string name, string port, DataBuffer buffer)
    {
        Name = name;
        Port = port;
        Buffer = buffer; 
    }

    public string Name { get; }
    public string Port { get; }
    public DataBuffer Buffer { get; }
}

public struct SubscriberSample
{
    public SubscriberSample(string name, string ip, string port, string[] topics, DataBuffer buffer)
    {
        Name = name;
        IP = ip;
        Port = port;
        Topics = topics;
        Buffer = buffer;
    }

    public string Name { get; }
    public string IP { get; }
    public string Port { get; }
    public string[] Topics { get; }
    public DataBuffer Buffer { get; }
}

