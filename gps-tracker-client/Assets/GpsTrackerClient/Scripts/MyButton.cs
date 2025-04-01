#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif
using UnityEngine.UI;

namespace GpsTrackerClient.Scripts
{
    public class MyButton : Button
    {
        
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MyButton))]
    public class MyButtonEditor : Editor
    {
        private Editor _defaultEditor;

        private void OnEnable()
        {
            _defaultEditor = CreateEditor(target, typeof(ButtonEditor));
        }

        private void OnDisable()
        {
            if (_defaultEditor != null)
            {
                DestroyImmediate(_defaultEditor);
            }
        }

        public override void OnInspectorGUI()
        {
            if (_defaultEditor != null)
            {
                _defaultEditor.OnInspectorGUI();
            }
        }
    }
#endif
}