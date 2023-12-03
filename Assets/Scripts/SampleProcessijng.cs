using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SampleProcessijng : MonoBehaviour
{
    public GameObject node;
    private DataBuffer buffer;
    // Start is called before the first frame update
    void Start()
    {
        buffer = node.GetComponent<DataBuffer>();
    }

    // Update is called once per frame
    void Update()
    {
        buffer.UpdateOrAddMessage("topic1",Encoding.UTF8.GetBytes("Hello World!"));
    }
}
