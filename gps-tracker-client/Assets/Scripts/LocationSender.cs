using System;
using UnityEngine;
using WebSocketSharp;
using System.Collections;
using TMPro;

public class LocationSender : MonoBehaviour
{
    [SerializeField] private string url;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject submitButtonObj;
    [SerializeField] private GameObject loadingObj;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI currentLocationText;
    
    public float reconnectDelay = 5f; // Wait before trying to reconnect
    
    private WebSocket _ws;
    private bool _isQuitting;
    private bool _isReconnecting;
    
    private Coroutine _sendLocationCoroutine;
    private Coroutine _reconnectCoroutine;

    private void Start()
    {
        inputField.text = url;
        statusText.text = $"Syncing location each {DeviceLocationService.Instance.UpdateInterval} seconds";
        
        DeviceLocationService.Instance.OnLocationUpdated += OnDeviceLocationUpdated;
    }

    private void OnDeviceLocationUpdated(LocationModel newLocation)
    {
        currentLocationText.text = $"Latitude: {newLocation.Latitude}<br>Longitude: {newLocation.Longitude}";
    }

    private void StartTracking()
    {
        Debug.LogWarning($"[LocationSender]::StartTracking() - url: {url}");
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        if (_ws != null)
        {
            _ws.Close(); // Ensure old connection is closed before creating a new one
            _ws = null;
        }

        _ws = new WebSocket(url);
        _ws.OnOpen += OnWsOpen();
        _ws.OnMessage += OnWsMessageReceived();
        _ws.OnError += OnWsGotError();
        _ws.OnClose += OnWsClosed();

        Debug.Log($"[LocationSender]::ConnectToServer Attempting to connect... to {_ws.Url}");
        _ws.Connect();

        if (!_isReconnecting) 
        {
            if (_sendLocationCoroutine != null)
            {
                StopCoroutine(_sendLocationCoroutine);
            }
            
            _sendLocationCoroutine = StartCoroutine(SendLocation());
        }
    }

    private EventHandler OnWsOpen()
    {
        return (_, _) =>
        {
            Debug.LogWarning("Connected to server");
            _isReconnecting = false; // Reset reconnect flag
            Debug.Log($"Agent created at {DeviceLocationService.Instance.CurrentLocation}");
        };
    }

    private EventHandler<MessageEventArgs> OnWsMessageReceived()
    {
        return (_, e) => Debug.Log("<<< Message from server: " + e.Data);
    }

    private EventHandler<ErrorEventArgs> OnWsGotError()
    {
        return (_, e)=> Debug.LogError("WebSocket Error: " + e.Message);
    }
    
    private EventHandler<CloseEventArgs> OnWsClosed()
    {
        return (_, e) =>
        {
            Debug.LogWarning($"Disconnected from server. Code: {e.Code}, Reason: {e.Reason}");
            if (!_isQuitting)
            {
                if (_reconnectCoroutine != null)
                {
                    StopCoroutine(_reconnectCoroutine);
                }
                _reconnectCoroutine = StartCoroutine(Reconnect());
            }
        };
    }

    private IEnumerator SendLocation()
    {
        while (_ws != null)
        {
            yield return new WaitForSeconds(DeviceLocationService.Instance.UpdateInterval);
            if (_ws is { ReadyState: WebSocketState.Open })
            {
                if (DeviceLocationService.Instance != null && DeviceLocationService.Instance.IsLocationEnabled)
                {
                    #if UNITY_EDITOR
                    DeviceLocationService.Instance.SearchOneStep();
                    #endif
                    
                    string jsonData = DeviceLocationService.Instance.GetCurrentLocationModelJson();
                    if (string.IsNullOrEmpty(jsonData))
                    {
                        Debug.LogWarning("Location data is empty. Skipping...");
                        continue;
                    }
                    Debug.Log($">>> Sending location data: {jsonData}");
                    _ws.Send(jsonData);
                }
            }
        }
    }

    private IEnumerator Reconnect()
    {
        if (_isReconnecting) yield break;
        _isReconnecting = true;

        Debug.LogWarning("Disconnected from server. Reconnecting...");
        yield return new WaitForSeconds(reconnectDelay);
        ConnectToServer();
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
        CloseWebSocket();
    }

    private void OnDestroy()
    {
        CloseWebSocket();
    }

    private void CloseWebSocket()
    {
        if (_ws != null)
        {
            if (_ws.ReadyState == WebSocketState.Open)
            {
                Debug.LogWarning("Closing WebSocket...");
                _ws.Close();
            }
            _ws.OnOpen -= OnWsOpen();
            _ws.OnMessage -= OnWsMessageReceived();
            _ws.OnError -= OnWsGotError();
            _ws.OnClose -= OnWsClosed();
            _ws = null;
        }
    }

    public void OnSubmitPressed()
    {
        url = inputField.text;
        Debug.Log($"[LocationSender]::OnSubmitPressed() - url: {inputField.text}");
        inputField.gameObject.SetActive(false);
        submitButtonObj.gameObject.SetActive(false);
        loadingObj.gameObject.SetActive(true);
        StartTracking();
    }
}
