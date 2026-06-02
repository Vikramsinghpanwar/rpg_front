using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimBallPos : MonoBehaviour
{
    public GameObject latestCollisionObj = null;
    // Start is called before the first frame update
  

    private void OnTriggerEnter2D(Collider2D collision)
    {
        latestCollisionObj = collision.gameObject;
    }
}
