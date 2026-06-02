using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;
    void Start()
    {
        gameObject.GetComponent<RectTransform>().rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);

    }
}
