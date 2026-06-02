using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveContinously : MonoBehaviour
{
    public float speed = 5f;
    public DotController dControllerH;
    public Vector3 direction = Vector3.right;
    public bool _is;
    public bool _Vertical;
    void Update()
    {
        if (_is)
        {
            transform.Translate(direction * speed * Time.deltaTime);

        }
        
    }
    private void Start()
    {
        dControllerH = GetComponentInParent<DotController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if(collision.gameObject.tag == "ePoint")
        {
            if (_Vertical)
            {
                gameObject.transform.localPosition = new Vector2(0, dControllerH.sPoint.y);

            }
            else
            {
                gameObject.transform.localPosition = new Vector2(dControllerH.sPoint.x, 0);

            }
        }
    }
}
