using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;

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

    // Update is called once per frame
    void Update()
    {
  
        // tools_coordinates = new float[tool_count*6];
        // int cur_coord = 0;
        // for (int i = 0; i< tool_count; i++) {
        //     tools_coordinates[cur_coord] = tools[i].transform.localPosition.x;
        //     tools_coordinates[cur_coord + 1] = tools[i].transform.localPosition.y;
        //     tools_coordinates[cur_coord + 2] = tools[i].transform.localPosition.z;
        //     tools_coordinates[cur_coord + 3] = tools[i].transform.localRotation.x;
        //     tools_coordinates[cur_coord + 4] = tools[i].transform.localRotation.y;
        //     tools_coordinates[cur_coord + 5] = tools[i].transform.localRotation.z;
            
        //     cur_coord += 6;
        // }
        for (int i = 0; i < tool_count; i++){
            tools_coordinates = ObtainToolCoordinates(tools[i]); 
            // Convert the float[] to string.
            string toolPoseString = string.Join(",", tools_coordinates.Select(f => f.ToString("F4")));
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
                string topic = topics[i];
                //Get the message from the topic "topic2" (here is string
                message = SubBuffer.GetMessage(topic);
                //Convert the byte[] to your datatype (here is string) and print it in debugConsole.
                debugConsole.Log(Encoding.UTF8.GetString(message));
            }
            
        }
        catch
        {

        }
    }
}
