using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using UnityEngine.AI;
using UnityEngine.Scripting;
using Unity.XR.CoreUtils;

public class SampleProcess : MonoBehaviour
{
    //This is the nodes you want to do the data exchange
    public GameObject PubNode, SubNode;
    private DataBuffer PubBuffer, SubBuffer;
    private DebugConsole debugConsole;
    private byte[] message;
    // Start is called before the first frame update
    public GameObject[] tools;
    public int tool_count
        {
            get { return tools.Length; }
        }
    private float[] tools_coordinates;
    public string[] topics;
    void Start()
    {
        //Find the databuffer component for this pub/subnode that you can interact safely from the main thread.
        PubBuffer = PubNode.GetComponent<DataBuffer>();
        SubBuffer = SubNode.GetComponent<DataBuffer>();
        //Find the debugconsole to print the information while in HoloLens.
        debugConsole = GameObject.FindObjectOfType<DebugConsole>();
    }

    private float[] ObtainToolCoordinates(GameObject tool)
    {
        float[] tool_coordinates = new float[7];
        tool_coordinates[0] = tool.transform.localPosition.x;
        tool_coordinates[1] = tool.transform.localPosition.y;
        tool_coordinates[2] = tool.transform.localPosition.z;
        tool_coordinates[3] = tool.transform.localRotation.x;
        tool_coordinates[4] = tool.transform.localRotation.y;
        tool_coordinates[5] = tool.transform.localRotation.z;
        tool_coordinates[6] = tool.transform.localRotation.w;
        return tool_coordinates;
    }

    private void UpdateToolCoordinates(GameObject tool)
    {
        byte[] poseMessage = SubBuffer.GetMessage(tool.name);
        string poseString = Encoding.UTF8.GetString(poseMessage);
        // debugConsole.Log(poseString.Split(',').ToString());
        if (tool.tag == "InTrack" || poseString.Split(',').Length < 7)
        {   // do not update when the message is not complete
            // debugConsole.Log(tool.name+":"+"Pose message is not complete");
            return;
        }
        float[] toolCoordinates = poseString.Split(',').Select(float.Parse).ToArray();
        Vector3 position = new Vector3(toolCoordinates[0], toolCoordinates[1], toolCoordinates[2]);
        Quaternion rotation = new Quaternion(toolCoordinates[3], toolCoordinates[4], toolCoordinates[5], toolCoordinates[6]);
        // tool.transform.localPosition = position;
        // tool.transform.localRotation = rotation;
        tool.transform.position = position;
        tool.transform.rotation = rotation;

    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < tool_count; i++){
            tools_coordinates = ObtainToolCoordinates(tools[i]); 
            // Convert the float[] to string.
            string toolPoseString = string.Join(",", tools_coordinates.Select(f => f.ToString("F4")));
            if (tools[i].tag != "InTrack")
            {
                toolPoseString += "," + "0";
            }
            else
            {
                toolPoseString += "," + "1";
            }
            
            //For publishers, you need to give the databuffer the (topic,message) pair. The buffer will enqueue the message and will sent as soon as possible.
            string topic_name = tools[i].name;
            PubBuffer.UpdateOrAddMessage(topic_name, Encoding.UTF8.GetBytes(toolPoseString));

        }
        //For subscribers, you will try to get the message from specific topc using this command. The "try-catch" structure protects your from nullexception
        //since the message from that topic maynot arrived yet..
        try
        {
            for (int i = 0; i < tool_count; i++)
            {
                // string topic = topics[i];
                string topic = tools[i].name;
                //Get the message from the topic "topic2" (here is string
                message = SubBuffer.GetMessage(topic);
                //Convert the byte[] to your datatype (here is string) and print it in debugConsole.
                string poseString = Encoding.UTF8.GetString(message);
                string[] poseValues = poseString.Split(',');
                if (poseString.Split(',').Length < 7)
                {   
                    debugConsole.Log(poseString);
                }
                else
                {
                    string translationString = $"{poseValues[0]}, {poseValues[1]}, {poseValues[2]}";
                    string logString = topic + ":" + translationString + ", " + tools[i].tag;
                    debugConsole.Log(logString);
                }
                // debugConsole.Log(topic);
                UpdateToolCoordinates(tools[i]);

                GameObject indicator = tools[i].transform.Find("indicator").gameObject;
                if(indicator != null)
                {
                    Renderer indicator_renderer = indicator.GetComponent<Renderer>();
                    if (tools[i].tag == "InTrack")
                    {
                        indicator_renderer.material.color = Color.green;
                    }
                    else
                    {
                        indicator_renderer.material.color = Color.red;
                    }
                }
            }
            
            
        }
        catch
        {

        }
    }
}
