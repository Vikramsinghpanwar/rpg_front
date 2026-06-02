using System.Collections;
using System.Collections.Generic;
using Features.Lobby.Integration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.ChickenRoad
{
    public class GameManager : MonoBehaviour
    {
        public long periodId;
        public Sprite greenShieldSprite;
        public Sprite redShieldSprite;
        public Sprite blueShieldSprite;

        public Hen activeHen;
        public Sprite cookedHenSprite;

        public static GameManager instance;
        public GameObject gameButtonsPanel;
        public Button goButton;
        public Button cashOutButton;
        public Transform tileRef;
        public Transform hen;
        public float distance;   // How far to move in the X direction
        public float time = 0.5f;    // Duration for the hen to move

        public Transform[] tiles_easy;
        public Transform[] tiles_medium;
        public Transform[] tiles_hard;
        public Transform[] tiles_hardCore;

        public AudioSource henSound;
        public GameObject cashOutPanel;
        public TextMeshProUGUI winAmountText;
        public TextMeshProUGUI winMultiplierText;
        public Sprite goldCoin;

        public TextMeshProUGUI walletTxt;

        private void Awake()
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
        // Start is called before the first frame update
        void Start()
        {
            walletTxt.text = "₹" + (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
            gameButtonsPanel.SetActive(false);
            distance = tileRef.GetComponent<RectTransform>().rect.width; // Get the width of the tile
        }

        public void StartGame()
        {
            if (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f < BetManager.instance.betAmount)
            {
                Debug.Log("low balance");
                //return;
            }

            periodId = long.Parse(System.DateTime.Now.ToString("yyyyMMddHHmmss"));
            Debug.Log("Game Started");
            gameButtonsPanel.SetActive(true);
            goButton.interactable = false;
            cashOutButton.interactable = false;
            activeHen = FindObjectOfType<Hen>();
            activeHen.ResetHen();
            Debug.Log("Position Reset");
            SocketManager.Instance.ConnectToServer();
            Wallet.DeductAmount(BetManager.instance.betAmount);
            walletTxt.text = "₹" + (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString("F2");
        }

        public void NextStep()
        {

            SocketManager.Instance.NextStep();
            goButton.interactable = false;
            cashOutButton.interactable = false;
        }

        public void MoveHenToNextTile()
        {
            Debug.Log("Moving Hen to Next Tile");
            StartCoroutine(MoveHen());
            ScrollSnapController.instance.ScrollNext();
            BetManager.instance.UpdateCashOutAmount();

        }

        private IEnumerator MoveHen()
        {
            activeHen.Run();
            Vector3 start = hen.localPosition;
            Vector3 end = start + new Vector3(distance, 0f, 0f);
            float elapsed = 0f;

            while (elapsed < time)
            {
                float t = elapsed / time;
                hen.localPosition = Vector3.Lerp(start, end, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            hen.localPosition = end;
            goButton.interactable = true;
            cashOutButton.interactable = true;
            activeHen.SetIdle();
            int indexS = ScrollSnapController.instance.currentIndex;

            tiles_easy[indexS].GetComponent<Tile>().GreenTile();
            if (indexS > 1)
                tiles_easy[indexS - 1].GetComponent<Tile>().GoldenTile();

        }
        public void CashOut()
        {
            // Logic to cash out
            Debug.Log("Cashed Out");
            SocketManager.Instance.CashOut();
        }

        public void CashOutSuccessfull()
        {
            Debug.Log("Cash Out Successful");
            StartCoroutine(CashOutCoroutine());
        }

        IEnumerator CashOutCoroutine()
        {
            winAmountText.text = "₹ " + (BetManager.instance.betAmount * BetManager.instance.GetCurrentMultiplier()).ToString("F2");
            winMultiplierText.text = "x" + BetManager.instance.GetCurrentMultiplier().ToString("F2");
            gameButtonsPanel.SetActive(false);
            cashOutPanel.SetActive(true);
            walletTxt.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString();
            yield return new WaitForSeconds(3f);
            ScrollSnapController.instance.ResetScroll();
            activeHen.ResetHen();
            cashOutPanel.SetActive(false);
            ResetGame();
        }

        public void EndGame()
        {
            MoveHenToNextTile();
            goButton.interactable = false;
            cashOutButton.interactable = false;
            StartCoroutine(EndGameCoroutine());
        }
        IEnumerator EndGameCoroutine()
        {
            henSound.Play();
            yield return new WaitForSeconds(0.5f);
            GameObject tile = tiles_easy[ScrollSnapController.instance.currentIndex].GetChild(1).gameObject;
            if (tile.gameObject.activeInHierarchy) tile.SetActive(false);
            tile.SetActive(true);

            activeHen.CookHen();
            tiles_easy[ScrollSnapController.instance.currentIndex].GetComponent<Tile>().RedTile();

            gameButtonsPanel.SetActive(false);
            yield return new WaitForSeconds(1f);
            ScrollSnapController.instance.ResetScroll();
            activeHen.ResetHen();
            ResetGame();
        }

        public void Lobby()
        {
            SceneManager.LoadScene(1);
        }
        void ResetGame()
        {
            switch (BetManager.instance.currentLevel)
            {
                case BetManager.Level.Easy:
                    for (int i = 1; i < tiles_easy.Length; i++)
                    {
                        tiles_easy[i].GetComponent<Tile>().BlueTile();
                    }
                    break;
                case BetManager.Level.Medium:
                    for (int i = 1; i < tiles_medium.Length; i++)
                    {
                        tiles_medium[i].GetComponent<Tile>().BlueTile();
                    }
                    break;
                case BetManager.Level.Hard:
                    for (int i = 1; i < tiles_hard.Length; i++)
                    {
                        tiles_hard[i].GetComponent<Tile>().BlueTile();
                    }
                    break;
                case BetManager.Level.Hardcore:
                    for (int i = 1; i < tiles_hardCore.Length; i++)
                    {
                        tiles_hardCore[i].GetComponent<Tile>().BlueTile();
                    }
                    break;
            }
        }
    }
}


