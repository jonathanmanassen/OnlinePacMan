using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class addPacDotsAndFruit : MonoBehaviour
{
    static public addPacDotsAndFruit instance;

    float timeBeforePacDots = 0;
    float timeBeforeFruit = 0;

    int pacDotsNb = 0;
    int fruitNb = 0;

    private void Awake()
    {
        instance = this;
    }

    public void pacDotsDecrease()
    {
        pacDotsNb--;
    }

    public void FruitDecrease()
    {
        fruitNb--;
    }

    private void SpawnObjects(int nb, Object go)
    {
        int tmp = 0;

        while (nb < 2)
        {
            int random = GameManager.instance.randPos.Next(0, 6 - (fruitNb + pacDotsNb + tmp));
            for (int j = 0; j < transform.childCount; j++)
            {
                if (transform.GetChild(j).childCount > 0)
                    continue;
                else if (transform.GetChild(j).childCount == 0)
                {
                    if (random > 0)
                        random--;
                    else
                    {
                        Instantiate(go, transform.GetChild(j));
                        break;
                    }
                }
            }
            nb++;
            tmp++;
        }
    }

    void Update()
    {
        if (!GameManager.start)
            return;
        if (fruitNb < 2)
            timeBeforeFruit -= Time.deltaTime;
        if (pacDotsNb < 2)
            timeBeforePacDots -= Time.deltaTime;

        if (timeBeforeFruit < 0)
        {
            SpawnObjects(fruitNb, Resources.Load("FruitSelector"));
            fruitNb = 2;
            timeBeforeFruit = 15;
        }
        if (timeBeforePacDots < 0)
        {
            SpawnObjects(pacDotsNb, Resources.Load("PowerPacDot"));
            pacDotsNb = 2;
            timeBeforePacDots = 30;
        }
    }
}