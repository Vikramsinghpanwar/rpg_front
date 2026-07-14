using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Features.Support.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TextMaxWidthLimiter : MonoBehaviour, ILayoutElement
    {
        [SerializeField] private float maxWidth = 400f;

        private TMP_Text textComponent;
        private LayoutElement layoutElement;

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
            layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }
        }

        private void OnEnable()
        {
            Awake();
        }

        private void Update()
        {
            if (textComponent == null) return;

            float preferredWidthVal = textComponent.GetPreferredValues(textComponent.text, 0, 0).x;

            if (preferredWidthVal > maxWidth)
            {
                layoutElement.preferredWidth = maxWidth;
            }
            else
            {
                layoutElement.preferredWidth = preferredWidthVal;
            }
        }

        public float minWidth => 0;
        public float preferredWidth => layoutElement != null ? layoutElement.preferredWidth : -1;
        public float flexibleWidth => 0;
        public float minHeight => 0;
        public float preferredHeight => -1;
        public float flexibleHeight => 0;
        public int layoutPriority => 1;
        public void CalculateLayoutInputHorizontal() {}
        public void CalculateLayoutInputVertical() {}
    }
}
