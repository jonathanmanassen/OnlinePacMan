using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    static public ScoreManager instance;

    public int playerNb = 0;
    private TextMeshPro text;

    public void Awake()
    {
        instance = this;
        text = GetComponent<TextMeshPro>();
    }

    public void SetScore(int score)
    {
        text.text = "Player: " + playerNb + "\nScore: " + score;
    }
}
