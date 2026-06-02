using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<System.Action> _executionQueue = new Queue<System.Action>();
private static int _version = 0;
    void Update()
    {
        while (_executionQueue.Count > 0)
        {
            _executionQueue.Dequeue().Invoke();
        }
    }

public static void Enqueue(System.Action action)
{
    int capturedVersion = _version;

    lock (_executionQueue)
    {
        _executionQueue.Enqueue(() =>
        {
            if (capturedVersion != _version) return; // 🔥 ignore stale events
            action.Invoke();
        });
    }
}
    public static int Length()
    {
        return _executionQueue.Count;
    }
    public static void DequeueAll()
    {
        _executionQueue.Clear();
    }

    public static void Clear()
    {
        lock (_executionQueue)
        {
            _executionQueue.Clear();
        }
    }

    public static void Reset()
{
    lock (_executionQueue)
    {
        _version++; // 🔥 invalidate ALL pending + future old events
        _executionQueue.Clear();
    }
}
}
