using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacFruit : MonoBehaviour
{
    public int score = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PacDot")
        {
            Destroy(other.gameObject);
        }
    }
}
