using UnityEngine;
using TMPro;

namespace Game.ChickenRoad2
{

    public class BetManager : MonoBehaviour
    {
        public static BetManager instance;
        public TextMeshProUGUI betAmountText;
        public TextMeshProUGUI cashOutAmountText;
        public TMP_Dropdown levelDropdown;

        public GameObject[] levelPanelsArray;

        public float betAmount = 10f;
        float[] easyMultipliers = { 0f, 1.03f, 1.12f, 1.17f, 1.23f, 1.29f, 1.36f, 1.44f, 1.53f, 1.63f, 1.75f, 1.88f, 2.04f, 2.22f, 2.45f, 2.72f, 3.06f, 3.5f, 4.08f, 4.90f, 6.13f, 6.61f, 9.81f, 19.44f };
        float[] mediumMultipliers = { 0f, 1.12f, 1.28f, 1.47f, 1.70f, 1.98f, 2.33f, 2.76f, 3.32f, 4.03f, 4.96f, 6.20f, 6.91f, 8.90f, 11.74f, 15.99f, 22.61f, 33.58f, 53.20f, 92.17f, 182.51f, 451.71f, 1788.80f };
        float[] hardMultipliers = { 0f, 1.23f, 1.55f, 1.98f, 2.56f, 3.36f, 4.49f, 5.49f, 7.53f, 10.56f, 15.21f, 22.59f, 34.79f, 55.97f, 94.99f, 172.42f, 341.40f, 760.46f, 2007.63f, 6956.47f, 41321.43f };
        float[] hardcoreMultipliers = { 0f, 1.63f, 2.80f, 4.95f, 9.08f, 15.21f, 30.12f, 62.96f, 140.24f, 337.19f, 890.19f, 2643.89f, 9161.08f, 39301.05f, 233448.29f, 2542251.93f };

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public enum Level
        {
            Easy,
            Medium,
            Hard,
            Hardcore
        }
        public Level currentLevel = Level.Easy;



        private void Start()
        {
            UpdateBetAmountText();
        }

        private void UpdateBetAmountText()
        {
            betAmountText.text = "₹ " + betAmount.ToString();
        }
        public void SetBetAmount(float amount)
        {
            betAmount = amount;
            UpdateBetAmountText();
        }

        public void UpdateCashOutAmount()
        {
            float cashOutAmount = betAmount * GetCurrentMultiplier();
            cashOutAmountText.text = "₹ " + cashOutAmount.ToString();
        }

        public float GetCurrentMultiplier()
        {
            switch (currentLevel)
            {
                case Level.Easy:
                    return easyMultipliers[ScrollSnapController.instance.currentIndex];
                case Level.Medium:
                    return mediumMultipliers[ScrollSnapController.instance.currentIndex];
                case Level.Hard:
                    return hardMultipliers[ScrollSnapController.instance.currentIndex];
                case Level.Hardcore:
                    return hardcoreMultipliers[ScrollSnapController.instance.currentIndex];
            }
            return 1f; // Default multiplier if no level matches
        }
    
    public void SetLevel()
        {
            levelPanelsArray[(int)currentLevel].SetActive(false);
            currentLevel = (Level)levelDropdown.value;
            levelPanelsArray[(int)currentLevel].SetActive(true);
            UpdateCashOutAmount();
        }
    

    }
}