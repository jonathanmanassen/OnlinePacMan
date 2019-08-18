using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSelector : MonoBehaviour
{
    // Start is called before the first frame update
    void Update()
    {
        if (Time.timeScale != 0)
        {
            int i = 0;
            int rand;
            do
            {
                if (i == 10)
                    Destroy(gameObject);
                rand = GameManager.instance.rand.Next(0, 5);
                i++;
            } while (transform.GetChild(rand) == null);
            transform.GetChild(rand).transform.parent = transform.parent;
            Destroy(gameObject);
        }
    }
}
