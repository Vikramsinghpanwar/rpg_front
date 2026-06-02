using UnityEngine;
using UnityEngine.UI;

namespace Game.ChickenRoad2
{
    public class Hen : MonoBehaviour
{
    public Animator animator;
    public Transform initial_HenPosition_transform;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    
    public void CookHen()
    {
        animator.ResetTrigger("idle");
        animator.ResetTrigger("run");
        animator.SetTrigger("cook");}

    public void Run()
    {
        animator.ResetTrigger("idle");
        animator.SetTrigger("run");
    }

    public void SetIdle()
    {
        animator.ResetTrigger("run");
        animator.SetTrigger("idle");
    }


    
    public void ResetHen()
        {
            transform.localPosition = initial_HenPosition_transform.localPosition;
            animator.ResetTrigger("run");
            animator.SetTrigger("idle");
        }

    // Update is called once per frame
        void Update()
    {

    }
}
}

