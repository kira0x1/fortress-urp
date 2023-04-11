using UnityEngine;

namespace Kira
{
    public class HideOnPlay : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(false);
        }
    }
}