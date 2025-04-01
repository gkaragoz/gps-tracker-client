using System;
using System.Collections;
using PimDeWitte.UnityMainThreadDispatcher;
using UnityEngine;
using WebSocketSharp;

namespace GpsTrackerClient.Scripts.Services
{
    public class SocketService : MonoBehaviour
    {
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnMessageReceived;
        public static SocketService Instance { get; private set; }
        public bool IsConnected => _ws is {ReadyState: WebSocketState.Open};

        [SerializeField] private string url;
        [SerializeField] private float reconnectDelay = 5f; // Wait before trying to reconnect
    
        private WebSocket _ws;
        private bool _isQuitting;
        private bool _isConnecting;

        private Coroutine _reconnectCoroutine;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private IEnumerator Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;
            yield return new WaitForSeconds(1f);
            ConnectToServer();
        }
        
        private void ConnectToServer()
        {
            Debug.LogWarning($"[SocketService]::ConnectToServer() - url: {url}");
            if (_ws != null)
            {
                _ws.Close(); // Ensure old connection is closed before creating a new one
                _ws = null;
            }

            _ws = new WebSocket(url);
            _ws.OnOpen += OnWsOpen;
            _ws.OnMessage += OnWsMessageReceived;
            _ws.OnError += OnWsGotError;
            _ws.OnClose += OnWsClosed;

            Debug.Log($"[SocketService]::ConnectToServer Attempting to connect... to {_ws.Url}");
            _isConnecting = true;
            _ws.Connect();
        }

        private void OnWsOpen(object sender, EventArgs e)
        {
            Debug.LogWarning("[SocketService]::OnWsOpen Connected to server");
            _isConnecting = false; // Reset reconnect flag   
            OnConnected?.Invoke();
        }

        private void OnWsMessageReceived(object sender, MessageEventArgs e)
        {
            Debug.Log("<<< Message from server: " + e.Data);

            try
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnMessageReceived?.Invoke(e.Data);
                });
            }
            catch (Exception exception)
            {
                Debug.LogError(exception.Message);                
            }
        }

        private void OnWsGotError(object sender, ErrorEventArgs e)
        {
            Debug.LogError("[SocketService]::OnWsGotError WebSocket Error: " + e.Message);
        }

        private void OnWsClosed(object sender, CloseEventArgs e)
        {
            Debug.LogWarning($"[SocketService]::OnWsClosed Disconnected from server. Code: {e.Code}, Reason: {e.Reason}");
            OnDisconnected?.Invoke();

            if (!_isQuitting)
                Reconnect();
        }
        
        private void Reconnect()
        {
            if (_isConnecting)
            {
                Debug.LogWarning($"[SocketService]::Reconnect() - Already reconnecting...");
                return;
            }

            if (_reconnectCoroutine != null)
                StopCoroutine(_reconnectCoroutine);
            _reconnectCoroutine = StartCoroutine(Do());

            IEnumerator Do()
            {
                float waitTime = reconnectDelay;
                while (!IsConnected)
                {
                    Debug.LogWarning($"[SocketService]::Reconnect Do() Reconnecting in {waitTime} seconds...");
                    yield return new WaitForSeconds(waitTime);
                    if (IsConnected)
                        break;
                    
                    waitTime = Mathf.Min(waitTime * 2, 30); // Double the delay, max 30s
                    ConnectToServer();
                }
            }
        }
        
        private void OnApplicationQuit()
        {
            Debug.LogWarning($"[SocketService]::OnApplicationQuit");
            _isQuitting = true;
            CloseWebSocket();
        }

        private void OnDestroy()
        {
            Debug.LogWarning($"[SocketService]::OnDestroy");
            CloseWebSocket();
        }

        private void CloseWebSocket()
        {
            Debug.LogWarning($"[SocketService]::CloseWebSocket called.");
            if (_ws != null)
            {
                if (_ws.ReadyState == WebSocketState.Open)
                {
                    Debug.LogWarning("[SocketService]::CloseWebSocket Closing WebSocket");
                    _ws.Close();
                }
                _ws.OnOpen -= OnWsOpen;
                _ws.OnMessage -= OnWsMessageReceived;
                _ws.OnError -= OnWsGotError;
                _ws.OnClose -= OnWsClosed;
                _ws = null;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
#if UNITY_EDITOR
            return;
#endif
            
            if (pauseStatus)
            {
                // Application is paused, stop location updates if necessary
                Debug.LogWarning("[SocketService]::OnApplicationPause() - Paused");
            }
            else
            {
                // Application is resumed, restart location updates
                Debug.LogWarning("[SocketService]::OnApplicationPause() - Resumed");
                if (_ws is not {IsAlive: true})
                    Reconnect();
            }
        }

        public void SendPackage(string json)
        {
            if (_ws is not {ReadyState: WebSocketState.Open})
            {
                Debug.LogError("[SocketService]::SendPackage() - WebSocket is not open.");
                return;
            }
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[SocketService]::SendPackage() Location data is empty. Skipping...");
                return;
            }
            
            Debug.Log($">>> Sending package: {json}");
            _ws.Send(json);
        }
    }
}