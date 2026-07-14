using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Core.Utils;

namespace Features.Withdrawal.UI
{
    public class WithdrawalPresetButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite unselectedSprite;
        [SerializeField] private Sprite selectedSprite;

        private int amountRupees;
        private bool isSelected;
        private WithdrawalScreen parentScreen;
        private Image _targetImage;
        private Button _button;

        private void Awake()
        {
            _targetImage = GetComponent<Image>();
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.transition = Selectable.Transition.None;
            }
        }

        public void Setup(int amountInRupees, WithdrawalScreen screen)
        {
            parentScreen = screen;
            amountRupees = amountInRupees;

            if (amountText != null)
            {
                amountText.text = MoneyFormatter.FormatPaisaRoundedRupees(amountInRupees * 100);
            }

            SetSelected(false);

            if (_button != null)
                _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            parentScreen.SelectPreset(amountRupees, this);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            var targetSprite = selected ? selectedSprite : unselectedSprite;

            if (_targetImage != null)
            {
                _targetImage.sprite = targetSprite;
            }

            if (backgroundImage != null)
            {
                backgroundImage.sprite = targetSprite;
            }
        }
    }
}
