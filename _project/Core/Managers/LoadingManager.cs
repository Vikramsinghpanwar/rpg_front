using TMPro;
using UnityEngine;

namespace Core.Managers
{
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager Instance { get; private set; }

        [SerializeField] GameObject overlay;
        [SerializeField] TMP_Text messageText;

        int activeCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[LoadingManager]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<LoadingManager>();
        }

        public void Bind(GameObject overlayGo, TMP_Text label)
        {
            overlay = overlayGo;
            messageText = label;
            if (overlay != null) overlay.SetActive(activeCount > 0);
        }

        public void Show(string message = null)
        {
            activeCount++;
            if (messageText != null) messageText.text = message ?? "";
            if (overlay != null) overlay.SetActive(true);
            else Debug.Log($"[Loading] {message}");
        }

        public void Hide()
        {
            if (activeCount > 0) activeCount--;
            if (activeCount == 0 && overlay != null) overlay.SetActive(false);
        }

        public void Reset()
        {
            activeCount = 0;
            if (overlay != null) overlay.SetActive(false);
        }
    }
}
