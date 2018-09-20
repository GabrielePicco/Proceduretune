﻿using System;
using UnityEngine;

public class Visitor : MonoBehaviour {

    private Vector3 direction;
    private GameManager gameManager;
    private int lastActorID = -1;
    private bool move = true;
    private bool waitingStop = false;
    private bool waitingPlay = false;

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
        waitingPlay = true;
    }

    public void OnCollide()
    {
        if (gameManager == null) return;
        Tile tile = gameManager.GetTile(transform.position);
        tile.OnVisitorCollide(gameObject);
        transform.position = tile.transform.position;
        CheckIfStopPlayNeeded();
    }

    private void CheckIfStopPlayNeeded(){
        if (waitingStop)
        {
            move = false;
            waitingStop = false;
        }
        if (waitingPlay)
        {
            move = true;
            waitingPlay = false;
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameManager.editVisitor(this);
        }
    }

    private void OnEnable()
    {
        GameManager.OnStartVisitor += OnPlay;
        GameManager.OnStopVisitor += OnStop;
        GameManager.OnVisitorsCollide += OnCollide;
    }

    private void OnDisable()
    {
        GameManager.OnStartVisitor -= OnPlay;
        GameManager.OnStopVisitor -= OnStop;
        GameManager.OnVisitorsCollide -= OnCollide;
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
