using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Teenpatti;
using System;
using Unity.Mathematics;
using Core.Bootstrap;
using Features.Lobby.Integration;

namespace Teenpatti
{

    public class PlayerManager : MonoBehaviour
    {

        public string myCardsType;
        public int myIndex;
        [Header("UI References")]
        public Image profileImg;
        public GameObject CardObj;
        public TextMeshProUGUI nameTxt;
        public TextMeshProUGUI myAmountTxt;
        public Image[] cardsList;
        public Sprite[] cardsSpriteList;
        public GameObject CardTypeObj;
        public Image timerImg;
        public TextMeshProUGUI bidAmountTxt;
        public Image Indicator;

        public AudioSource myAudioSouce;

        [Header("Player Data")]
        public string myId;
        public string status;
        public int position;
        public bool isLocalPlayer = false;
        public bool hasFolded = false;
        public float amountInvested;
        public bool hasSeenCards = false;
        public GameObject popupObj;

        [Header("Timer")]
        public Animator playerAnimator;

        public GameObject dealerBadge;
        public GameObject popup;

        public void StartRecharging(float timeout)
        {
            TimerStart(timeout, timeout);
        }

        public void PlayThunderImpact()
        {
            StartCoroutine(Impact());
        }

        IEnumerator Impact()
        {
            Color original = profileImg.color;

            profileImg.color = Color.yellow;

            yield return new WaitForSeconds(0.1f);

            profileImg.color = original;
        }

        private void Start()
        {
            CardObj = transform.GetChild(0).gameObject;
            timerImg.fillAmount = 0f;
            playerAnimator = gameObject.GetComponent<Animator>();
            amountInvested = 0;
            myAudioSouce = gameObject.GetComponent<AudioSource>();
        }

        public void FillData(string name, float amount, string[] cards = null)
        {
            nameTxt.text = name;
        }

        public void InvestAmount(float amount)
        {
            amountInvested += amount;
            myAmountTxt.text = amountInvested.ToString("F2");
            bidAmountTxt.text = amount.ToString("F2");

            if (isLocalPlayer && GameManager.Instance != null)
            {
                Wallet.DeductAmount(amount);
                GameManager.Instance.userWalletTxt.text = GameMode.mode == GameMode.Modes.privateGame ? (BootstrapService.Instance.Wallet.win_balance / 100f + BootstrapService.Instance.Wallet.deposit_balance / 100f).ToString("F2") : (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
            }
        }

        public void DeclinedSideShow()
        {
            ShowPOPUP(GameManager.Instance.declinedSpr);
        }

        public void CheckCards()
        {
            if (cardsSpriteList == null || cardsSpriteList.Length < 3) return;

            for (int i = 0; i < 3; i++)
            {
                if (i < cardsList.Length && i < cardsSpriteList.Length)
                {
                    cardsList[i].sprite = cardsSpriteList[i];
                    cardsList[i].gameObject.SetActive(true);
                }
            }

            CardTypeObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = myCardsType;
            CardTypeObj.SetActive(true);


            if (isLocalPlayer && ActionButtons.instance != null)
                ActionButtons.instance.checkBtn.SetActive(false);
        }

        public void MarkAsDealer()
        {
            dealerBadge.SetActive(true);
            Debug.Log("Dealer status: " + dealerBadge.activeInHierarchy);
        }

        public void PopulateCards(string[] cards)
        {
            cardsSpriteList = new Sprite[cards.Length];
            for (int i = 0; i < cards.Length; i++)
            {
                cardsSpriteList[i] = GameManager.Instance.GetSprite(cards[i]);
            }
        }

        public void ShowCards(Sprite[] cards)
        {
            for (int i = 0; i < cardsList.Length; i++)
            {
                cardsList[i].sprite = cards[i];
            }
        }

        public void ShowHandRank(string rank)
        {
            if (Indicator.gameObject.activeInHierarchy) Indicator.gameObject.SetActive(false);
            CardTypeObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = rank;
            CardTypeObj.SetActive(true);
        }


        public void FirstBet()
        {
            InvestAmount(GameLiveData.instance.lastBetAmount);
            playerAnimator.SetTrigger("_chaal");
            ActionTaken();
            bidAmountTxt.text = GameLiveData.instance.lastBetAmount.ToString("F2");
        }

        public void ShowPOPUP(Sprite spr)
        {
            popup.transform.GetChild(0).GetComponent<Image>().sprite = spr;
            popup.SetActive(true);
        }

        public void ShowIndicator(Sprite spr)
        {
            Indicator.gameObject.SetActive(true);
            Indicator.sprite = spr;
        }




        public void Pack()
        {
            if (GameManager.Instance.playerRechargingPanel.activeInHierarchy) GameManager.Instance.playerRechargingPanel.SetActive(false);

            if (isLocalPlayer)
            {
                CheckCards();
                foreach (var p in GameLiveData.instance.playerDetailsArray)
                {
                    if (p == null) continue;
                    if (p.userId == BootstrapLobbyAdapter.GetUserId() && !p.isActive)
                    {
                        ActionButtons.instance.ImBack_Panel.SetActive(true);
                        break;
                    }
                }
            }
            myAudioSouce.PlayOneShot(GameManager.Instance.packAC);
            playerAnimator.SetBool("_isPacked", true);
            status = "packed";
            hasFolded = true;
            ShowIndicator(GameManager.Instance.packSpr);
            ShowMsg("Packed");
        }

        public void Lose()
        {
            if (isLocalPlayer)
            {
                CheckCards();
                foreach (var p in GameLiveData.instance.playerDetailsArray)
                {
                    if (p == null) continue;
                    if (p.userId == BootstrapLobbyAdapter.GetUserId() && !p.isActive)
                    {
                        ActionButtons.instance.ImBack_Panel.SetActive(true);
                        break;
                    }
                }
            }
            myAudioSouce.PlayOneShot(GameManager.Instance.packAC);
            playerAnimator.SetBool("_isPacked", true);
            status = "packed";
            hasFolded = true;
            ShowIndicator(GameManager.Instance.packSpr);
            ShowMsg("Lose");
        }

        public void Chaal(float amount)
        {
            myAudioSouce.PlayOneShot(GameManager.Instance.addMoneyAC);
            ShowPOPUP(GameManager.Instance.chaalSpr);
            InvestAmount(amount);

            playerAnimator.SetTrigger("_chaal");
            ActionTaken();
        }
        public void HandleShow(float amount)
        {
            myAudioSouce.PlayOneShot(GameManager.Instance.addMoneyAC);
            ShowMsg("Show");
            InvestAmount(amount);
            playerAnimator.SetTrigger("_chaal");
            ActionTaken();

        }

        public void AddMoneyToPool(float amount)
        {
            myAudioSouce.PlayOneShot(GameManager.Instance.addMoneyAC);
            InvestAmount(amount);
            playerAnimator.SetTrigger("_chaal");
            ActionTaken();
        }


        public void Chaal2X(float amount)
        {
            myAudioSouce.PlayOneShot(GameManager.Instance.addMoneyAC);
            InvestAmount(amount);
            ShowPOPUP(GameManager.Instance.chaal2xSpr);
            playerAnimator.SetTrigger("_chaal");
            ActionTaken();
        }

        public void Blind(float amount)
        {
            myAudioSouce.PlayOneShot(GameManager.Instance.addMoneyAC);
            InvestAmount(amount);
            ShowPOPUP(GameManager.Instance.blindSpr);
            playerAnimator.SetTrigger("_chaal");
            ActionTaken();
        }

        public void Blind2X(float amount)
        {
            myAudioSouce.PlayOneShot(GameManager.Instance.addMoneyAC);
            InvestAmount(amount);
            ShowPOPUP(GameManager.Instance.blind2xSpr);
            playerAnimator.SetTrigger("_chaal");
            ActionTaken();
        }

        public void Show()
        {
            CheckCards();
        }
        public void MarkSeen()
        {
            if (isLocalPlayer)
            {
                CheckCards();
            }
            hasSeenCards = true;
            ShowIndicator(GameManager.Instance.seenSpr);
            ShowMsg("Seen");
        }

        public void SetAsSeen()
        {
            hasSeenCards = true;
            //ShowMsg("Seen");
        }

        void ActionTaken()
        {
            Invoke("DelayedPotUpdate", 1f);
        }

        void DelayedPotUpdate()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UpdatePotAmount();
            }
        }




        public void Connected(string name, float amount)
        {
            FillData(name, amount, null);
        }

        public void TimerStart(float timeRemaining = 30, float timeout = 30)
        {
            if (isLocalPlayer)
            {
                myAudioSouce.PlayOneShot(GameManager.Instance.myTurnAC);
#if UNITY_ANDROID || PLATFORM_ANDROID
                if (PlayerPrefs.GetInt("isVibrationOn") == 1)
                    Handheld.Vibrate();
#endif

            }


            if (GameManager.Instance != null && GameManager.Instance.timerCoroutine != null)
            {
                GameManager.Instance.StopCoroutine(GameManager.Instance.timerCoroutine);
            }

            GameManager.Instance.timerCoroutine = StartCoroutine(DecreaseOverTime(timerImg, timeRemaining, timeout));

            if (isLocalPlayer && ActionButtons.instance != null)
            {
                ActionButtons.instance.ShowButtonsPanel();
            }
            else if (ActionButtons.instance != null)
            {
                ActionButtons.instance.HideButtonsPanel();
            }
        }

        public void ActionTaken(string action, int amount)
        {
            if (action != "seen" && action != "react_emoji" && action != "react_msg")
            {
                if (isLocalPlayer)
                {
                    ActionButtons.instance.HideButtonsPanel();
                }
                StopAllCoroutines();
                timerImg.fillAmount = 0;
            }

            switch (action.ToLower())
            {
                case "chaal":
                    Chaal(amount);
                    break;
                case "chaal2x":
                    Chaal2X(amount);
                    break;
                case "blind":
                    Blind(amount);
                    break;
                case "blind2x":
                    Blind2X(amount);
                    break;
                case "pack":
                case "fold":
                    Pack();
                    break;
                case "show":
                    Show();
                    break;
                case "seen":
                    MarkSeen();
                    break;
                case "react_emoji":
                    React(0, amount);
                    break;
                case "react_msg":
                    React(1, amount);
                    break;
                default:
                    Debug.Log("Invalid Action Request: " + action);
                    break;
            }
        }


        public void React(int type, int value)
        {
            if (type == 0)
            {
                GameObject emoji = Instantiate(EmojiManager.instance.emojiPrefabs[value], transform.GetChild(1));
                StartCoroutine(DestroyAfter(emoji, 2f));
            }
            else
            {
                ShowMsg(EmojiManager.instance.react_msgs[value]);
            }


        }

        IEnumerator DestroyAfter(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(obj);
        }
        IEnumerator DeactivateAfter(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            obj.SetActive(false);
        }

        public void ShowMsg(string msg)
        {
            popupObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = msg;
            popupObj.gameObject.SetActive(true);
            StartCoroutine(DeactivateAfter(popupObj, 2f));

        }

        IEnumerator DecreaseOverTime(Image img, float timeRemaining = 30, float duration = 30)
        {
            float startTime = duration - timeRemaining;
            float timer = startTime;
            img.fillAmount = 1f;

            Color green = Color.green;
            Color yellow = Color.yellow;
            Color red = Color.red;

            const float alpha = 0.5f;

            while (timer <= duration)
            {
                float normalized = timer / duration;

                // Fill amount
                img.fillAmount = 1f - normalized;

                // Color transition
                Color currentColor;
                if (normalized < 0.5f)
                {
                    currentColor = Color.Lerp(green, yellow, normalized / 0.5f);
                }
                else
                {
                    currentColor = Color.Lerp(yellow, red, (normalized - 0.5f) / 0.5f);
                }

                currentColor.a = alpha; // 🔑 enforce opacity
                img.color = currentColor;

                timer += Time.deltaTime;
                yield return null;
            }

            img.fillAmount = 0f;
            img.color = new Color(red.r, red.g, red.b, alpha);

            // Auto fold
        }

        public void Win()
        {
            playerAnimator.SetTrigger("win");
        }

        public void SetTurn(bool isTurn)
        {
            timerImg.fillAmount = isTurn ? 1f : 0f;
            timerImg.gameObject.SetActive(isTurn);
        }


        // [Header("Spectator & Vacant States")]
        // public GameObject spectatorBadge;
        // public GameObject vacantBadge;
        // public TextMeshProUGUI vacantText;
        public bool isSpectator = false;
        public bool isVacant = false;

        public void MarkAsSpectator(bool spectator)
        {
            isSpectator = spectator;

            // if (spectatorBadge != null)
            //     spectatorBadge.SetActive(spectator);

            if (spectator)
            {
                // Disable interaction for spectators
                SetVacant(false);
                profileImg.sprite = GameManager.Instance.spectatorProfileSpr;
                nameTxt.text = "Spectator";

                // Hide cards and chips for spectators
                CardObj.SetActive(false);
            }
        }

        public void PopulateWithPlayer(int profile, string username)
        {
            profileImg.sprite = GameManager.Instance.GetAvatarByIndex(profile);
            profileImg.color = new Color(255f, 255f, 255f, 1f);
            nameTxt.text = username;
        }

        public void SetVacant(bool vacant, string playerName = "Vacant")
        {
            isVacant = vacant;

            // if (vacantBadge != null)
            //     vacantBadge.SetActive(vacant);

            if (vacant)
            {
                myId = "";
                // Show vacant seat
                profileImg.sprite = GameManager.Instance.emptyProfileSpr;
                nameTxt.text = playerName;
                // Hide player-specific elements
                CardObj.SetActive(false);
                // spectatorBadge.SetActive(false);
                dealerBadge.SetActive(false);

                // Set color to indicate vacant
                profileImg.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
            else
            {
                profileImg.color = Color.white;
            }
        }

        public void PromoteToPlayer(string username, float chips)
        {
            isSpectator = false;

            // if (spectatorBadge != null)
            //     spectatorBadge.SetActive(false);

            // if (vacantBadge != null)
            //     vacantBadge.SetActive(false);

            // Restore player UI
            nameTxt.text = username;
            myAmountTxt.text = chips.ToString();
            myAmountTxt.gameObject.SetActive(true);
            bidAmountTxt.gameObject.SetActive(true);
            profileImg.color = Color.white;

            // Show empty card slots
            CardObj.SetActive(true);
            foreach (var card in cardsList)
            {
                card.sprite = GameManager.Instance.cardBackSprite;
                card.gameObject.SetActive(true);
            }
        }

        // Update ResetPlayer method:
        public void ResetPlayer()
        {
            status = "";
            CardTypeObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
            CardTypeObj.SetActive(false);
            amountInvested = 0;
            playerAnimator.SetBool("_isPacked", false);
            //ActionButtons.instance.checkBtn.SetActive(true);
            bidAmountTxt.text = "0";
            hasSeenCards = false;



            hasFolded = false;
            Indicator.gameObject.SetActive(false);
            playerAnimator.ResetTrigger("win");
            CardObj.SetActive(false);
            dealerBadge.SetActive(false);
            // spectatorBadge.SetActive(false);
            // vacantBadge.SetActive(false);
            isSpectator = false;
            isVacant = false;

            foreach (var card in cardsList)
            {
                card.sprite = GameManager.Instance.cardBackSprite;
            }

            cardsSpriteList = null;
            profileImg.color = Color.white;
        }


    }
}