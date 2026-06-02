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
        [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.3f);
        [SerializeField] private Color selectedColor = new Color(0.3f, 0.5f, 0.8f);
        
        private RechargeScreen parentScreen;
        private long amountPaisa;
        private bool isSelected;
        
        public void Setup(Models.RechargeAmountPreset amount, RechargeScreen screen)
        {
            parentScreen = screen;
            amountPaisa = amount.amount_paisa;
            
            if (amountText != null)
                amountText.text = amount.display_text;
            
            if (bonusText != null && amount.bonus_amount_paisa > 0)
            {
                bonusText.gameObject.SetActive(true);
                bonusText.text = $"+{MoneyFormatter.FormatPaisaRoundedRupees(amount.bonus_amount_paisa)} bonus";
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
            var amount = new Models.RechargeAmountPreset(amountPaisa);
            parentScreen.SelectAmount(amount, this);
        }

        public void Select()
        {
            var amount = new Models.RechargeAmountPreset(amountPaisa);
            parentScreen.SelectAmount(amount, this);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (backgroundImage != null)
            {
                backgroundImage.color = selected ? selectedColor : normalColor;
            }
        }
    }
}