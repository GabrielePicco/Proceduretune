using System;
using UnityEngine;

public class Visitor : MonoBehaviour {

    private Vector3 direction;
    private GameManager gameManager;
    private int lastActorID = -1;
    private bool move = true;
    private bool waitingStop = false;

    void Start () 
    {
        direction = Vector3.right;
        gameManager = GameObject.Find("GM").GetComponent<GameManager>();
    }
	
	void Update ()
    {
        if(move) transform.position += direction * GameManager.SPEED * Time.deltaTime;
    }

    public void OnStop()
    {
        waitingStop = true;
    }

    public void OnPlay()
    {
        move = true;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameManager.editVisitor(this);
        }
    }

    /** 
     * Invert the direction, only if is not done by the same actor more than one time
     */
    public void InvertDirection(int identifier, Vector3 precisePosition)
    {
        if(lastActorID != identifier){
            transform.position = precisePosition;
            this.direction *= -1;
            lastActorID = identifier;
        }
    }

    public void NotifyVisit(Vector3 position)
    {
        if(waitingStop)
        {
            transform.position = position;
            move = false;
            waitingStop = false;
        }
    }

    public void ChangeDirection(Vector3 direction, int identifier, Vector3 precisePosition)
    {
        if (lastActorID != identifier)
        {
            transform.position = precisePosition;
            this.direction = direction;
            lastActorID = identifier;
        }
    }

    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }

    public Vector3 GetDirection()
    {
        return this.direction;
    }
}
