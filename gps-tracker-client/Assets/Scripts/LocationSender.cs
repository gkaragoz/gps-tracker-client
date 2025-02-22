using UnityEngine;
using WebSocketSharp;
using System.Collections;

public class LocationSender : MonoBehaviour
{
    private WebSocket ws;
    public string userId = "searcher_1"; // Unique ID for each searcher
    public float updateInterval = 30f; // Configurable update interval
    private bool isQuitting = false;
    private bool isReconnecting = false;
    private const float reconnectDelay = 5f; // Wait before trying to reconnect

    private void Start()
    {
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        if (ws != null)
        {
            ws.Close(); // Ensure old connection is closed before creating a new one
            ws = null;
        }

        ws = new WebSocket("ws://localhost:3000");

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to server");
            isReconnecting = false; // Reset reconnect flag
        };

        ws.OnMessage += (sender, e) => Debug.Log("<<< Message from server: " + e.Data);
        ws.OnError += (sender, e) => Debug.LogWarning("WebSocket Error: " + e.Message);

        ws.OnClose += (sender, e) =>
        {
            Debug.LogWarning($"Disconnected from server. Code: {e.Code}, Reason: {e.Reason}");

            if (!isQuitting)
            {
                StartCoroutine(Reconnect());
            }
        };

        ws.Connect();

        if (!isReconnecting) 
        {
            StartCoroutine(SendLocation());
        }
    }

    private IEnumerator SendLocation()
    {
        while (ws != null)
        {
            yield return new WaitForSeconds(updateInterval);
            SendLocationData();
        }
    }

    private void SendLocationData()
    {
        if (ws is { ReadyState: WebSocketState.Open })
        {
            Vector2 gpsLocation = GetGPSLocation();
            LocationModel locationModel = new LocationModel(userId, gpsLocation.x, gpsLocation.y);
            string jsonData = JsonUtility.ToJson(locationModel);
            Debug.Log($">>> Sending location data: {jsonData}");
            ws.Send(jsonData);
        }
    }

    private IEnumerator Reconnect()
    {
        if (isReconnecting) yield break;
        isReconnecting = true;

        Debug.Log("Attempting to reconnect...");
        yield return new WaitForSeconds(reconnectDelay);
        ConnectToServer();
    }

    private Vector2 GetGPSLocation()
    {
        return new Vector2(Random.Range(-90f, 90f), Random.Range(-180f, 180f)); // Replace with real GPS data
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
        CloseWebSocket();
    }

    private void OnDestroy()
    {
        CloseWebSocket();
    }

    private void CloseWebSocket()
    {
        if (ws is { ReadyState: WebSocketState.Open })
        {
            Debug.Log("Closing WebSocket...");
            ws.Close();
        }
    }
}
