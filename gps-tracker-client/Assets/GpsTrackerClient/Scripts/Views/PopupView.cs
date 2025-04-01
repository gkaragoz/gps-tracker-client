using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GpsTrackerClient.Scripts.Views
{
    public class PopupView : MonoBehaviour
    {
        [SerializeField] private RectTransform parentRectTransform;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descText;
        [SerializeField] private Button closeButton;

        public void Open(string title, string description, Action onClose)
        {
            if (titleText == null || descText == null)
            {
                Debug.LogError("Title or description text is not assigned.");
                return;
            }

            titleText.text = title;
            descText.text = description;
            
            gameObject.SetActive(true);

            // force layout update
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);
            
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                onClose?.Invoke();
                gameObject.SetActive(false);
            });
        }

        public void ForceClose()
        {
            gameObject.SetActive(false);
        }
    }
}