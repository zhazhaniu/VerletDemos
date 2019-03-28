using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidParticles : MonoBehaviour
{
    public bool isStatic = true;
    public float mass = 1f;
    public float radius = 0.5f;
    //Vt-1
    public Vector3 v_prev = Vector3.zero;
    //Vt
    public Vector3 v_now = Vector3.zero;
    //force t-1
    public Vector3 f_prev = Vector3.zero;
    //force t
    public Vector3 f_now = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        LiquidManager.instance.AddObject(this);
    }

    private void OnDestroy()
    {
        LiquidManager.instance.RemoveObject(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStatic)
        {
            UpdateState(Time.deltaTime);
            MovePosition(Time.deltaTime);
        }

    }

    public void UpdateState(float dt)
    {
        Vector3 f_next = LiquidManager.instance.CalculateForce(this);
        Vector3 v_next = v_now + (f_next + f_now) / (2.0f * mass) * dt;

        f_prev = f_now;
        v_prev = v_now;

        f_now = f_next;
        v_now = v_next;
    }

    //For Test, should move in other system
    public void MovePosition(float dt)
    {
        Vector3 curPos = transform.position;
        curPos += v_now * dt;
        transform.position = curPos;
    }
}
