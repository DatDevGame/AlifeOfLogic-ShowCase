using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicTransform : MonoBehaviour {

    private Transform target = null;
    [HideInInspector]
    public bool MimicPositionX = true;
    [HideInInspector]
    public bool MimicPositionY = true;
    [HideInInspector]
    public Vector3 offset = Vector3.zero;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(CopyTransformCR());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private IEnumerator CopyTransformCR()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            CopyTransform();
        }
    }

    private void CopyTransform()
    {
        if (target == null)
            return;
        Vector3 position = target.position;
        position.x = MimicPositionX ? position.x : transform.position.x;
        position.y = MimicPositionY ? position.y : transform.position.y;
        transform.position = position + offset;
        transform.rotation = target.rotation;
    }

    public void SetTargetTransform(Transform target)
    {
        this.target = target;
    }
}
