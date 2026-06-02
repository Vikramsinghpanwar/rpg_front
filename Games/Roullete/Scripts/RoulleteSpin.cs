using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RouletteSpin : MonoBehaviour
{
    public float initialTorque = 500f;
    public float spinDeceleration = 0.99f;
    public float minAngularVelocity = 0.1f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Mathf.Abs(rb.angularVelocity) < minAngularVelocity)
        {
            rb.angularVelocity = 0f;
        }
        else
        {
            rb.angularVelocity *= spinDeceleration;
        }
    }

    public void Spin()
    {
        rb.AddTorque(initialTorque, ForceMode2D.Impulse);
    }
}
