using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PawnTimer : MonoBehaviour
{

    [SerializeField] Image[] players;
    private float speed = 1f;
    public static PawnTimer instance;
    public static bool stopTimer;
    public float timerDuration = 7;
    private void Awake()
    {
        instance = this;
    }
    float i;

Coroutine timerCoroutine;
public void StartTimer(PawnType pawnType){
     if(GameLocalData.pawnType != pawnType)
    DiceController.instance.myChanceImage.SetActive(false);
    else DiceController.instance.myChanceImage.SetActive(true);
        if (timerCoroutine != null)
        {
            foreach (Image player in players)
            {
                player.fillAmount = 0;
            }
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    i = timerDuration;
    timerCoroutine = StartCoroutine(Timer(pawnType));
}
   
    IEnumerator Timer(PawnType pawntype)
    {
        Debug.Log("Turn of  : " + pawntype.ToString());
        i = timerDuration;
        int currentPawn = (int)pawntype;
        int selectedPawn = (int)GameLocalData.pawnType;
        int index = 0;
        if (selectedPawn <= 2)
        {
            index = selectedPawn <= currentPawn ? Mathf.Abs(selectedPawn - currentPawn) :
               Mathf.Abs(4 - currentPawn);
        }
        else
        {
            index = selectedPawn <= currentPawn ? Mathf.Abs(selectedPawn - currentPawn) :
            Mathf.Abs(4-(selectedPawn - currentPawn));
        }

        stopTimer = false;
        while (i > 0)
        {
            if (stopTimer)
            {
                players[index].fillAmount = 0;
                yield break;
            }

            i -= Time.deltaTime * speed; // Decrease over time
            players[index].fillAmount = i / timerDuration;
            yield return null;
        }
        players[index].fillAmount = 0;
        //timer khatam
        if (pawntype == GameLocalData.pawnType)
        {
            Debug.Log("my turn");
            ServerRequest.instance.SwitchPlayers();
            DiceController.instance.myChanceImage.SetActive(false);
            //DiceController.instance.OnClick();
        }
        yield return new WaitForEndOfFrame();
        players[index].fillAmount = 0;
    }

    public void ResetTimer()
    {
        i = timerDuration;
    }

}
