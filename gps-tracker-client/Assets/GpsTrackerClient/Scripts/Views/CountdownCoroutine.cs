using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace GpsTrackerClient.Scripts.Views
{
    public class CountdownCoroutine : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI countdownText;
        
        private bool _isRunning;
        private float _countdownTime;
        private string _countdownTextPrefix;

        private void Update()
        {
            if (!_isRunning)
                return;
            
            _countdownTime -= Time.deltaTime;
            if (_countdownTime <= 0)
            {
                _isRunning = false;
                _countdownTime = 0;
            }
            
            if (countdownText != null)
                countdownText.text = $"{_countdownTextPrefix}{_countdownTime:F1}";
        }

        public void SetCountdown(float countdownTime, string countdownTextPrefix = "")
        {
            _countdownTime = countdownTime;
            _countdownTextPrefix = countdownTextPrefix;
            _isRunning = true;
        }

        public void StopCountdown()
        {
            _isRunning = false;
        }
    }
}