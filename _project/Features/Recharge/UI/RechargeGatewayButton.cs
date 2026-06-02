using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.Recharge.Models;

namespace Features.Recharge.UI
{
    // Drop-in gateway tile. Inspector assigns the label/icon/background; if any are absent
    // it falls back to a single child TMP_Text. Keeps RechargeScreen free of UI plumbing.
    public class RechargeGatewayButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.3f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.5f, 0.8f);

        public GatewayInfo Gateway { get; private set; }

        Action<GatewayInfo> onClick;
        Button button;

        void Awake()
        {
            button = GetComponent<Button>();
            if (button == null) button = gameObject.AddComponent<Button>();
            if (nameText == null) nameText = GetComponentInChildren<TMP_Text>(true);
            if (background == null) background = GetComponent<Image>();
            button.onClick.AddListener(() => onClick?.Invoke(Gateway));
        }

        public void Setup(GatewayInfo gateway, Action<GatewayInfo> clickHandler)
        {
            Gateway = gateway;
            onClick = clickHandler;
            if (nameText != null) nameText.text = gateway?.name ?? gateway?.id ?? "";
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (background != null) background.color = selected ? selectedColor : normalColor;
        }
    }
}
