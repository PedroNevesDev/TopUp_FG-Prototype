using System.Collections.Generic;
using UnityEngine;

public class Breakables : MonoBehaviour
{

    public List<DropData> listOfDrops = new List<DropData>();

    public float hp=40;
    private float health;
    public void OnEnable()
    {
        health = hp;
    }
    public void TakeDamage(float damage)
{
    health-=damage;

    if(health<=0)
    {
        listOfDrops.ForEach(d=>{
            int max = Random.Range(d.min,d.max);
            for(int i = 0;i<max;i++)
            {
                ObjectPool.Instance.GetObject(d.drop,transform.position,Quaternion.identity);
            }
        });
        try{
        ObjectPool.Instance.ReturnObject(gameObject);
        }catch
        {
            Destroy(gameObject);
        }
    }

}

}
