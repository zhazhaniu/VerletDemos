using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityVerletObjectManager : MonoBehaviour
{
    public static VelocityVerletObjectManager instance = null;

    public float RunningRadius = 18f;

    private void Awake()
    {
        instance = this;
    }


    HashSet<VelocityVerletObject> objects = new HashSet<VelocityVerletObject>();
    public void AddObject(VelocityVerletObject obj)
    {
        objects.Add(obj);
    }

    public void RemoveObject(VelocityVerletObject obj)
    {
        objects.Remove(obj);
    }

    public static float G = (float)6.67; //6.67e-11;
    //you may add your own rules instead of "Universal Gravitation"
    public Vector3 CalculateForce(VelocityVerletObject obj)
    {
        Vector3 f = Vector3.zero;
        foreach(VelocityVerletObject item in objects)
        {
            if (item != obj)
            {
                Vector3 dir = item.transform.position - obj.transform.position;
                float r2 = Mathf.Max(0.1f, Vector3.SqrMagnitude(dir));

                //ensure ug will not inf
                if (r2 > 1)
                {
                    float ug = (float)(G * item.mass * obj.mass / r2);
                    //force dir: obj->item
                    f += dir.normalized * ug;
                }
                
                
            }
        }
        return f;
    }

    private void Update()
    {
        foreach (VelocityVerletObject item in objects)
        {
            float dis = Vector3.Distance(item.transform.position, transform.position);
            if (dis > RunningRadius)
            {
                item.transform.position = RunningRadius * (item.transform.position - transform.position).normalized;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, RunningRadius);
    }
}
