using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.Translate(new Vector2(-100, -100));
        Invoke("Del",2f);
    }

    public void Del()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
