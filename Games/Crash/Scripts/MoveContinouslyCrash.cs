/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveContinouslyCrash : MonoBehaviour
{
    public float speed = 5f;
    public DotControllerCrash dControllerH;
    public Vector3 direction = Vector3.right;
    public bool _is;
    public bool _Vertical;
    void Update()
    {
        if (_is)
        {
            transform.Translate(direction * speed * Time.deltaTime);
            if (_Vertical)
            {
                speed += 0.01f;
            }
        }
        
    }
    private void Start()
    {
        dControllerH = GetComponentInParent<DotControllerCrash>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if(collision.gameObject.tag == "ePoint")
        {
            if (_Vertical)
            {
                gameObject.transform.localPosition = new Vector2(0, dControllerH.sPoint.y);
                float f = float.Parse(gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.ToString().TrimEnd('X'));
                gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = f + 0.1f + "X";
            }
            else
            {
                gameObject.transform.localPosition = new Vector2(dControllerH.sPoint.x, 0);
                float f = float.Parse(gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text.ToString().TrimEnd('X'));
                gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = f + 2f + "X";
            }
        }
    }
}
*/