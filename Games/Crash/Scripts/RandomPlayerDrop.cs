using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class RandomPlayerDrop : MonoBehaviour
{
    public ControllerCrash controllerRef;
    public GameObject tile;
    public bool _isdropping;
    // Start is called before the first frame update
    void Start()
    {
        controllerRef = FindObjectOfType<ControllerCrash>();
    }

    public IEnumerator DroppingEnum()
    {
        _isdropping = true;
        yield return new WaitForSeconds(Random.Range(0.3f, 1f));
        do
        {
            int s = Random.Range(3, 10);
            for (int i = 0; i < s; i++)
            {
                GameObject g = Instantiate(tile, gameObject.transform);
                g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "player_" + Random.Range(1000, 10000);
                g.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = controllerRef.s.ToString("F2") + "x";
                g.transform.SetParent(gameObject.transform.parent.transform.parent);
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(1f);
        }
        while (_isdropping);
        
    }

   
  
    // Update is called once per frame
    void Update()
    {
        
    }
}
