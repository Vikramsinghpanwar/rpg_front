
using System.Diagnostics;
public class PawnInputAnalizer : PawnMovementController
{
    public virtual void OnClick()
    {
        CheckHome();
        if (!isLeftTheHouse)
            return;

        if (richedTheDestination)
            return;

        if (!EnoughSpotsLeft(DiceController.instance.currentDiceValue))
            return;

        if (DiceController.instance.playerCanMove)
        {
            StartCoroutine(MoveTo(DiceController.instance.currentDiceValue));
            DiceController.instance.playerCanMove = false;
            DiceController.instance.RemoveAllHighlights();
        }
    }
}
