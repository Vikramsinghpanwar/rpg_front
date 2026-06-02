// using UnityEngine;
// using System.Collections.Generic;
// using System;

// namespace Teenpatti{



// public class MainThreadDispatcher : MonoBehaviour
// {
//     private static MainThreadDispatcher _instance;
//     private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    
//     public static MainThreadDispatcher Instance
//     {
//         get
//         {
//             if (_instance == null)
//             {
//                 _instance = FindObjectOfType<MainThreadDispatcher>();
//                 if (_instance == null)
//                 {
//                     GameObject obj = new GameObject("MainThreadDispatcher");
//                     _instance = obj.AddComponent<MainThreadDispatcher>();
//                 }
//             }
//             return _instance;
//         }
//     }
    
//     void Awake()
//     {
//         if (_instance != null && _instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         _instance = this;
//         DontDestroyOnLoad(gameObject);
//     }
    
//     void Update()
//     {
//         lock (_executionQueue)
//         {
//             while (_executionQueue.Count > 0)
//             {
//                 try
//                 {
//                     _executionQueue.Dequeue().Invoke();
//                 }
//                 catch (Exception e)
//                 {
//                     Debug.LogError($"Error in MainThreadDispatcher: {e.Message}");
//                 }
//             }
//         }
//     }
    
//     public static void Enqueue(Action action)
//     {
//         lock (_executionQueue)
//         {
//             _executionQueue.Enqueue(action);
//         }
//     }
    
//     public static void DequeueAll()
//     {
//         lock (_executionQueue)
//         {
//             _executionQueue.Clear();
//         }
//     }
// }
// }