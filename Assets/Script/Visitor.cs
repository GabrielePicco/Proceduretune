using UnityEngine;

public class Visitor : MonoBehaviour {

    private float speed = 3;
    private Vector3 direction;
    private GameManager gameManager;
    private int lastActorID = -1;

    void Start () 
    {
        direction = Vector3.right;
        gameManager = GameObject.Find("GM").GetComponent<GameManager>();
    }
	
	void Update ()
    {
        if(GameManager.gameState == GameManager.GameState.Running){
            transform.position += direction * speed * Time.deltaTime;
        }
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
