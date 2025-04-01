using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GpsTrackerClient.Scripts.Views
{
    public class InfoItem : MonoBehaviour
    {
        [SerializeField] private RectTransform parentRectTransform;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descText;

        public void SetInfo(string title, string description)
        {
            if (titleText == null || descText == null)
            {
                Debug.LogError("Title or description text is not assigned.");
                return;
            }

            titleText.text = title;
            descText.text = description;
            
            // force layout update
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentRectTransform);
        }
    }
}