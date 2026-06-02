using System.Collections;
using System.Collections.Generic;
using Features.Lobby.Integration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.ChickenRoad2
{
    public class GameManager : MonoBehaviour
    {
        public AudioSource brakeSound;
        public Sprite[] carsSpriteArray;
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

        public TextMeshProUGUI walletTxt;
        public Sprite goldCoin;

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
            walletTxt.text = (BootstrapLobbyAdapter.GetWalletBalanceTotal() / 100f).ToString();
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
        }

        public void NextStep()
        {

            SocketManager.Instance.NextStep();
            goButton.interactable = false;
            cashOutButton.interactable = false;
        }

        public void MoveHenToNextTile(bool die = false)
        {
            Debug.Log("Moving Hen to Next Tile");
            StartCoroutine(MoveHen(die));
            BetManager.instance.UpdateCashOutAmount();
        }

        private IEnumerator MoveHen(bool die)
        {
            if (tiles_easy[ScrollSnapController.instance.currentIndex + 1].GetComponent<Tile>()._isCarMoving && !die) yield return new WaitForSeconds(1f);
            ScrollSnapController.instance.ScrollNext();

            activeHen.Run();
            Vector3 start = hen.localPosition;
            Vector3 end = start + new Vector3(distance, 0f, 0f);
            float elapsed = 0f;
            if (!die)
            {
                if (Random.Range(0, 10) < 3)
                {
                    if (ScrollSnapController.instance.currentIndex > 1)
                    {
                        tiles_easy[ScrollSnapController.instance.currentIndex].GetComponent<Tile>().BrakeCar();
                    }
                }

            }


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

            if (!die)
                tiles_easy[indexS].GetComponent<Tile>().GreenTile();
            if (indexS > 1)
                tiles_easy[indexS - 1].GetComponent<Tile>().GoldenTile();
            tiles_easy[indexS - 1].GetComponent<Tile>().ResetCar();
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
            MoveHenToNextTile(true);
            goButton.interactable = false;
            cashOutButton.interactable = false;
            StartCoroutine(EndGameCoroutine());
        }
        IEnumerator EndGameCoroutine()
        {
            henSound.Play();
            tiles_easy[ScrollSnapController.instance.currentIndex].GetComponent<Tile>().MoveCar();

            yield return new WaitForSeconds(0.5f);
            tiles_easy[ScrollSnapController.instance.currentIndex].GetChild(1).gameObject.SetActive(true);
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
                        tiles_easy[i].GetComponent<Tile>().ResetCar();
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


