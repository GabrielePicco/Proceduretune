using System.Collections.Generic;
using UnityEngine;

public enum TileType { None, Border, Margin, Corner };

public class Tile : MonoBehaviour {

    public TileType tileType;
    private GameManager gameManager;
    private int row;
    private int column;
    private List<Note> notes;
    private delegate void AttenuateIfNeeded();
    private AttenuateIfNeeded AttenuateNotesIfNeeded;

    private void Start()
    {
        gameManager = GameObject.Find("GM").GetComponent<GameManager>();
        notes = new List<Note>();
    }

    private void Update()
    {
        if(AttenuateNotesIfNeeded != null) AttenuateNotesIfNeeded();
    }


    /**
     * Invert the position if needed and Play the notes on the tile
     */
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag.Equals("Visitor") && Vector3.Distance(collision.transform.position, transform.position) < GameManager.COLLISION_DISTANCE)
        {
            Visitor visitor = collision.GetComponent<Visitor>();
            visitor.NotifyVisit(transform.position);
            InvertPositionIfNeeded(visitor);
            PlayNotes();
        }
    }

    private void InvertPositionIfNeeded(Visitor visitor)
    {

        Arrow arrow = GetComponentInChildren<Arrow>();
        if (arrow == null || (arrow != null && arrow.isArrowWrongDirection()))
        {
            if ((tileType == TileType.Corner) || (tileType == TileType.Border && (visitor.GetDirection() == Vector3.up || visitor.GetDirection() == Vector3.down))
                || (tileType == TileType.Margin && (visitor.GetDirection() == Vector3.right || visitor.GetDirection() == Vector3.left)))
            {
                visitor.InvertDirection(gameObject.GetInstanceID(), transform.position);
            }
        }
    }


    private void PlayNotes()
    {
        if (GameManager.gameState == GameManager.GameState.Paused) return;
        foreach(Note note in notes){
            PlayNote(note);
        }
    }

    private void PlayNote(Note note)
    {
        note.audioSource.Stop();
        note.audioSource.volume = 1;
        note.audioSource.Play();
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameManager.ShowOptionMenu(this);
        }
    }

    public void addNote(Note note){
        note.audioSource = gameObject.AddComponent<AudioSource>();
        note.audioSource.playOnAwake = false;
        note.audioSource.clip = note.clip;
        notes.Add(note);
        AttenuateNotesIfNeeded += note.OnAttenuateIfNeeded;
    }

    public void SetTileType(TileType type)
    {
        this.tileType = type;
    }

    public void SetCoordinate(int i, int j)
    {
        this.row = i;
        this.column = j;
    }

    public int getRow()
    {
        return row;
    }

    public int getColumn()
    {
        return column;
    }
}
