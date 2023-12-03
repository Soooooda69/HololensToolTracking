using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<System.Action> ExecutionQueue = new Queue<System.Action>();
    private static MainThreadDispatcher _instance;

/*    public static MainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            _instance = new MainThreadDispatcher();
        }
        return _instance;
    }*/

    public static void Enqueue(System.Action action)
    {
        lock (ExecutionQueue)
        {
            ExecutionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        while (ExecutionQueue.Count > 0)
        {
            System.Action action = null;
            lock (ExecutionQueue)
            {
                if (ExecutionQueue.Count > 0)
                {
                    action = ExecutionQueue.Dequeue();
                }
            }

            action?.Invoke();
        }
    }
}