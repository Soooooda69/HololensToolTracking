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

    void Start()
    {
        //Find the databuffer component for this pub/subnode that you can interact safely from the main thread.
        PubBuffer = PubNode.GetComponent<DataBuffer>();
        SubBuffer = SubNode.GetComponent<DataBuffer>();
        //Find the debugconsole to print the information while in HoloLens.
        debugConsole = GameObject.FindObjectOfType<DebugConsole>();
    }

    // Update is called once per frame
    void Update()
    {
        // get {
        //         float[] coordinates = new float[sphere_count*3];
        //         int cur_coord = 0;
        //         for (int i = 0; i< sphere_count; i++) {
        //             coordinates[cur_coord] = spheres[i].transform.localPosition.x;
        //             coordinates[cur_coord + 1] = spheres[i].transform.localPosition.y;
        //             coordinates[cur_coord + 2] = spheres[i].transform.localPosition.z;
        //             cur_coord += 3;
        //         }
        //         return coordinates;
        //     }
        tools_coordinates = new float[tool_count*6];
        int cur_coord = 0;
        for (int i = 0; i< tool_count; i++) {
            tools_coordinates[cur_coord] = tools[i].transform.localPosition.x;
            tools_coordinates[cur_coord + 1] = tools[i].transform.localPosition.y;
            tools_coordinates[cur_coord + 2] = tools[i].transform.localPosition.z;
            tools_coordinates[cur_coord + 3] = tools[i].transform.localRotation.x;
            tools_coordinates[cur_coord + 4] = tools[i].transform.localRotation.y;
            tools_coordinates[cur_coord + 5] = tools[i].transform.localRotation.z;
            
            cur_coord += 6;
        }
        // Convert the float[] to string.
        string toolPoseString = string.Join(",", tools_coordinates.Select(f => f.ToString("F4")));
        //For publishers, you need to give the databuffer the (topic,message) pair. The buffer will enqueue the message and will sent as soon as possible.
        PubBuffer.UpdateOrAddMessage("topic1", Encoding.UTF8.GetBytes(toolPoseString));

        //For subscribers, you will try to get the message from specific topc using this command. The "try-catch" structure protects your from nullexception
        //since the message from that topic maynot arrived yet..
        try
        {
            message = SubBuffer.GetMessage("topic2");
            //Convert the byte[] to your datatype (here is string) and print it in debugConsole.
            debugConsole.Log(Encoding.UTF8.GetString(message));
        }
        catch
        {

        }
    }
}
