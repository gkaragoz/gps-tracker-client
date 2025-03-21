using System;
using System.Collections;
using UnityEngine;

public class DeviceLocationService : MonoBehaviour
{
    [SerializeField] private float updateInterval = 5;
    
    public static DeviceLocationService Instance { get; private set; }

    public event Action<LocationModel> OnLocationUpdated;
    public float UpdateInterval => updateInterval;
    public bool IsLocationEnabled { get; private set; }

    private LocationModel _currentLocation;
    public LocationModel CurrentLocation
    {
        get => _currentLocation;
        private set
        {
            _currentLocation = value;
            if (_agent == null)
            {
                var startLocation = Instance.CurrentLocation;
                string deviceId = SystemInfo.deviceUniqueIdentifier;
                _agent = new Agent(deviceId, startLocation);
            }
            else
            {
                _agent.UpdateLocation(value);
            }
            OnLocationUpdated?.Invoke(value);
        }
    }
    private Agent _agent;

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
        yield return RequestLocationPermissionAndUpdate();
    }

    private IEnumerator RequestLocationPermissionAndUpdate()
    {
#if UNITY_EDITOR
        Debug.Log("Running in Editor - Mocking GPS Data.");
        IsLocationEnabled = true;
        CurrentLocation = new LocationModel(27.9881f, 86.9250f); // Mount Everest (mock data)
        yield break;  // No need for real GPS updates in editor mode.
#else
    if (!Input.location.isEnabledByUser)
    {
        Debug.LogWarning("Location services are disabled by the user.");
        IsLocationEnabled = false;
        yield break;
    }

    Input.location.Start(1f, 0.5f); // High accuracy: 1m precision, updates if moved 0.5m

    int maxWait = 10; // Wait for up to 10 seconds for GPS to initialize
    while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
    {
        yield return new WaitForSeconds(1);
        maxWait--;
    }

    if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
    {
        Debug.LogWarning("Failed to initialize GPS.");
        IsLocationEnabled = false;
        yield break;
    }

    // GPS is now running
    IsLocationEnabled = true;
    
    while (IsLocationEnabled)
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            LocationInfo locationInfo = Input.location.lastData;
            CurrentLocation = new LocationModel(locationInfo.latitude, locationInfo.longitude);
            Debug.Log($"Updated Location: {locationInfo.latitude}, {locationInfo.longitude}");
        }
        else
        {
            Debug.LogWarning("GPS stopped running.");
            IsLocationEnabled = false;
            yield break;
        }

        yield return new WaitForSeconds(0.1f); // Get updates **every 0.1 seconds** (fastest possible)
    }

    Input.location.Stop(); // Stop GPS when not needed
#endif
    }

    private void OnApplicationQuit()
    {
        Input.location.Stop();
    }

    public string GetCurrentLocationModelJson()
    {
        return _agent.GetLocationModelJson();
    }

    public void SearchOneStep()
    {
        CurrentLocation += GetRandomStep();
    }
    
    private Vector2 GetRandomStep()
    {
        return new Vector2(UnityEngine.Random.Range(-0.0001f, 0.0001f), UnityEngine.Random.Range(-0.0001f, 0.0001f));
    }
}