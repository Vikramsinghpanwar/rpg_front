using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Managers
{
    public class PopupManager : MonoBehaviour
    {
        public static PopupManager Instance { get; private set; }

        [SerializeField] GameObject popupRoot;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] TMP_Text primaryButtonText;
        [SerializeField] TMP_Text secondaryButtonText;
        [SerializeField] Button primaryButton;
        [SerializeField] Button secondaryButton;

        Action primaryAction;
        Action secondaryAction;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("[PopupManager]");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<PopupManager>();
        }

        void Awake()
        {
            if (primaryButton != null) primaryButton.onClick.AddListener(OnPrimary);
            if (secondaryButton != null) secondaryButton.onClick.AddListener(OnSecondary);
        }

        public void Bind(GameObject root, TMP_Text title, TMP_Text body, Button primary, TMP_Text primaryLabel, Button secondary = null, TMP_Text secondaryLabel = null)
        {
            popupRoot = root;
            titleText = title;
            bodyText = body;
            primaryButton = primary;
            primaryButtonText = primaryLabel;
            secondaryButton = secondary;
            secondaryButtonText = secondaryLabel;

            if (primaryButton != null) primaryButton.onClick.AddListener(OnPrimary);
            if (secondaryButton != null) secondaryButton.onClick.AddListener(OnSecondary);

            if (popupRoot != null) popupRoot.SetActive(false);
        }

        public void Show(string title, string message, string buttonLabel = "OK", Action onClose = null)
        {
            primaryAction = onClose;
            secondaryAction = null;

            if (popupRoot == null)
            {
                Debug.Log($"[Popup] {title}: {message}");
                onClose?.Invoke();
                return;
            }

            if (titleText != null) titleText.text = title ?? "";
            if (bodyText != null) bodyText.text = message ?? "";
            if (primaryButtonText != null) primaryButtonText.text = buttonLabel ?? "OK";
            if (secondaryButton != null) secondaryButton.gameObject.SetActive(false);

            popupRoot.SetActive(true);
        }

        public void ShowError(string message) => Show("Error", message, "OK");
        public void ShowSuccess(string message) => Show("Success", message, "OK");
        public void ShowInfo(string message) => Show("Info", message, "OK");

        public void ShowConfirm(string title, string message, string confirmLabel, string cancelLabel, Action onConfirm, Action onCancel = null)
        {
            primaryAction = onConfirm;
            secondaryAction = onCancel;

            if (popupRoot == null)
            {
                Debug.Log($"[Confirm] {title}: {message}");
                onConfirm?.Invoke();
                return;
            }

            if (titleText != null) titleText.text = title ?? "";
            if (bodyText != null) bodyText.text = message ?? "";
            if (primaryButtonText != null) primaryButtonText.text = confirmLabel ?? "OK";

            if (secondaryButton != null)
            {
                secondaryButton.gameObject.SetActive(true);
                if (secondaryButtonText != null) secondaryButtonText.text = cancelLabel ?? "Cancel";
            }

            popupRoot.SetActive(true);
        }

        public void Hide()
        {
            if (popupRoot != null) popupRoot.SetActive(false);
            primaryAction = null;
            secondaryAction = null;
        }

        void OnPrimary()
        {
            var action = primaryAction;
            Hide();
            action?.Invoke();
        }

        void OnSecondary()
        {
            var action = secondaryAction;
            Hide();
            action?.Invoke();
        }
    }
}
