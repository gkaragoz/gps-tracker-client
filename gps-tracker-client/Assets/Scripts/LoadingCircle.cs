using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    [SerializeField] private RectTransform progressRect;
    [SerializeField] private float rotateSpeed = 200f;

    private void Update()
    {
        progressRect.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}