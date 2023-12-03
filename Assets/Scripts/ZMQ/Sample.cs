using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
public class Sample : MonoBehaviour
{
    static DebugConsole debugConsole;
    public GameObject pub, sub, pub2,sub2;
    void Start()
    {
        debugConsole = GameObject.FindObjectOfType<DebugConsole>();
    }
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        try
        {
            pub2.GetComponent<DataBuffer>().UpdateOrAddMessage("topic4", Encoding.UTF8.GetBytes("Hello World from Pub2!"));
            pub.GetComponent<DataBuffer>().UpdateOrAddMessage("topic2", Encoding.UTF8.GetBytes("Hello World!"));
            debugConsole.Log(Encoding.UTF8.GetString(sub.GetComponent<DataBuffer>().GetMessage("topic1")));
            debugConsole.Log(Encoding.UTF8.GetString(sub2.GetComponent<DataBuffer>().GetMessage("topic3")));
        }
        catch
        {

        }

    }
}
