using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppingPlayer : MonoBehaviour
{
    private void OnEnable()
    {
        gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector3(0, -10f, 0));
        Invoke("DeleteMe", 2f);
    }

    public void DeleteMe()
    {
        Destroy(gameObject);
    }
}
