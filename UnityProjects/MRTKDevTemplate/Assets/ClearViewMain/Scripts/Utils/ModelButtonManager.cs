using UnityEngine;

namespace ClearView
{
    public class ModelButtonManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ModelManager modelManager;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private Transform toggleListParent;
        [SerializeField] private Component toggleCollection; // assign your ToggleCollection component here

        public ModelManager ModelManager => modelManager;
        public GameObject ListItemPrefab => listItemPrefab;
        public Transform ToggleListParent => toggleListParent;
        public Component ToggleCollection => toggleCollection;

        private void Reset()
        {
            modelManager = GetComponent<ModelManager>();
        }
    }
}
