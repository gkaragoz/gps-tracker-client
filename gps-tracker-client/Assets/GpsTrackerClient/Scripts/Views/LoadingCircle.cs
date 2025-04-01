using TMPro;
using UnityEngine;

namespace GpsTrackerClient.Scripts.Views
{
    public class LoadingCircle : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textRef;
        [SerializeField] private RectTransform progressRect;
        [SerializeField] private float rotateSpeed = 200f;

        private void Update()
        {
            progressRect.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
        }

        public void SetText(string text)
        {
            if (textRef == null)
                return;

            textRef.text = text;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}