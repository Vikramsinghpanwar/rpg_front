using TMPro;
using UnityEngine;

namespace Features.Share.UI
{
    public class MlmLevelItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text commissionText;

        public void Setup(int level, double commission)
        {
            if (levelText != null) levelText.text = $"Level {level}";
            if (commissionText != null) commissionText.text = $"{commission}%";
        }
    }
}
