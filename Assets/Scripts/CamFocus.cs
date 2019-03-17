using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFocus : MonoBehaviour
{
    public Transform target = null;

    public Vector3 offset = Vector3.zero;
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = target.position + offset;
    }
}
