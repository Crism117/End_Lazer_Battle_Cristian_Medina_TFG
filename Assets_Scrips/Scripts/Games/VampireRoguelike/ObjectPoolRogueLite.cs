using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolRogueLite : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 20;

    Queue<GameObject> pool = new Queue<GameObject>();

    private void Start() { Prewarm(); }

    public void Prewarm()
    {
        if (prefab == null) return;
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject Get()
    {
        if (pool.Count > 0)
        {
            var o = pool.Dequeue();
            if (o != null) return o;
        }
        if (prefab != null)
        {
            var newObj = Instantiate(prefab, transform);
            newObj.SetActive(false);
            return newObj;
        }
        return null;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;
        obj.transform.SetParent(transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
