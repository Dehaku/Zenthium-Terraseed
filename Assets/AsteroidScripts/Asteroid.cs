using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Asteroid : MonoBehaviour
{
    public Mass mass;
    public Spherize spherize;
    
    IObjectPool<Asteroid> _pool;

    public void SetPool(IObjectPool<Asteroid> pool)
    {
        _pool = pool;
    }

    public void Release()
    {
        _pool.Release(this);
    }

}
