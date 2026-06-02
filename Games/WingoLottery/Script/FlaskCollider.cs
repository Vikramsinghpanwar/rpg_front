using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlaskCollider : MonoBehaviour
{
    public PhysicsMaterial2D pMaterial2D;
    private void Start()
    {
        PolygonCollider2D p = gameObject.GetComponent<PolygonCollider2D>();
        if(p != null)
        {
            Vector2[] points = p.points;
            EdgeCollider2D eCollidor = gameObject.AddComponent<EdgeCollider2D>();
            eCollidor.points = points;
            Destroy(p);
            eCollidor.edgeRadius = 1f;
            if(pMaterial2D != null)
            {
                eCollidor.sharedMaterial = pMaterial2D;
            }
        }
   
        
    }
}
