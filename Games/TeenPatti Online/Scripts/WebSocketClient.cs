// WebSocketClient.cs
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;

namespace Teenpatti
{
    public class WebSocketClient : MonoBehaviour
    {
        public static WebSocketClient Instance { get; private set; }
        
        private WebSocket websocket;
        public bool isConnecting = false;
        public bool connected = false;
        private bool isIntentionalDisconnect = false;
        private Queue<string> messageQueue = new Queue<string>();
        
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnMessageReceived;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        
        
        public async Task Connect(string url)
        {
            if (isConnecting) return;
            
            isConnecting = true;
            
            try
            {
                websocket = new WebSocket(url);
                
                websocket.OnOpen += () =>
                {
                    Debug.Log("WebSocket connected!");
                    isConnecting = false;
                    connected = true;
                    MainThreadDispatcher.Enqueue(() => OnConnected?.Invoke());
                };
                
                websocket.OnError += (error) =>
                {
                    Debug.LogError($"WebSocket error: {error}");
                    isConnecting = false;
                    connected = false;
                    MainThreadDispatcher.Enqueue(() => OnDisconnected?.Invoke(error));
                };
                
                websocket.OnClose += (code) =>
                {
                    Debug.Log($"WebSocket closed with code: {code}");
                    isConnecting = false;
                    connected = false;
                    string reason = isIntentionalDisconnect ? "Intentional disconnect" : $"Code: {code}";
                    MainThreadDispatcher.Enqueue(() => OnDisconnected?.Invoke(reason));
                };
                
                websocket.OnMessage += (bytes) =>
                {
                    string message = Encoding.UTF8.GetString(bytes);
                    messageQueue.Enqueue(message);
                };
                
                await websocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection failed: {ex.Message}");
                isConnecting = false;
                MainThreadDispatcher.Enqueue(() => OnDisconnected?.Invoke(ex.Message));
            }
        }
        
        private void Update()
        {
            // Process messages on main thread
            while (messageQueue.Count > 0)
            {
                string message = messageQueue.Dequeue();
                OnMessageReceived?.Invoke(message);
            }
            
            // Keep WebSocket connection alive
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                #if !UNITY_WEBGL || UNITY_EDITOR
                websocket.DispatchMessageQueue();
                #endif
            }
        }
        
        public async Task Send(string message)
        {
            if (websocket == null || websocket.State != WebSocketState.Open)
            {
                Debug.LogError("Cannot send message, WebSocket not connected");
                return;
            }
            
            try
            {
                await websocket.SendText(message);
                Debug.Log($"Sent: {message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to send message: {ex.Message}");    
            }
        }
        
        public async Task SendJson(object data)
        {
            string json = JsonUtility.ToJson(new Wrapper { data = data });
            await Send(json);
        }
        
        public async Task Disconnect()
        {
            isIntentionalDisconnect = true;
            if (websocket != null)
            {
                await websocket.Close();
            }
        }
        
        private void OnDestroy()
        {
            if (websocket != null)
            {
                websocket.Close();
            }
        }
        
        [System.Serializable]
        private class Wrapper
        {
            public object data;
        }
    }
}