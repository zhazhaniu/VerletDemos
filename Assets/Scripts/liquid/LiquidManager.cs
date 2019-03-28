using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidManager : MonoBehaviour
{
    public static LiquidManager instance = null;

    public float RunningRadius = 18f;

    private void Awake()
    {
        instance = this;
    }


    HashSet<LiquidParticles> objects = new HashSet<LiquidParticles>();
    public void AddObject(LiquidParticles obj)
    {
        objects.Add(obj);
    }

    public void RemoveObject(LiquidParticles obj)
    {
        objects.Remove(obj);
    }

    public static Vector3 g = new Vector3(0, -3f, 0);

    public float weakForce = 1 / 500f;
    public float strongForce = 10f;
    public float dragForce = 3f;

    //you may add your own rules instead of "Universal Gravitation"
    public Vector3 CalculateForce(LiquidParticles obj)
    {
        Vector3 f = g;
        foreach (LiquidParticles item in objects)
        {
            if (item != obj)
            {
                Vector3 dir = item.transform.position - obj.transform.position;
                float r = Mathf.Max(0.1f, Vector3.Distance(item.transform.position, obj.transform.position));

                if (r > item.radius + obj.radius)
                {
                    float ug = (float)(item.mass * obj.mass * weakForce);
                    //force dir: obj->item
                    f += dir.normalized * ug;
                }
                else if (r < (item.radius + obj.radius) / 3f)
                {
                    float ug = (float)(item.mass * obj.mass * strongForce);
                    //force dir: item->obj
                    f -= dir.normalized * ug;
                }
                else
                {
                    float ug = (float)(item.mass * obj.mass * dragForce);
                    //force dir: obj->item
                    f += dir.normalized * ug;
                }
            }
        }
        return f;
    }

    private void Update()
    {
        foreach (LiquidParticles item in objects)
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
        //Gizmos.DrawWireCube(new Vector3(0, minY, 0), new Vector3(30f, 0.1f, 30f));
        Gizmos.DrawWireSphere(this.transform.position, RunningRadius);
    }
}
