using System;
using UnityEngine;
using WebSocketSharp;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class LocationSender : MonoBehaviour
{
    public float updateInterval = 30f; // Configurable update interval
    public float reconnectDelay = 5f; // Wait before trying to reconnect
    public int agentCount = 10; // Number of agents to simulate
    
    private WebSocket _ws;
    private bool _isQuitting;
    private bool _isReconnecting;
    
    private readonly List<Agent> _agents = new List<Agent>();

    private void Start()
    {
        ConnectToServer();
        CreateAgents();
    }

    private void ConnectToServer()
    {
        if (_ws != null)
        {
            _ws.Close(); // Ensure old connection is closed before creating a new one
            _ws = null;
        }

        _ws = new WebSocket("ws://localhost:3000");

        _ws.OnOpen += (sender, e) =>
        {
            Debug.Log("Connected to server");
            _isReconnecting = false; // Reset reconnect flag
        };

        _ws.OnMessage += (sender, e) => Debug.Log("<<< Message from server: " + e.Data);
        _ws.OnError += (sender, e) => Debug.LogWarning("WebSocket Error: " + e.Message);

        _ws.OnClose += (sender, e) =>
        {
            Debug.LogWarning($"Disconnected from server. Code: {e.Code}, Reason: {e.Reason}");

            if (!_isQuitting)
            {
                StartCoroutine(Reconnect());
            }
        };

        _ws.Connect();

        if (!_isReconnecting) 
        {
            StartCoroutine(SendLocation());
        }
    }
    
    private void CreateAgents()
    {
        for (int ii = 0; ii < agentCount; ii++)
        {
            var randomPlaceId = Random.Range(0, Enum.GetValues(typeof(FamousLocation)).Length);
            var famousPlaceName = Enum.GetName(typeof(FamousLocation), randomPlaceId);
            var famousPlaceLocationModel = GeoHelper.FamousPlaces[(FamousLocation)randomPlaceId];
            var startLocation = new LocationModel(famousPlaceLocationModel.Latitude, famousPlaceLocationModel.Longitude);
            _agents.Add(new Agent($"Agent-{ii}", famousPlaceName, startLocation, 0.01f));
            Debug.Log($"Agent-{ii} created at {famousPlaceName}");
        }
    }

    private IEnumerator SendLocation()
    {
        while (_ws != null)
        {
            yield return new WaitForSeconds(updateInterval);
            foreach (Agent agent in _agents)
                SendLocationData(agent);
        }
    }

    private void SendLocationData(Agent agent)
    {
        if (_ws is { ReadyState: WebSocketState.Open })
        {
            agent.SearchOneStep();
            string jsonData = agent.GetLocationModelJson();
            Debug.Log($">>> Sending location data: {jsonData}");
            _ws.Send(jsonData);
        }
    }

    private IEnumerator Reconnect()
    {
        if (_isReconnecting) yield break;
        _isReconnecting = true;

        Debug.Log("Attempting to reconnect...");
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
        if (_ws is { ReadyState: WebSocketState.Open })
        {
            Debug.Log("Closing WebSocket...");
            _ws.Close();
        }
    }
}
