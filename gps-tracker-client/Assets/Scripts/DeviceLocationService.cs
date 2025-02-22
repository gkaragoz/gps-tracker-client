using System.Collections;
using UnityEngine;

public class DeviceLocationService : MonoBehaviour
{
    public static DeviceLocationService Instance { get; private set; }

    public bool IsLocationEnabled { get; private set; }
    public LocationModel CurrentLocation { get; private set; }

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
        yield return RequestLocationPermission();
    }

    private IEnumerator RequestLocationPermission()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("Location services are disabled by the user.");
            IsLocationEnabled = false;
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogWarning("Failed to initialize location services.");
            IsLocationEnabled = false;
        }
        else
        {
            IsLocationEnabled = true;
            StartCoroutine(UpdateLocation());
        }
    }

    private IEnumerator UpdateLocation()
    {
        while (IsLocationEnabled)
        {
            if (Input.location.status == LocationServiceStatus.Running)
            {
                LocationInfo locationInfo = Input.location.lastData;
                CurrentLocation = new LocationModel(locationInfo.latitude, locationInfo.longitude);
            }
            yield return new WaitForSeconds(5f); // Update location every 5 seconds
        }
    }

    private void OnApplicationQuit()
    {
        Input.location.Stop();
    }
}