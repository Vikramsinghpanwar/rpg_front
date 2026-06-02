using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
	[Header("Piece Elements")]
	public SpriteRenderer pieceColorSpriteRenderer;
	public SpriteRenderer pieceShadowSpriteRenderer;
	public SpriteRenderer piecePlusSignSpriteRenderer;
	public SpriteRenderer pieceHighlighterSpriteRenderer;

	public Transform transformToMove;
	public Transform transformToScale;
	public Transform piecePlusSign;
	//public Transform highlighterIcon;
	public CircleCollider2D pieceCollider;

	Vector2 homePosition;
	GameLogic GameLogicRef;

	public int playerID;
	public int pieceID;

	public int currentIndex = -1;
	public int currentGlobalIndex = -1;

	public bool isScaleDown = false;	

	private void OnMouseDown()
	{
		//Debug.Log("I AM CLICKED => " + gameObject.name + playerID + "," + pieceID);
		GameLogicRef.MovePiece(playerID, pieceID);
	}

	public bool IsMovable(int diceCount)
	{
		if (currentIndex != -1 && currentIndex != 56 && currentIndex + diceCount < 57)
		{
			return true;
		}
		else if (currentIndex == -1 && diceCount == 6)
		{
			return true;
		}

		return false;
	}

	public bool IsReachedOnDestination()
	{
		if (currentIndex == 56)
		{
			return true;
		}
		return false;
	}

	public bool IsOnHome()
	{
		if (currentIndex == -1)
		{
			return true;
		}
		return false;
	}

	public void EnableMovableIndication()
	{
		piecePlusSign.gameObject.SetActive(true);
		if (!LeanTween.isTweening(piecePlusSign.gameObject))
		{
			LeanTween.rotateAroundLocal(piecePlusSign.gameObject, Vector3.forward, 180F, 0.7F).setEaseOutCubic().setLoopCount(-1);
		}
	}

	public void DisableMovableIndication()
	{
		piecePlusSign.gameObject.SetActive(false);
		if (LeanTween.isTweening(piecePlusSign.gameObject))
		{
			LeanTween.cancel(piecePlusSign.gameObject);
			LeanTween.rotateZ(piecePlusSign.gameObject, 0F, 0.5F).setEaseOutCubic();
		}
	}

	public void EnableCollider()
	{
		if (!pieceCollider.enabled)
		{
			pieceCollider.enabled = true;
		}
	}

	public void DisableCollider()
	{
		if (pieceCollider.enabled)
		{
			pieceCollider.enabled = false;
		}
	}


	public void SetPieceProperties(Sprite sprite, Vector2 homeposition, GameLogic gameLogicRef, int playerid, int pieceid)
	{
		homePosition = homeposition;
		transformToMove.localPosition = homeposition;
		pieceColorSpriteRenderer.sprite = sprite;
		GameLogicRef = gameLogicRef;
		playerID = playerid;
		pieceID = pieceid;

		DisableMovableIndication();
		DisableCollider();
	}

	public void ResetPiecePropertiesOnKill()
	{
		LeanTween.moveLocal(transformToMove.gameObject, homePosition, GameLogicRef.SPEED_FOR_PIECEMOVE_BACKWARD).setEaseLinear();
		currentIndex = -1;
		currentGlobalIndex = -1;
	}

	public void SetSortingLayerTo(string _sortingLayerName)
	{
		pieceColorSpriteRenderer.sortingLayerName = _sortingLayerName;
		pieceShadowSpriteRenderer.sortingLayerName = _sortingLayerName;
		piecePlusSignSpriteRenderer.sortingLayerName = _sortingLayerName;
		pieceHighlighterSpriteRenderer.sortingLayerName = _sortingLayerName;
	}
}
