using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Teenpatti;
using Teenpatti;
using System.Linq;
using Features.Lobby.Integration;
public class CardsDeal : MonoBehaviour
{
    public Transform deckPosition;
    public GameObject cardPrefab;
    public float dealDelay = 0.15f;
    public List<PlayerSeat> allPlayerSeats = new List<PlayerSeat>();
    public List<PlayerSeat> activePlayers = new List<PlayerSeat>();


    [System.Serializable]
    public class PlayerSeat
    {
        public Transform seatTransform;
        public Transform[] cardSlots; // size = 3

        public bool isActive;
    }

    public IEnumerator DealCards()
    {
        int pivotIndex = -1;
        var playerDetailArray = Teenpatti.GameLiveData.instance.playerDetailsArray;
        for (int i = 0; i < playerDetailArray.Length; i++)
        {
            if (playerDetailArray[i] == null) continue;
            if (playerDetailArray[i].id == "") continue;
            if (playerDetailArray[i].userId == BootstrapLobbyAdapter.GetUserId())
            {
                pivotIndex = i;
                break;
            }
        }
        playerDetailArray = playerDetailArray.Skip(pivotIndex).Concat(playerDetailArray.Take(pivotIndex)).ToArray();

        int totalRounds = 3;
        activePlayers.Clear();
        for (int i = 0; i < allPlayerSeats.Count; i++)
        {
            if (playerDetailArray[i] == null) continue;

            if (playerDetailArray[i].isActive)
            {
                activePlayers.Add(allPlayerSeats[i]);
            }
        }
        // yield return new WaitForSeconds(2f);

        for (int round = 0; round < totalRounds; round++)
        {
            foreach (PlayerSeat player in activePlayers)
            {
                GameObject card = Instantiate(cardPrefab, deckPosition.position, Quaternion.identity);
                card.transform.SetParent(deckPosition);
                card.transform.localScale = new Vector3(1, 1, 1);
                Transform targetSlot = player.cardSlots[round];

                StartCoroutine(MoveCard(card.transform, targetSlot.position, round));

                yield return new WaitForSeconds(dealDelay);
            }
        }
        Teenpatti.GameManager.Instance.PreBets();
        ActivePlayerCards();
        yield return new WaitForSeconds(0.5f);
        RemoveDealedCards();
    }


    IEnumerator MoveCard(Transform card, Vector3 targetPos, int round)
    {
        card.DOMove(targetPos, 0.35f)
            .SetEase(Ease.OutQuad);

        card.DORotate(new Vector3(0, 0, round * (-20) + 20), 0.35f); // optional flip

        yield return null;
    }

    public void RemoveDealedCards()
    {
        foreach (Transform card in deckPosition)
        {
            Destroy(card.gameObject);
        }

        Teenpatti.GameManager.Instance.OnCardsDealComplete();
    }

    public void ActivePlayerCards()
    {
        foreach (var player in Teenpatti.GameManager.Instance.playersList)
        {
            if (!player.isVacant && !player.isSpectator)
            {
                player.CardObj.SetActive(true);
            }
        }
    }

}


