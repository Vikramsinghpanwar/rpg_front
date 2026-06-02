using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class New : MonoBehaviour
{
    public int par1;
    public string hello;
    // Start is called before the first frame update
    void Start()
    {
        hello = "HELLO";
        FunNew();
    }

 /*   public void Fun()
    {
        for(int i = 1; i< hello.Length; i++)
        {
            print(hello.Substring(0, i));
        }

        for (int i = hello.Length; i > 0; i--)
        {
            print(hello.Substring(0, i));

        }

    }
*/

    public void FunNew()
    {
        for(int i = 0; i< hello.Length; i++)
        {
            for (int k = 0; k < i; k++)
            {
                print(hello[k]);
            }

        }
    }

  
}
