using UnityEngine;

namespace SFramework.SFTask.Module
{
    public class SfUniqueId : MonoBehaviour
    {
        public string Id;
        private void Awake()
        {
            if (string.IsNullOrEmpty(Id)) Id = System.Guid.NewGuid().ToString();
        }
    }
}
