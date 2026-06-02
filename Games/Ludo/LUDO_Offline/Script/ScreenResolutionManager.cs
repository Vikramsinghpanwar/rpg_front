using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenResolutionManager : MonoBehaviour
{
	public Transform gameboard;

	void Start()
	{
		//Ref variables
		float refHeight = 854F;
		float refWidth = 480F;
		float refRatio = refWidth / refHeight;
		float refScale = 1F;// 0.012096F;

		//Current variables
		float currentRatio = (float)Screen.width / (float)Screen.height;

		//Calculation
		float currentScale = (currentRatio * refScale) / refRatio;

		if (currentScale <= 1F)
		{
			gameboard.localScale = new Vector2(currentScale, currentScale);
		}
	}
}
