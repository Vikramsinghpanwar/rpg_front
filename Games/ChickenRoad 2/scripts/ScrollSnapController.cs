using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Game.ChickenRoad2
{
    public class ScrollSnapController : MonoBehaviour
    {
        public static ScrollSnapController instance;
        public ScrollRect scrollRect;
        public RectTransform content;
        public int totalItems = 5; // Set this based on number of tiles
        public float scrollDuration = 0.3f;

        public int currentIndex = 0;
        private bool isScrolling = false;
        public float delay = 0.05f; // Delay before starting the scroll

        void Start()
        {
            instance = this;
        }

        public void ScrollNext()
        {
            if (isScrolling || currentIndex >= totalItems - 1) return;
            currentIndex++;
            StartCoroutine(SmoothScrollTo(currentIndex));
        }

        public void ScrollPrevious()
        {
            if (isScrolling || currentIndex <= 0) return;
            currentIndex--;
            StartCoroutine(SmoothScrollTo(currentIndex));
        }

        private IEnumerator SmoothScrollTo(int targetIndex)
        {
            yield return new WaitForSeconds(delay);
            isScrolling = true;
            float startTime = Time.time;
            float startPos = scrollRect.horizontalNormalizedPosition;
            float targetPos = (float)targetIndex / (totalItems - 1);

            while (Time.time < startTime + scrollDuration)
            {
                float t = (Time.time - startTime) / scrollDuration;
                scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPos, targetPos, t);
                yield return null;
            }

            scrollRect.horizontalNormalizedPosition = targetPos;
            isScrolling = false;
        }

        public void ResetScroll()
        {
            currentIndex = 0;
            scrollRect.horizontalNormalizedPosition = 0f;
        }
    }
}
