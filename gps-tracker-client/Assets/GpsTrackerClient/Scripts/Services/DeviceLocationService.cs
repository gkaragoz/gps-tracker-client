#if UNITY_IOS
using UnityEngine.iOS;
#endif
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace GpsTrackerClient.Scripts.Services
{
    public class DeviceLocationService : MonoBehaviour
    {
        [SerializeField] private float updateInterval = 5;

        public static DeviceLocationService Instance { get; private set; }

        public event Action<LocationModel> OnDeviceLocationUpdated;
        public event Action<UserUpdateResponse> OnSyncCompleted;
        public float UpdateInterval => updateInterval;
        public bool IsLocationEnabled { get; private set; }

        private Coroutine _sendLocationCoroutine;
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

                OnDeviceLocationUpdated?.Invoke(value);
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
            SocketService.Instance.OnMessageReceived += OnMessageReceived;
            _ = RequestLocationPermissionAndUpdate();
        }

        private void OnDestroy()
        {
            SocketService.Instance.OnMessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError("[DeviceLocationService]::OnMessageReceived Location data is empty. Skipping...");
                return;
            }

            try
            {
                var userUpdateResponse = JsonUtility.FromJson<UserUpdateResponse>(jsonData);
                if (userUpdateResponse is {IsValid: true})
                {
                    Debug.Log($"[DeviceLocationService]::OnMessageReceived");
                    OnSyncCompleted?.Invoke(userUpdateResponse);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DeviceLocationService::OnMessageReceived exception {e.Message}");   
            }
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
#if UNITY_IOS
    // Check if location permission is already granted
    if (!Input.location.isEnabledByUser)
    {
        Debug.LogWarning("iOS: Location services are disabled.");
        ShowLocationDisabledPopup();
        return;
    }
#endif

#if UNITY_ANDROID
    if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
    {
        Permission.RequestUserPermission(Permission.FineLocation);
        while (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            await Task.Delay(500); // Wait until the user responds
        }
    }
#endif

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
        ShowLocationDisabledPopup();
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

#if !UNITY_EDITOR
        private void ShowLocationDisabledPopup()
        {
            Debug.LogWarning("Showing location permission popup...");

#if UNITY_ANDROID
            // Open Android app location settings
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject intent =
                new AndroidJavaObject("android.content.Intent", "android.settings.LOCATION_SOURCE_SETTINGS");
            currentActivity.Call("startActivity", intent);
#elif UNITY_IOS
        // Open iOS location settings
        Application.OpenURL("App-Prefs:root=LOCATION_SERVICES");
#endif
        }
#endif

        private void OnApplicationQuit()
        {
            Input.location.Stop();
        }
        
        private IEnumerator SendLocation()
        {
            while (SocketService.Instance.IsConnected)
            {
                if (!SocketService.Instance.IsConnected) 
                    yield break;

                if (!IsLocationEnabled)
                    yield break;

#if UNITY_EDITOR
                CurrentLocation += GetRandomStep();
#endif
            
                string jsonData = _agent.GetLocationModelJson();
                if (string.IsNullOrEmpty(jsonData))
                {
                    Debug.LogWarning("[DeviceLocationService]::SendLocation Location data is empty. Skipping...");
                    continue;
                }
                
                SocketService.Instance.SendPackage(jsonData);
                yield return new WaitForSeconds(UpdateInterval);
            }
        }

        private Vector2 GetRandomStep()
        {
            return new Vector2(UnityEngine.Random.Range(-0.0001f, 0.0001f), UnityEngine.Random.Range(-0.0001f, 0.0001f));
        }
        
        public void StartTracking()
        {
            if (!IsLocationEnabled)
            {
                Debug.LogError("[DeviceLocationService]::StartTracking Location services are disabled.");
                return;
            }
            if (!SocketService.Instance.IsConnected)
            {
                Debug.LogError("[DeviceLocationService]::StartTracking Socket is NOT connected.");
                return;
            }
            
            if (_sendLocationCoroutine != null)
                StopCoroutine(_sendLocationCoroutine);
            
            _sendLocationCoroutine = StartCoroutine(SendLocation());
        }

        public void StopTracking()
        {
            if (_sendLocationCoroutine != null)
            {
                StopCoroutine(_sendLocationCoroutine);
                _sendLocationCoroutine = null;
            }
        }
    }
}