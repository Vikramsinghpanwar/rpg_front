using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.Utils;

namespace Features.Recharge.UI
{
    public class RechargeAmountButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text bonusText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Sprite selectedSprite;

        private RechargeScreen parentScreen;
        private Models.RechargeAmountPreset amountPreset;

        public void Setup(Models.RechargeAmountPreset amount, RechargeScreen screen)
        {
            parentScreen = screen;
            amountPreset = amount;

            if (amountText != null)
                amountText.text = amount.display_text;

            if (bonusText != null && amount.bonus_amount_paisa > 0)
            {
                bonusText.gameObject.SetActive(true);
                bonusText.text = $"+{MoneyFormatter.FormatPaisaRoundedRupees(amount.bonus_amount_paisa)}";
            }
            else if (bonusText != null)
            {
                bonusText.gameObject.SetActive(false);
            }

            var btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            parentScreen.SelectAmount(amountPreset, this);
        }

        public void Select()
        {
            parentScreen.SelectAmount(amountPreset, this);
        }

        public void SetSelected(bool selected)
        {
            if (backgroundImage != null)
            {
                backgroundImage.sprite = selected ? selectedSprite : unselectedSprite;
            }
        }
    }
}