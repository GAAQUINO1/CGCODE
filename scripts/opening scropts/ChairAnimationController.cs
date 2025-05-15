using UnityEngine;
using System.Collections;

public class ChairAnimationController : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(StopAnimationAfterPlay());
    }

    IEnumerator StopAnimationAfterPlay()
    {
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        anim.enabled = false; // Stops looping by disabling Animator
    }
}
