using System.Collections.Generic;
using UnityEngine;

public enum TileType { None, Border, Margin, Corner };

public class Tile : MonoBehaviour {

    public TileType tileType;
    private GameManager gameManager;

    private List<Note> notes = new List<Note>();
    private int row;
    private int column;
    private delegate void AttenuateIfNeeded();
    private AttenuateIfNeeded AttenuateNotesIfNeeded;


    private void Start()
    {
        gameManager = GameObject.Find("GM").GetComponent<GameManager>();
    }


    private void Update()
    {
        //if(AttenuateNotesIfNeeded != null) AttenuateNotesIfNeeded();
    }


    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameManager.ShowOptionMenu(this);
        }
    }


    /**
     * Invert the position if needed and Play the notes on the tile
     */
    public void OnVisitorCollide(GameObject collision)
    {
        if (collision.tag.Equals("Visitor"))
        {
            Visitor visitor = collision.GetComponent<Visitor>();
            InvertPositionIfNeeded(visitor);
            TriggerArrowCollision(collision);
            PlayNotes();
        }
    }


    /**
     * Trigger the collision on the arrow, if present
     */
    private void TriggerArrowCollision(GameObject collision)
    {
        Arrow arrow = GetComponentInChildren<Arrow>();
        if (arrow != null) arrow.OnVisitorCollision(collision);
    }


    /**
     * If needed, invert the position of the visitor
     */
    private void InvertPositionIfNeeded(Visitor visitor)
    {

        Arrow arrow = GetComponentInChildren<Arrow>();
        if (arrow == null || (arrow != null && arrow.isArrowWrongDirection()))
        {
            if ((tileType == TileType.Corner) || (tileType == TileType.Border && (visitor.GetDirection() == Vector3.up || visitor.GetDirection() == Vector3.down))
                || (tileType == TileType.Margin && ((visitor.GetDirection() == Vector3.right && getColumn() == gameManager.tilesNumber-1) || (visitor.GetDirection() == Vector3.left && getColumn() == 0))))
            {
                visitor.InvertDirection(gameObject.GetInstanceID(), transform.position);
            }
        }
    }


    /**
     * Play all the notes
     */
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
        note.audioSource.pitch = GameManager.PITCH;
        note.audioSource.Play();
    }


    public void AddNote(Note note){
        note.audioSource = gameObject.AddComponent<AudioSource>();
        note.audioSource.playOnAwake = false;
        note.audioSource.clip = note.clip;
        notes.Add(note);
        AttenuateNotesIfNeeded += note.OnAttenuateIfNeeded;
    }

    public void DestroyAllNotes()
    {
        foreach(Note note in notes)
        {
            Destroy(note.sprite);
        }
        notes.Clear();
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

    public Arrow getArrow()
    {
        return GetComponentInChildren<Arrow>();
    }

    public List<Note> getNotes()
    {
        return notes;
    }

    public int getNotesCount(){
        return notes.Count;
    }
}
