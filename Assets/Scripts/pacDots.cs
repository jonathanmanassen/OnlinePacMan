using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pacDots : MonoBehaviour
{
    public bool power = false;

    private void OnTriggerEnter(Collider other)
    {
        PacManController pacMan;

        if ((pacMan = other.GetComponent<PacManController>()) != null)
        {
            if (power)
            {
                addPacDotsAndFruit.instance.pacDotsDecrease();
                pacMan.AddScore(5);
            }
            else
                pacMan.AddScore();
            Destroy(gameObject);
        }
    }
}
