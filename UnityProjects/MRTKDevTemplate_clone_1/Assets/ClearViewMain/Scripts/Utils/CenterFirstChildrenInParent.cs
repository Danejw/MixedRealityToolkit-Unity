using UnityEngine;

namespace ClearView
{
    public class CenterFirstChildrenInParent : MonoBehaviour
    {
        [ContextMenu("Center First Children In Parent")]
        public void CenterChildren()
        {
            if (transform.childCount == 0)
            {
                Debug.LogWarning("No children to center.", this);
                return;
            }

            bool hasBounds = false;
            Bounds combinedBounds = new Bounds();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                Renderer[] renderers = child.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer r in renderers)
                {
                    if (!hasBounds)
                    {
                        combinedBounds = r.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        combinedBounds.Encapsulate(r.bounds);
                    }
                }
            }

            if (!hasBounds)
            {
                Debug.LogWarning("No renderers found under first-level children.", this);
                return;
            }

            Vector3 offset = transform.position - combinedBounds.center;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                child.position += offset;
            }

            Debug.Log($"Centered children. Offset applied: {offset}", this);
        }
    }
}