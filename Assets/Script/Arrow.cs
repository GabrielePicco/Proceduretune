using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour {

    public enum Direction {Up = 0, Down = 2, Left = 3, Rigth = 1};

    private Direction dir = Direction.Rigth;
    private IEnumerator rotCoroutine;
    private GameManager gameManager;

    void Start () {
        gameManager = GameObject.Find("GM").GetComponent<GameManager>();
        transform.Rotate(new Vector3(0, 0, -90));
	}


    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            gameManager.ShowOptionMenu(transform.parent.gameObject.GetComponent<Tile>());
        }
    }


    public void OnVisitorCollision(GameObject collision)
    {
        if (collision.tag.Equals("Visitor") && !isArrowWrongDirection())
        {
            Vector3 direction = getRotation() * Vector3.up;
            collision.GetComponent<Visitor>().ChangeDirection(direction, transform.parent.gameObject.GetInstanceID(), transform.position);
        }
    }


    public void Rotate()
    {
        if(rotCoroutine != null)StopCoroutine(rotCoroutine);
        rotCoroutine = Rotate(getRotation(), -90, 0.3f);
        StartCoroutine(rotCoroutine);
        dir = (Arrow.Direction)(((int)dir + 1) % 4);
    }


    IEnumerator Rotate(Quaternion from, float angle, float duration = 0.3f)
    {
        Vector3 axis = new Vector3(0, 0, 1);
        Quaternion to = from;
        to *= Quaternion.Euler(axis * angle);

        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = to;
    }


    public bool isArrowWrongDirection()
    {
        Tile tile = GetComponentInParent<Tile>();
        bool wrong = false;
        switch (getDirection())
        {
            case Arrow.Direction.Up:
                if (tile.getRow() == 0) wrong = true;
                break;
            case Arrow.Direction.Left:
                if (tile.getColumn() == 0) wrong = true;
                break;
            case Arrow.Direction.Rigth:
                if (tile.getColumn() == gameManager.tilesNumber - 1) wrong = true;
                break;
            case Arrow.Direction.Down:
                if (tile.getRow() == gameManager.tilesNumber - 1) wrong = true;
                break;
        }
        return wrong;
    }


    private Quaternion getRotation()
    {
        switch (dir)
        {
            case Direction.Up:
                return Quaternion.Euler(0, 0, 0);
            case Direction.Down:
                return Quaternion.Euler(0, 0, -180);
            case Direction.Left:
                return Quaternion.Euler(0, 0, 90);
            default:
                return Quaternion.Euler(0, 0, -90);
        }
    }


    public Direction getDirection(){
        return dir;
    }

    public void setDirection(Direction direction){
        while(direction != dir){
            Rotate();
        }
    }


    public static Vector3 ResolveDirection(Direction dir)
    {
        Quaternion rotation;
        switch (dir)
        {
            case Direction.Up:
                rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction.Down:
                rotation = Quaternion.Euler(0, 0, -180);
                break;
            case Direction.Left:
                rotation = Quaternion.Euler(0, 0, 90);
                break;
            default:
                rotation = Quaternion.Euler(0, 0, -90);
                break;
        }
        return rotation * Vector3.up;
    }

    public static Direction ResolveDirection(Vector3 dir)
    {
        if (dir == Vector3.up){
            return Direction.Up;
        }else if (dir == Vector3.down)
        {
            return Direction.Down;
        }else if (dir == Vector3.left)
        {
            return Direction.Left;
        }else{
            return Direction.Rigth;
        }
    }
}
