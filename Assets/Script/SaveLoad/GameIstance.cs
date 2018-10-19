using System;
using System.Collections;
using UnityEngine;

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
                    tileSv.notes.Add(note.bundle + ";" + note.audioSource.clip.name);
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
                    gameManager.AddArrow(i, j, (Arrow.Direction)direction);
                }
                foreach (string note in tiles[i, j].notes){
                    String[] unpack = note.Split(';');
                    gameManager.StartCoroutine(AddNoteCrt(gameManager, unpack[0], unpack[1], i, j));

                }
                if (tiles[i, j].visitorDirection != -1)
                {
                    gameManager.AddVisitor(i, j, Arrow.ResolveDirection((Arrow.Direction)tiles[i, j].visitorDirection));
                }
            }
        }
    }

    IEnumerator AddNoteCrt(GameManager gameManager, string bundle, string nameNote, int i, int j)
    {
        yield return gameManager.StartCoroutine(AssetBundleManager.Instance.DownloadAssetBundle(bundle));
        AssetBundle octaveBundle = AssetBundleManager.Instance.getBundle();
        AudioClip noteClip = octaveBundle.LoadAsset<AudioClip>(nameNote);
        gameManager.AddNote(i, j, bundle, noteClip);
    }
}