using UnityEngine;

public class EggHatchEffect : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.Play("EggHatchEffect");
        Destroy(gameObject, 1f);
    }
}
