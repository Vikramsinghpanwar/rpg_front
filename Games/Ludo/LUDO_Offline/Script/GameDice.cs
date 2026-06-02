using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDice : MonoBehaviour
{
	public GameObject diceGameObject;
	public GameObject superKingIconGameObject;
	public GameObject diceIconGameObject;
	public GameObject diceBKGameObject;
	public Sprite[] diceFace;
	public Sprite zeroFace;
	public PolygonCollider2D diceCollider;
	public Color normalColor;
	public Color hiddenColor;
	public Color middleIconNormalColor;
	public Color middleIconHiddenColor;
	public int diceID;
	GameLogic GameLogicRef;
	AudioFX AudioFXRef;
	float timeForUnrollNRoll;

	public Vector3 scaleTo;
	Vector3 initialScale;

    private void Awake()
    {
		initialScale = superKingIconGameObject.transform.localScale;
	}

	public void OnMouseDown()
	{
		//Debug.Log("I AM CLICKED => " + gameObject.name);
		//Disable my all ongoing tweens and perform click
		StopTapDiceTween();
		//diceIconGameObject.SetActive(false);
		GameLogicRef.GenerateDiceCount(diceID);
	}

	public void PlayRollDiceTween(int rolledDiceFace)
	{
		//DEBUG
		//if (rolledDiceFace >= 6)
		//{
		//	rolledDiceFace = 5;
		//}
		diceGameObject.transform.localScale = Vector2.one;
		diceGameObject.transform.localRotation = Quaternion.identity;

		LeanTween.rotateZ(diceGameObject, -720F, timeForUnrollNRoll).setEaseOutQuart();
		AudioFXRef.DiceUnRoll();
		LeanTween.scale(diceGameObject, Vector2.one * 0.1F, timeForUnrollNRoll).setEaseOutQuart().setOnComplete(() =>
		{
			diceIconGameObject.SetActive(false);

			diceGameObject.GetComponent<SpriteRenderer>().sprite = diceFace[rolledDiceFace - 1];
			LeanTween.rotateZ(diceGameObject, 720F, timeForUnrollNRoll).setEaseOutQuart();
			//AudioFXRef.DiceRoll();
			LeanTween.scale(diceGameObject, Vector2.one, timeForUnrollNRoll).setEaseOutQuart();
		});
	}

	public void SetDiceProperties(int _id, GameLogic _gameLogicRef, AudioFX _AudioFXRef)
	{
		diceID = _id;
		GameLogicRef = _gameLogicRef;
		AudioFXRef = _AudioFXRef;
		gameObject.SetActive(true);
		timeForUnrollNRoll = GameLogicRef.SPEED_FOR_ROLLDICE;
		//Debug.Log(timeForUnrollNRoll);
	}

	public void EnableCollider()
	{
		if (!diceCollider.enabled)
		{
			//diceIconGameObject.SetActive(true);
			diceCollider.enabled = true;
		}
	}

	public void DisableCollider()
	{
		if (diceCollider.enabled)
		{
			diceIconGameObject.SetActive(false);
			diceCollider.enabled = false;
		}
	}

	public void PlayTapDiceTween()
	{
		superKingIconGameObject.SetActive(true);
		diceIconGameObject.SetActive(true);
		LeanTween.scale(superKingIconGameObject, scaleTo, 1F).setEase(LeanTweenType.punch).setLoopCount(-1);
	}

	public void StopTapDiceTween()
	{
		superKingIconGameObject.SetActive(false);

		if (LeanTween.isTweening(superKingIconGameObject))
		{
			LeanTween.cancel(superKingIconGameObject, false);
			superKingIconGameObject.transform.localScale = initialScale;
		}
	}

	public void ShowDice()
	{
		diceGameObject.GetComponent<SpriteRenderer>().color = normalColor;
		diceGameObject.GetComponent<SpriteRenderer>().sprite = zeroFace;
		diceBKGameObject.SetActive(true);
		superKingIconGameObject.GetComponent<SpriteRenderer>().color = middleIconNormalColor;
		//diceIconGameObject.SetActive(true);
	}

	public void HideDice()
	{
		diceGameObject.GetComponent<SpriteRenderer>().color = hiddenColor;
		diceGameObject.GetComponent<SpriteRenderer>().sprite = zeroFace;
		diceBKGameObject.SetActive(false);
		superKingIconGameObject.GetComponent<SpriteRenderer>().color = middleIconHiddenColor;
		superKingIconGameObject.SetActive(true);
		//diceIconGameObject.SetActive(false);
	}

	public void InitializeScaleAndRotation()
	{
		diceGameObject.transform.localScale = Vector2.one;
		diceGameObject.transform.localRotation = Quaternion.identity;
	}
}
