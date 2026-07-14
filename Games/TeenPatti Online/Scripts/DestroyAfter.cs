using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    [SerializeField] private float destroyTime = 2f;
    void OnEnable()
    {
        Invoke("Destroy", destroyTime);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
