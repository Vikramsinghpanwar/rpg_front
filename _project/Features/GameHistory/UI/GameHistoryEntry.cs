using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Features.GameHistory.Controllers;
using Features.GameHistory.Models;

namespace Features.GameHistory.UI
{
    public class GameHistoryEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text gameNameText;
        [SerializeField] private TMP_Text gameTypeText;
        [SerializeField] private TMP_Text netResultText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text roomIdText;
        [SerializeField] private TMP_Text roundIdText;
        [SerializeField] private TMP_Text debitText;
        [SerializeField] private TMP_Text creditText;
        [SerializeField] private TMP_Text idText;
        [SerializeField] private Image netResultBadge;

        public void Setup(GameHistoryItem entry)
        {
            if (gameNameText != null)
                gameNameText.text = entry.game_name ?? "";

            if (gameTypeText != null)
                gameTypeText.text = entry.game_type ?? "";

            if (netResultText != null)
            {
                netResultText.text = GameHistoryController.FormatPaisa(entry.net_result);
                netResultText.color = GameHistoryController.GetNetResultColor(entry.net_result);
            }

            if (dateText != null)
                dateText.text = GameHistoryController.FormatDate(entry.played_at);

            if (roomIdText != null)
                roomIdText.text = entry.room_id ?? "";

            if (roundIdText != null)
                roundIdText.text = entry.round_id ?? "";

            if (debitText != null)
                debitText.text = GameHistoryController.FormatPaisa(entry.debit_amount);

            if (creditText != null)
                creditText.text = GameHistoryController.FormatPaisa(entry.credit_amount);

            if (idText != null)
                idText.text = entry.id ?? "";

            if (netResultBadge != null)
                netResultBadge.color = GameHistoryController.GetNetResultColor(entry.net_result);
        }
    }
}
