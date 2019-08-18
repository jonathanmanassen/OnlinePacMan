using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    public enum State   //the different states the ai can be in
    {
        PURSUE_HIGHEST_SCORE,
        SEEK_HIGHEST_SCORE,
        PURSUE_SECOND_HIGHEST_SCORE,
        SEEK_CLOSEST,
        NONE
    }

    public enum AliveState
    {
        ALIVE,
        DEAD,
        WAITING,
    }
    
    private Vector3 velocity;
    private Vector3 target;
    private PacManController TransformTarget;
    private Vector3 initialPos;

    private AliveState aliveState;
    private float deadTimer;

    private SkinnedMeshRenderer mesh;
    private Color saveColor;

    //all the hyper parameters the user can change to change the behaviours

    public float maxMag = 7f;
    public State state;

    private Animator anim;
    private BoxCollider col;

    private void Awake()
    {
        //anim = GetComponentInChildren<Animator>();
        col = GetComponent<BoxCollider>();
        initialPos = transform.position;
        mesh = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();
        if (mesh)
            saveColor = mesh.material.color;
    }

    void Turn()
    {
        if (velocity == Vector3.zero)
            return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, Quaternion.LookRotation(velocity).eulerAngles.y, 90), Time.deltaTime * 300);
    }

    void KinematicFlee() //the flee behaviour
    {
        Vector3 dir = transform.position - target;

        velocity = maxMag * dir.normalized;
        transform.position += velocity * Time.deltaTime;
        Turn();
    }

    void KinematicArrive(bool turn = true, int speedModifier = 1)  //the arrive behaviour
    {
        Vector3 dir = target - transform.position;

        float mag = 0;
        mag = Mathf.Min(maxMag, dir.magnitude / 0.2f);  //gets the desired velocity magnitude
        velocity = mag * dir.normalized / speedModifier;   //gets the velocity
        transform.position += velocity * Time.deltaTime;   //changes the position
    }

    private void KinematicSeek() //the seek behaviour
    {
        Vector3 dir = target - transform.position;

        velocity = dir.normalized * maxMag;
        transform.position += velocity * Time.deltaTime;
        Turn();
    }

    private void KinematicPursue() //the pursue behaviour
    {
        Vector3 dir = TransformTarget.transform.position - transform.position;
        float distance = dir.magnitude;
        float speed = maxMag;
        float prediction = distance / speed;
        if (prediction > 1.5f)
            prediction = 1.5f;

        target = TransformTarget.transform.position;
        target += TransformTarget.velocity * prediction;
        target = Astar.CreateAndMakePath(transform.position, target)[0];
        KinematicSeek();
    }

    PacManController GetHighestScorePacMan()
    {
        PacManController[] pacmen = FindObjectsOfType<PacManController>();
        if (pacmen == null)
            return null;
        int max = -1;
        PacManController tmp = null;

        int i = 0;
        foreach (PacManController p in pacmen)
        {
            if (p.dead)
                continue;
            int score = p.GetScore();
            if (score > max)
            {
                max = score;
                tmp = p;
            }
        }
        return tmp;
    }

    PacManController GetSecondHighestScorePacMan()
    {
        PacManController[] pacmen = FindObjectsOfType<PacManController>();
        if (pacmen == null)
            return null;
        int max = -1;
        PacManController tmp = null;
        PacManController highestScore = GetHighestScorePacMan();

        foreach (PacManController p in pacmen)
        {
            if (p.dead)
                continue;
            int score = p.GetScore();
            if (score > max && p != highestScore)
            {
                max = score;
                tmp = p;
            }
        }
        return tmp;
    }

    void Update() //calls the behaviours according to the state
    {
        if (Time.timeScale == 0)
            return;

        if (aliveState == AliveState.DEAD)
        {
            if (Vector3.Distance(initialPos, transform.position) < 0.5f)
            {
                if (mesh)
                    mesh.material.color = saveColor;
                aliveState = AliveState.WAITING;
                deadTimer = 5;
                //anim.SetBool("Dead", false);
            }
            else
            {
                target = Astar.CreateAndMakePath(transform.position, initialPos)[0];
                KinematicArrive();
            }
        }
        else if (aliveState == AliveState.WAITING)
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPos, Time.deltaTime);
            deadTimer -= Time.deltaTime;
            if (deadTimer < 0)
            {
                aliveState = AliveState.ALIVE;
                col.enabled = true;
            }
        }
        else if (state == State.PURSUE_HIGHEST_SCORE)
        {
            PacManController tmp = GetHighestScorePacMan();
            if (tmp == null)
                return;
            TransformTarget = tmp;
            KinematicPursue();
        }
        else if (state == State.SEEK_HIGHEST_SCORE)
        {
            PacManController tmp = GetHighestScorePacMan();
            if (tmp == null)
                return;
            TransformTarget = tmp;
            target = Astar.CreateAndMakePath(transform.position, TransformTarget.transform.position)[0];
            KinematicSeek();
        }
        else if (state == State.PURSUE_SECOND_HIGHEST_SCORE)
        {
            PacManController tmp = GetSecondHighestScorePacMan();
            if (tmp == null)
                return;
            TransformTarget = tmp;
            KinematicPursue();
        }
        else if (state == State.SEEK_CLOSEST)
        {
            SeekClosest();
        }
    }
    
    void SeekClosest()
    {
        PacManController[] pacmen = FindObjectsOfType<PacManController>();
        if (pacmen == null)
            return;
        float min = Mathf.Infinity;
        PacManController tmp = null;

        foreach (PacManController p in pacmen)
        {
            if (p.dead)
                continue;
            float dist = Vector3.Distance(p.transform.position, transform.position);
            if (dist < min)
            {
                min = dist;
                tmp = p;
            }
        }
        if (tmp == null)
            return;
        TransformTarget = tmp;
        target = Astar.CreateAndMakePath(transform.position, TransformTarget.transform.position)[0];
        KinematicSeek();
    }

    public void Die()
    {
        if (mesh)
            mesh.material.color = Color.grey;
        aliveState = AliveState.DEAD;
        col.enabled = false;
//        anim.SetBool("Dead", true);
    }
}
