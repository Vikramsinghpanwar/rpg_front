using System.Collections;
using Game.ChickenRoad;
using UnityEngine;
using UnityEngine.UI;

namespace Game.ChickenRoad2
{
    public class Tile : MonoBehaviour
    {
        public Image shieldImage;
        public Transform carObj;
        float moveDuration = 1f;
        Transform tile;
        float padding = 300f;
        GameObject barricade;
        Coroutine autoCar_Coroutine;
        public bool _isCarMoving;



        void Start()
        {
            tile = transform;
            shieldImage = transform.GetChild(0).GetComponent<Image>();
            barricade = transform.GetChild(2).gameObject;
            carObj = transform.GetChild(1);
            autoCar_Coroutine = StartCoroutine(AutoFire());
        }

        public void GoldenTile()
        {
            shieldImage.sprite = GameManager.instance.goldCoin;
            shieldImage.gameObject.SetActive(true);
            barricade.SetActive(false);
        }
        public void GreenTile()
        {
            Debug.Log("Green");
            shieldImage.sprite = GameManager.instance.greenShieldSprite;
            shieldImage.gameObject.SetActive(false);
            barricade.SetActive(true);
        }
        public void RedTile()
        {
            Debug.Log("red");
            shieldImage.sprite = GameManager.instance.redShieldSprite;
            barricade.SetActive(false);
        }
        public void BlueTile()
        {
            if (!shieldImage.gameObject.activeInHierarchy)
                shieldImage.gameObject.SetActive(true);

            shieldImage.sprite = GameManager.instance.blueShieldSprite;
            barricade.SetActive(false);
            if (autoCar_Coroutine != null)
            {
                StopCoroutine(autoCar_Coroutine);
                autoCar_Coroutine = StartCoroutine(AutoFire());
            }
        }

        IEnumerator AutoFire()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(2f, 10f));
                if (shieldImage.sprite == GameManager.instance.goldCoin || barricade.activeInHierarchy)
                {
                    StopCoroutine(autoCar_Coroutine);
                    break;
                }

                MoveCar();
            }
        }

        private IEnumerator MoveImageTopToBottom()
        {
            // Get top and bottom Y relative to the tile
            float tileHeight = tile.GetComponent<RectTransform>().rect.height;
            Vector3 localStart = new Vector3(0, tileHeight / 2f + padding, 0);  // Top of tile
            Vector3 localEnd = new Vector3(0, -tileHeight / 2f - padding, 0);   // Bottom of tile

            carObj.localPosition = localStart;

            float elapsedTime = 0f;
            while (elapsedTime < moveDuration)
            {
                carObj.localPosition = Vector3.Lerp(localStart, localEnd, elapsedTime / moveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            carObj.localPosition = localEnd;
            _isCarMoving = false;

        }

        private IEnumerator MoveImageTopToMiddle()
        {
            float localMoveDuration = moveDuration / 2f;
            // Get top and middle Y relative to the tile
            float tileHeight = tile.GetComponent<RectTransform>().rect.height;
            Vector3 localStart = new Vector3(0, tileHeight / 2f + padding, 0);  // Top of tile
            Vector3 localEnd = new Vector3(0, 100, 0);   // Middle of tile

            carObj.localPosition = localStart;

            float elapsedTime = 0f;
            while (elapsedTime < localMoveDuration)
            {
                carObj.localPosition = Vector3.Lerp(localStart, localEnd, elapsedTime / localMoveDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            carObj.localPosition = localEnd;
            GameManager.instance.brakeSound.Play();
        }

        public void ResetCar()
        {
            Debug.Log("Resetting Car Position");
                        float tileHeight = tile.GetComponent<RectTransform>().rect.height;

            carObj.localPosition = new Vector3(0, tileHeight / 2f + padding, 0);
        }

        public void MoveCar()
        {
            carObj.GetComponent<Image>().sprite = GameManager.instance.carsSpriteArray[Random.Range(0, GameManager.instance.carsSpriteArray.Length)];
            StartCoroutine(MoveImageTopToBottom());
            _isCarMoving = true;
        }

        public void BrakeCar()
        {
            carObj.GetComponent<Image>().sprite = GameManager.instance.carsSpriteArray[Random.Range(0, GameManager.instance.carsSpriteArray.Length)];
            StartCoroutine(MoveImageTopToMiddle());
            _isCarMoving = false;
        }


    }
}