using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericWinElement : MonoBehaviour
{
	public SpriteRenderer winRankBackgroundSpriteRenderer;
	public SpriteRenderer winRankSpriteRenderer;

	public void SetColorAndSprite(Color _col, Sprite _sprite)
	{
		winRankBackgroundSpriteRenderer.color = _col;
		winRankSpriteRenderer.sprite = _sprite;
	}
}
