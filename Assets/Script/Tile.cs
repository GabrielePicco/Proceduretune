using System;
using UnityEngine;

public enum TileType { None, Border, Margin, Corner };

public class Tile : MonoBehaviour {

    public TileType tileType;
    private GameManager gameManager;
    private int row;
    private int column;

    private void Start()
    {
        gameManager = GameObject.Find("GM").GetComponent<GameManager>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag.Equals("Visitor"))
        {
            Visitor visitor = collision.GetComponent<Visitor>();
            Arrow arrow = GetComponentInChildren<Arrow>();
            if (arrow == null || (arrow != null && arrow.isArrowWrongDirection()))
            {
                if ((tileType == TileType.Corner) || (tileType == TileType.Border && (visitor.GetDirection() == Vector3.up || visitor.GetDirection() == Vector3.down))
                    || (tileType == TileType.Margin && (visitor.GetDirection() == Vector3.right || visitor.GetDirection() == Vector3.left)))
                {
                    if (Vector3.Distance(collision.transform.position, transform.position) < GameManager.COLLISION_DISTANCE)
                    {
                        collision.GetComponent<Visitor>().InvertDirection(gameObject.GetInstanceID(), transform.position);
                    }
                }
            }
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameManager.ShowOptionMenu(this);
        }
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
