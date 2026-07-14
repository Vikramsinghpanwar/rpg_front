using System.Collections.Generic;
using Features.Recharge.Models;

namespace Features.Recharge.Utils
{
    public static class RechargeBonusCalculator
    {
        public static long GetApplicableBonus(long enteredAmountPaisa, List<RechargeAmountPreset> presets)
        {
            if (enteredAmountPaisa <= 0 || presets == null || presets.Count == 0)
                return 0;

            long bestBonus = 0;
            foreach (var preset in presets)
            {
                if (preset.amount_paisa <= enteredAmountPaisa)
                {
                    bestBonus = preset.bonus_amount_paisa;
                }
                else
                {
                    break;
                }
            }
            return bestBonus;
        }
    }
}
