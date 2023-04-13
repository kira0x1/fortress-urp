using UnityEditor;
using UnityEngine;

namespace Kira
{
    [CustomEditor(typeof(UpdatableData), true)]
    public class UpdatableDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UpdatableData data = (UpdatableData)target;

            if (GUILayout.Button("Update"))
            {
                data.NotifyUpdatedValues();
            }
        }
    }
}