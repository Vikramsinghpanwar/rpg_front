using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blower : MonoBehaviour
{
    public string c_tag;
    public float force = 1000f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == c_tag)
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            rb.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(0, 1f)) * force, ForceMode2D.Impulse);
        }
    }

}
