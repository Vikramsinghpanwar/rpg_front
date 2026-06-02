using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowPawn : PawnInputAnalizer
{
    private const PawnType _playerType = PawnType.yellow;
    private void Awake()
    {
        pawnType = _playerType;
    }

    public override void OnClick()
    {
        if (DiceController.instance.currentPawn != _playerType)
            return;
        base.OnClick();
    }
}
