[System.Serializable]
public class GameIstance
{
    public string name = "Unknow";
    private float speed;
    private float pitch;
    private int tilesNumber;
    private TileSavable[,] tiles;

    public GameIstance(GameManager gameManager)
    {
        this.name = System.DateTime.Now.ToString();
        this.speed = GameManager.SPEED;
        this.pitch = GameManager.PITCH;
        this.tilesNumber = gameManager.tilesNumber;
        tiles = new TileSavable[tilesNumber, tilesNumber];
        for (int i = 0; i < gameManager.tilesNumber; i++){
            for (int j = 0; j < gameManager.tilesNumber; j++){
                TileSavable tileSv = new TileSavable(); 
                Tile tile = gameManager.GetTile(i, j);
                foreach(Note note in tile.getNotes()){
                    tileSv.notes.Add(note.notePath);
                }
                Arrow arrow = tile.getArrow();
                if (arrow != null) tileSv.arrowDirection = (int)arrow.getDirection();
                tiles[i, j] = tileSv;
            }
        }
        foreach(Visitor v in gameManager.GetVisitors())
        {
            Tile tile = gameManager.GetTile(v.transform.position);
            tiles[tile.getRow(), tile.getColumn()].visitorDirection = (int)Arrow.ResolveDirection(v.GetDirection());
        }
    }


    public void Restore(GameManager gameManager)
    {
        gameManager.ChangeVisitorSpeed(speed);
        gameManager.ChangeNotesPitch(pitch);
        gameManager.DestroyGrid();
        gameManager.tilesNumber = tilesNumber;
        gameManager.GenerateGrid(tilesNumber);
        gameManager.DeleteAllVisitors();
        gameManager.StartVisitor();
        for (int i = 0; i < gameManager.tilesNumber; i++)
        {
            for (int j = 0; j < gameManager.tilesNumber; j++)
            {
                if(tiles[i, j].arrowDirection != -1){
                    int direction = tiles[i, j].arrowDirection;
                    if (direction == 1) direction = 3;
                    else if (direction == 3) direction = 1;
                    gameManager.AddArrow(i, j, (Arrow.Direction)direction);
                }
                if (tiles[i, j].visitorDirection != -1)
                {
                    gameManager.AddVisitor(i, j, Arrow.ResolveDirection((Arrow.Direction)tiles[i, j].visitorDirection));
                }
                foreach (string note in tiles[i, j].notes){
                    gameManager.AddNote(i, j, note);
                }
            }
        }
    }
}