using System;
using System.Collections;
using System.Threading.Tasks;
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

    private void Start()
    {
        _ = RequestLocationPermissionAndUpdate();
    }

    private async Task RequestLocationPermissionAndUpdate()
    {
#if UNITY_EDITOR
        Debug.Log("Running in Editor - Mocking GPS Data.");
        IsLocationEnabled = true;
        CurrentLocation = new LocationModel(27.9881f, 86.9250f); // Mock location (Mount Everest)
        await Task.CompletedTask;
        return;
#else
    if (!Input.location.isEnabledByUser)
    {
        Debug.LogWarning("Location services are disabled by the user.");
        IsLocationEnabled = false;
        return;
    }

    Input.location.Start(1f, 0.5f); // Request high accuracy location

    int maxWait = 10; // Wait up to 10 seconds for GPS to initialize
    while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
    {
        await Task.Delay(1000);
        maxWait--;
    }

    if (maxWait <= 0 || Input.location.status == LocationServiceStatus.Failed)
    {
        Debug.LogWarning("Failed to initialize GPS.");
        IsLocationEnabled = false;
        return;
    }

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
            break;
        }

        await Task.Delay(1000); // Update location every second instead of 0.1s
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