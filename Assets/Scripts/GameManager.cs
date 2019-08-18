using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;

    static public bool start = false;

    public float time = 1;
    public TextMeshPro text;

    public System.Random rand;
    public System.Random randPos;

    PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        Time.timeScale = 0;
        instance = this;
        time *= 60;
    }

    void Update()
    {
        if (!start && PhotonNetwork.player.IsMasterClient && Input.GetKeyDown(KeyCode.Space) && FindObjectsOfType<PacManController>().Length > 1)
        {
            view.RPC("StartGame", PhotonTargets.All, (int)System.DateTime.Now.Ticks);
        }
        if (start && PhotonNetwork.player.IsMasterClient)
        {
            time -= Time.deltaTime;
        }
        if (time < 0 || (start == true && FindObjectsOfType<PacManController>().Length == 1))
        {
            view.RPC("EndGame", PhotonTargets.All);
        }
        if (Input.GetKeyDown(KeyCode.Escape) && Input.GetKey(KeyCode.LeftShift))
        {
            RestartGame();
        }
    }

    [PunRPC]
    void StartGame(int seed)
    {
        rand = new System.Random(seed);
        randPos = new System.Random(seed / 2);
        start = true;
        Time.timeScale = 1;
    }

    [PunRPC]
    void EndGame()
    {
        Time.timeScale = 0;
        int max = -1;
        int id = 0;

        foreach (PacManController pacMan in FindObjectsOfType<PacManController>())
        {
            int score = pacMan.GetScore();
            if (score > max)
            {
                max = score;
                id = pacMan.GetComponent<PhotonView>().ownerId;
            }
        }
        text.text = "Player " + id + " wins!";
        text.gameObject.SetActive(true);
    }

    public void RestartGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            StartCoroutine(ReloaSceneCor());
        }
    }

    IEnumerator ReloaSceneCor()
    {
        //send RPC to other clients to load my scene
        view.RPC("LoadMyScene", PhotonTargets.Others, SceneManager.GetActiveScene().name);
        yield return null;
        PhotonNetwork.isMessageQueueRunning = false;
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name); //restart the game
    }

    [PunRPC]
    public void LoadMyScene(string sceneName)
    {
        Debug.Log("REcieved RPC " + sceneName);
        PhotonNetwork.LoadLevel(sceneName); //restart the game
    }
}