using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDeactiveCoroutine : MonoBehaviour {
    public float duration = 2;
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        StartCoroutine(AutoDeactive_CR());
    }
    IEnumerator AutoDeactive_CR()
    {
        yield return new WaitForSeconds(duration);
        if (animator != null)
            animator.SetTrigger("Deactive");
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}
