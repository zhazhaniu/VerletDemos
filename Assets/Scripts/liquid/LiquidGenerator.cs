using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidGenerator : MonoBehaviour
{
    public GameObject template = null;
    public float elapse = 1f;


    float accTime = 0f;
    void Update()
    {
        if (template)
        {
            accTime += Time.deltaTime;
            if (accTime > elapse)
            {
                GameObject tmp = Instantiate(template);
                tmp.SetActive(true);
                var randomPos = Random.insideUnitCircle;
                tmp.transform.position = transform.position + new Vector3(randomPos.x, 0, randomPos.y) * 3;
                tmp.AddComponent<LifeCtrl>().lifeTime = Random.Range(10, 120);
                accTime = 0;
            }
        }
        
    }
}
