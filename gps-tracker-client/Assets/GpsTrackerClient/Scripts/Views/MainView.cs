using System;
using GpsTrackerClient.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GpsTrackerClient.Scripts.Views
{
    public class MainView : MonoBehaviour
    {
        [SerializeField] private PopupView popupView;
        
        [Header("Blocker and Loading Circle")]
        [SerializeField] private GameObject blockerObj;
        [SerializeField] private LoadingCircle loadingCircle;

        [Header("Connection Status")] 
        [SerializeField] private Color connectedColor;
        [SerializeField] private Color notConnectedColor;
        [SerializeField] private TextMeshProUGUI connectionStatusText;
        [SerializeField] private Image connectionStatusImage;
        
        [Header("Info Texts")]
        [SerializeField] private InfoItem currentLocationInfoItem;
        [SerializeField] private InfoItem lastSyncedInfoItem;
        
        [Header("Tracking Buttons")]
        [SerializeField] private GameObject startTrackingSection;
        [SerializeField] private GameObject stopTrackingSection;
        [SerializeField] private CountdownCoroutine countdownCoroutine;

        private void Awake()
        {
            ShowLoadingCircle();
            currentLocationInfoItem.SetInfo(string.Empty, string.Empty);
            lastSyncedInfoItem.SetInfo(string.Empty, string.Empty);
            ShowStartTrackingButton();
        }

        private void Start()
        {
            SocketService.Instance.OnConnected += OnConnected;
            SocketService.Instance.OnDisconnected += OnDisconnected;
            DeviceLocationService.Instance.OnDeviceLocationUpdated += OnDeviceLocationUpdated;
            DeviceLocationService.Instance.OnSyncCompleted += OnSyncCompleted;
        }

        private void OnDestroy()
        {
            SocketService.Instance.OnConnected -= OnConnected;
            SocketService.Instance.OnDisconnected -= OnDisconnected;
            DeviceLocationService.Instance.OnDeviceLocationUpdated -= OnDeviceLocationUpdated;
            DeviceLocationService.Instance.OnSyncCompleted -= OnSyncCompleted;
        }

        private void OnConnected()
        {
            HideLoadingCircle();
            connectionStatusImage.color = connectedColor;
            connectionStatusText.text = "Online";
        }

        private void OnDisconnected()
        {
            ShowLoadingCircle();
            connectionStatusImage.color = notConnectedColor;
            connectionStatusText.text = "Offline";
        }
        
        private void OnDeviceLocationUpdated(LocationModel locationModel)
        {
            var desc = $"lat: {locationModel.Latitude}<br>long: {locationModel.Longitude}";
            currentLocationInfoItem.SetInfo("Current Location:", desc);
        }
        
        private void OnSyncCompleted(UserUpdateResponse userUpdateResponse)
        {
            lastSyncedInfoItem.SetInfo("Last Synced Date Time:", userUpdateResponse.data.DateTime.ToString("dd/MM/yyyy HH:mm:ss"));
            countdownCoroutine.SetCountdown(DeviceLocationService.Instance.UpdateInterval, "Syncing: ");
        }

        #region Blocker And Loading Circle
        
        private void ShowBlocker()
        {
            blockerObj.SetActive(true);
        }

        private void HideBlocker()
        {
            blockerObj.SetActive(false);
        }
        
        private void ShowLoadingCircle() 
        {
            ShowBlocker();
            loadingCircle.SetText("Connecting to server...");
            loadingCircle.Show();
        }
        
        private void HideLoadingCircle() 
        {
            HideBlocker();
            loadingCircle.Hide();
        }

        #endregion

        #region Tracking Buttons

        private void ShowStopTrackingButton()
        {
            startTrackingSection.SetActive(false);
            stopTrackingSection.SetActive(true);
            countdownCoroutine.SetCountdown(DeviceLocationService.Instance.UpdateInterval, "Syncing: ");
        }

        private void ShowStartTrackingButton()
        {
            startTrackingSection.SetActive(true);
            stopTrackingSection.SetActive(false);
            countdownCoroutine.StopCountdown();
        }

        #endregion

        #region Buttons

        public void OnAllSessionButtonPressed()
        {
            Debug.Log("All Session button pressed");
            popupView.Open("All Sessions", "This feature is not implemented yet.", () => { });
        }

        public void OnOnlineCountButtonPressed()
        {
            Debug.Log("Online Count button pressed");
            popupView.Open("Online Count", "This feature is not implemented yet.", () => { });
        }
        
        public void OnDeviceIdButtonPressed()
        {
            Debug.Log("Device ID button pressed");
            popupView.Open("Device Id", "This feature is not implemented yet.", () => { });
        }
        
        public void OnListLogsButtonPressed()
        {
            Debug.Log("List Logs button pressed");
            popupView.Open("List Logs", "This feature is not implemented yet.", () => { });
        }
        
        public void OnStartTrackingButtonPressed()
        {
            Debug.Log("Start Tracking button pressed");
            ShowStopTrackingButton();

            DeviceLocationService.Instance.StartTracking();
        }

        public void OnStopTrackingButtonPressed()
        {
            Debug.Log("Stop Tracking button pressed");
            ShowStartTrackingButton();

            DeviceLocationService.Instance.StopTracking();
        }

        #endregion
    }
}