using System;
using UnityEngine;

namespace Kira
{
    public class UpdatableData : ScriptableObject
    {
        public event Action OnValuesUpdated;
        public bool autoUpdate;

        // delayCall is to get arround a annoying onvalidate unity warning
        #if UNITY_EDITOR
        private void OnValidate() => UnityEditor.EditorApplication.delayCall += _OnValidate;

        protected virtual void _OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= _OnValidate;
            if (this == null) return;
            if (autoUpdate)
            {
                NotifyUpdatedValues();
            }
        }
        #endif

        public void NotifyUpdatedValues()
        {
            OnValuesUpdated?.Invoke();
        }
    }
}