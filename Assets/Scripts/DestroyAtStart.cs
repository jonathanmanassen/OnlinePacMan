using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAtStart : MonoBehaviour
{
    public float time = 0f;

    void Start()
    {
        Destroy(gameObject, time);
    }
}
