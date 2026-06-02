using System.Collections;
using UnityEngine;

public class TimerKQ : MonoBehaviour
{

    private void Start()
    {
        // Start the timer coroutine
        StartCoroutine(StartTimer(15f));
    }

    private IEnumerator StartTimer(float duration)
    {
        for(int i = 0; i< duration; i++)
        {
            yield return new WaitForSeconds(1f);
        }
        // Wait for the specified duration

        // Call your custom function after the timer expires
        CallFunctionAfterTimer();


    }

    private void CallFunctionAfterTimer()
    {
        // Add your custom function logic here
        Debug.Log("Function called after 15 seconds!");
    }
}
