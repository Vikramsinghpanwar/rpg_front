using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunInBackground : MonoBehaviour
{
    private void OnEnable()
    {
        Application.runInBackground = true;
    }
}
