using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.ChickenRoad
{
    public class Tile : MonoBehaviour
    {
        public Image shieldImage;

        void Start()
        {
            shieldImage = transform.GetChild(0).GetComponent<Image>();
            StartCoroutine(AutoFire());
        }
         public void GoldenTile()
        {
            shieldImage.sprite = GameManager.instance.goldCoin;
            shieldImage.gameObject.SetActive(true);
        }
        public void GreenTile()
        {
            shieldImage.sprite = GameManager.instance.greenShieldSprite;
        }
        public void RedTile()
        {
            shieldImage.sprite = GameManager.instance.redShieldSprite;
        }
        public void BlueTile()
        {
            shieldImage.sprite = GameManager.instance.blueShieldSprite;
        }

        IEnumerator AutoFire()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(5f, 20f));
                if(transform.GetChild(1).gameObject.activeSelf)
                    continue; // Skip if the child is already active
                if (shieldImage.sprite == GameManager.instance.greenShieldSprite) continue;
                transform.GetChild(1).gameObject.SetActive(true);
                yield return new WaitForSeconds(2f);
                transform.GetChild(1).gameObject.SetActive(false);
            }
        }


    }
}