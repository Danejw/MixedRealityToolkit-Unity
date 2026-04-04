using UnityEngine;

namespace ClearView
{
    public class ToggleListPaginator : MonoBehaviour
    {
        [SerializeField] private Transform pagesParent;
        [SerializeField] private int currentPageIndex = 0;

        private void Awake()
        {
            if (pagesParent == null)
                pagesParent = transform;

            ClampIndex();
            ShowCurrentPage();
        }

        public void NextPage()
        {
            if (pagesParent == null || pagesParent.childCount == 0) return;

            currentPageIndex++;
            if (currentPageIndex >= pagesParent.childCount)
                currentPageIndex = 0;

            ShowCurrentPage();
        }

        public void PreviousPage()
        {
            if (pagesParent == null || pagesParent.childCount == 0) return;

            currentPageIndex--;
            if (currentPageIndex < 0)
                currentPageIndex = pagesParent.childCount - 1;

            ShowCurrentPage();
        }

        public void SetPage(int pageIndex)
        {
            if (pagesParent == null || pagesParent.childCount == 0) return;

            currentPageIndex = pageIndex;
            ClampIndex();
            ShowCurrentPage();
        }

        public void RefreshPages()
        {
            if (pagesParent == null)
                pagesParent = transform;

            ClampIndex();
            ShowCurrentPage();
        }

        private void ShowCurrentPage()
        {
            if (pagesParent == null) return;

            for (int i = 0; i < pagesParent.childCount; i++)
            {
                pagesParent.GetChild(i).gameObject.SetActive(i == currentPageIndex);
            }
        }

        private void ClampIndex()
        {
            if (pagesParent == null || pagesParent.childCount == 0)
            {
                currentPageIndex = 0;
                return;
            }

            if (currentPageIndex < 0)
                currentPageIndex = 0;

            if (currentPageIndex >= pagesParent.childCount)
                currentPageIndex = pagesParent.childCount - 1;
        }
    }
}