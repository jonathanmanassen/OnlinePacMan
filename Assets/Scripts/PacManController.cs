using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacManController : MonoBehaviour
{
    public float speed = 3f;

    private Animator anim;
    private PhotonView m_PhotonView;
    private AudioSource sound;

    private int score = 0;
    private List<Vector3> path = null;

    private Vector3 initialPos;
    float invinsibleTime = 0;

    float pacDotTime;

    [HideInInspector]
    public Vector3 velocity = Vector3.zero;
    [HideInInspector]
    public bool dead;
    [HideInInspector]
    bool pacDot = false;

    private void Awake()
    {
        dead = false;
        anim = GetComponent<Animator>();
        m_PhotonView = GetComponent<PhotonView>();
        sound = GetComponent<AudioSource>();
    }

    private void Start()
    {
        initialPos = transform.position;
        if (m_PhotonView.isMine)
            ScoreManager.instance.playerNb = m_PhotonView.ownerId;
    }

    void Update()
    {
        invinsibleTime -= Time.deltaTime;
        pacDotTime -= Time.deltaTime;

        if (m_PhotonView.isMine == false)
        {
            transform.position += velocity * Time.deltaTime;
            return;
        }

        if (dead)
        {
            if (path == null || path.Count == 0)
            {
                m_PhotonView.RPC("Revive", PhotonTargets.All);
            }
            else if (Vector3.Distance(path[0], transform.position) < 0.1f)
            {
                path.RemoveAt(0);
            }
            else
            {
                Vector3 dir = path[0] - transform.position;

                m_PhotonView.RPC("SetVelocity", PhotonTargets.All, dir.normalized * speed);
                velocity = dir.normalized * speed * Time.deltaTime;
                transform.position += velocity;
            }
        }
        else
        {
            float moveHorizontal = Input.GetAxisRaw("Horizontal");
            float moveVertical = Input.GetAxisRaw("Vertical");

            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

            m_PhotonView.RPC("SetVelocity", PhotonTargets.All, movement * speed);
            velocity = movement * Time.deltaTime * speed;
            transform.position += velocity;
            if (pacDot && pacDotTime < 0)
            {
                m_PhotonView.RPC("PowerDown", PhotonTargets.All);
            }
            else if (pacDot && pacDotTime < 1)
            {
                m_PhotonView.RPC("PoweringDown", PhotonTargets.All);
            }
        }

        if (velocity.x != 0 || velocity.z != 0)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, Quaternion.LookRotation(velocity).eulerAngles.y, 90), Time.deltaTime * 300);
        }
        if (transform.position.x < -4.8f)
            transform.position += new Vector3(9.6f, 0, 0);
        else if (transform.position.x > 4.8f)
            transform.position -= new Vector3(9.6f, 0, 0);
        GridPoint tmp;
        if ((tmp = RegularGrid.instance.GetClosestGridPointLocation(transform.position)) != null)
            transform.position = tmp.pos;
    }

    public void AddScore(int score = 1)
    {
        this.score += score;
        if (m_PhotonView.isMine == false)
        {
            return;
        }
        ScoreManager.instance.SetScore(this.score);
        sound.Play();
    }

    public int GetScore()
    {
        return score;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (m_PhotonView.isMine == false)
            return;
        PacManController tmp;
        if ((tmp = collision.gameObject.GetComponent<PacManController>()) != null)
        {
            if (!pacDot && tmp.pacDot)
            {
                m_PhotonView.RPC("DestroySelf", PhotonTargets.All);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_PhotonView.isMine == false)
            return;
        if (other.tag == "Enemy")
        {
            if (pacDot)
            {
                m_PhotonView.RPC("KillAI", PhotonTargets.All, other.name);
            }
            else if (invinsibleTime < 0)
            {
                m_PhotonView.RPC("Die", PhotonTargets.All);
            }
        }
        else if (other.tag == "PowerPacDot")
        {
            m_PhotonView.RPC("EatStrategic", PhotonTargets.All, other.transform.position, 5);
            m_PhotonView.RPC("PowerUp", PhotonTargets.All);
        }
        else if (other.tag == "Fruit")
        {
            m_PhotonView.RPC("EatStrategic", PhotonTargets.All, other.transform.position, other.GetComponent<PacFruit>().score);
            m_PhotonView.RPC("RenewBoard", PhotonTargets.All);
        }
        else if (other.tag == "PacDot")
        {
            m_PhotonView.RPC("EatPacDot", PhotonTargets.All, other.transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (m_PhotonView.isMine == false)
            return;
        if (other.tag == "Enemy")
        {
            if (pacDot)
            {
                m_PhotonView.RPC("KillAI", PhotonTargets.All, other.name);
            }
            else if (invinsibleTime < 0)
            {
                m_PhotonView.RPC("Die", PhotonTargets.All);
            }
        }
    }

    GameObject FindObject(Transform parent, Vector3 position)
    {
        GameObject tmp = null;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).position == position)
                tmp = parent.GetChild(i).gameObject;
        }
        return tmp;
    }

    [PunRPC]
    private void SetVelocity(Vector3 velocity)
    {
        this.velocity = velocity;
    }

    [PunRPC]
    private void EatPacDot(Vector3 position)
    {
        GameObject tmp = FindObject(RegularGrid.instance.transform, position);

        if (tmp == null)
            return;
        Destroy(tmp);
        AddScore();
    }

    [PunRPC]
    private void EatStrategic(Vector3 position, int score)
    {
        GameObject tmp = FindObject(GameObject.Find("StrategicLocations").transform, position);

        if (tmp == null)
            return;

        Destroy(tmp.transform.GetChild(0).gameObject);
        AddScore(score);
    }

    [PunRPC]
    private void PowerUp()
    {
        if (pacDot == false)
        {
            speed += 1;
        }
        pacDot = true;
        pacDotTime = 10;
        transform.GetComponent<SphereCollider>().radius = 0.01f;
        transform.localScale = new Vector3(200, 200, 200);
        addPacDotsAndFruit.instance.pacDotsDecrease();
    }

    [PunRPC]
    private void RenewBoard()
    {
        RegularGrid.instance.LoadPacDots();
        addPacDotsAndFruit.instance.FruitDecrease();
    }


    [PunRPC]
    private void DestroySelf()
    {
        Destroy(transform.parent.gameObject);
    }

    [PunRPC]
    private void KillAI(string name)
    {
        AIMovement []ais = FindObjectsOfType<AIMovement>();

        foreach (AIMovement ai in ais)
        {
            if (ai.name == name)
            {
                ai.Die();
                return;
            }
        }
    }

    [PunRPC]
    private void Die()
    {
        dead = true;
        path = Astar.CreateAndMakePath(transform.position, initialPos);
        GetComponent<SphereCollider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    [PunRPC]
    private void Revive()
    {
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<SphereCollider>().enabled = true;
        dead = false;
        path = null;
        invinsibleTime = 3;
    }

    [PunRPC]
    private void PowerDown()
    {
        pacDot = false;
        speed -= 1;
        transform.localScale = new Vector3(100, 100, 100);
        transform.GetComponent<SphereCollider>().radius = 0.02f;
    }

    [PunRPC]
    private void PoweringDown()
    {
        transform.localScale -= new Vector3(100, 100, 100) * Time.deltaTime;
        transform.GetComponent<SphereCollider>().radius += 0.01f * Time.deltaTime;
    }
}
