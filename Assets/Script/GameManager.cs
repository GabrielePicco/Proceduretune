using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

/**
 * Manage all the main behaviour of the game
 */
public class GameManager : MonoBehaviour
{

    public enum GameState { Running, Paused };

    public static float SPEED = 3f;
    public static float PITCH = 1f;

    [Header("Game Settings")]
    public int tilesNumber = 7;
    [Range(0, 30)]
    public float spanPercentual = 15;

    [Header("Model Prefab")]
    public GameObject tile;
    public GameObject visitor;
    public GameObject arrow;
    public GameObject note;

    [Header("GUI")]
    public GameObject btnPrefab;
    public GameObject scroolDynamic;
    public GameObject scroolOctave;
    public GameObject scroolDynamicContent;
    public GameObject defaultPanel;
    public Canvas canvasModal;
    public Slider sliderVelocity;
    public Slider sliderPitch;

    [Header("PlayStopMenu")]
    public Color btnNormalColor = new Color32(77, 77, 77, 255);
    public Color btnSelectedColor = new Color32(190, 48, 48, 255);
    public GameObject btnPlay;
    public GameObject btnPause;
    public static GameState gameState = GameState.Running;

    [Header("ModalButton")]
    public GameObject btnAddArrow;
    public GameObject btnModifyArrow;
    public GameObject btnDeleteAllNotes;

    [Header("EditorButton")]
    public GameObject btnArrowRotate;
    public GameObject btnArrowDelete;
    public GameObject btnVisitorDelete;

    private float collisionInterval = 1;
    private float tileSize;
    private Tile selectedTile;
    private GameObject selectedVisitor;
    private GameObject newVisitor = null;
    private Vector2 cornerPosition;
    private Tile[,] tiles;

    public delegate void StartVisitorEvent();
    public static StartVisitorEvent OnStartVisitor;
    public delegate void StopVisitorEvent();
    public static StopVisitorEvent OnStopVisitor;
    public delegate void VisitorsCollideEvent();
    public static VisitorsCollideEvent OnVisitorsCollide;


    /**
     * Genereate the game grid, add a visitor and start the clock
     */
    void Start()
    {
        GenerateGrid(tilesNumber);
        this.selectedTile = GameObject.Find("Tile(Clone)").GetComponent<Tile>();
        HideAllEditor();
        AddVisitor();
        StartCoroutine(onClockEvent());
    }


    /**
     * Call the OnVisitorCollide function every collisionInterval seconds, instantiace new visitor at correct time if needed
     */
    IEnumerator onClockEvent()
    {
        while (true)
        {
            if (newVisitor != null)
            {
                newVisitor.SetActive(true);
                newVisitor = null;
            }
            if (OnVisitorsCollide != null) OnVisitorsCollide();
            yield return new WaitForSeconds(collisionInterval);
        }
    }

    public void ChangeVisitorSpeed()
    {
        SPEED = sliderVelocity.value;
        CalculateCollisionInterval(tileSize);
    }

    public void ChangeNotesPitch()
    {
        PITCH = sliderPitch.value;
    }


    /**
     * Calculate the time needed by a visitor to go from a tile to another
     */
    private void CalculateCollisionInterval(float spaceBetweenTile)
    {
        collisionInterval = spaceBetweenTile / GameManager.SPEED;
    }


    /**
     * Set the correct scale to alla the game component
     */
    private void setComponentScale(float scale)
    {
        Vector2 localScale = new Vector2(scale, scale);
        tile.transform.localScale = localScale;
        visitor.transform.localScale = localScale;
        arrow.transform.localScale = localScale;
        note.transform.localScale = localScale;
    }


    private void HideAllEditor()
    {
        HideArrowEditor();
        HideVisitorEditor();
    }

    #region Visitors

    public void AddVisitor()
    {
        HideOptionMenu();
        Vector3 pos = selectedTile.transform.position;
        newVisitor = Instantiate(visitor, pos, visitor.transform.rotation);
        newVisitor.SetActive(false);
        selectedVisitor = newVisitor;
        selectedVisitor.GetComponent<SpriteRenderer>().color = btnSelectedColor;
        this.selectedTile.GetComponent<SpriteRenderer>().color = btnNormalColor;
        ShowVisitorEditor();
    }

    public void DeleteVisitor()
    {
        HideAllEditor();
        Destroy(selectedVisitor);
        selectedVisitor = null;
    }

    public void editVisitor(Visitor visitor)
    {
        if (selectedVisitor != null) selectedVisitor.GetComponent<SpriteRenderer>().color = Color.white;
        if (selectedTile != null) selectedTile.GetComponent<SpriteRenderer>().color = btnNormalColor;
        selectedVisitor = visitor.gameObject;
        selectedVisitor.GetComponent<SpriteRenderer>().color = btnSelectedColor;
        ShowVisitorEditor();
    }

    public void StartVisitor()
    {
        if (gameState == GameState.Paused)
        {
            OnStartVisitor();
            gameState = GameState.Running;
            btnPlay.GetComponent<Image>().color = btnSelectedColor;
            btnPause.GetComponent<Image>().color = btnNormalColor;
        }
    }

    public void StopVisitor()
    {
        if (gameState == GameState.Running)
        {
            OnStopVisitor();
            gameState = GameState.Paused;
            btnPlay.GetComponent<Image>().color = btnNormalColor;
            btnPause.GetComponent<Image>().color = btnSelectedColor;
        }
    }

    private void ShowVisitorEditor()
    {
        HideAllEditor();
        btnVisitorDelete.SetActive(true);
    }

    private void HideVisitorEditor()
    {
        btnVisitorDelete.SetActive(false);
    }

    #endregion

    #region Arrow

    public void AddArrow()
    {
        HideOptionMenu();
        GameObject gm = Instantiate(arrow, selectedTile.transform.position, arrow.transform.rotation);
        gm.transform.parent = selectedTile.transform;
        ShowArrowEditor();
    }

    public void DeleteArrow()
    {
        Destroy(selectedTile.GetComponentInChildren<Arrow>().gameObject);
        HideArrowEditor();
    }

    public void ModifyArrow()
    {
        HideOptionMenu();
        ShowArrowEditor();
    }

    public void RotateArrow()
    {
        selectedTile.GetComponentInChildren<Arrow>().Rotate();
    }


    private void ShowArrowEditor()
    {
        HideAllEditor();
        btnArrowRotate.SetActive(true);
        btnArrowDelete.SetActive(true);
    }

    private void HideArrowEditor()
    {
        btnArrowRotate.SetActive(false);
        btnArrowDelete.SetActive(false);
    }

    #endregion

    #region Note

    public void AddNote(String notePath)
    {
        scroolDynamic.SetActive(false);
        canvasModal.enabled = false;
        Note newNote = new Note();
        newNote.clip = Resources.Load<AudioClip>(notePath.Substring(0, notePath.LastIndexOf('.')));
        int idx = selectedTile.getNotesCount();
        if (idx < 9)
        {
            Vector3 position = selectedTile.transform.position;
            position.x += (tileSize / 7) * 2 * (idx % 3 - 1);
            position.y -= (tileSize / 7) * 2 * (Mathf.FloorToInt(idx / 3) - 1);
            GameObject noteSprite = Instantiate(note, position, note.transform.rotation);
            newNote.sprite = noteSprite;
        }
        selectedTile.AddNote(newNote);
    }

    public void RemoveAllNotes()
    {
        HideOptionMenu();
        selectedTile.DestroyAllNotes();
    }

    #endregion

    #region Grid Management

    /**
     * Dynamically generate the grid
     */
    private void GenerateGrid(int tileNumber)
    {
        tiles = new Tile[tilesNumber, tilesNumber];
        float tileOriginalHeight = tile.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        float worldScreenHeight = (float)(Camera.main.orthographicSize * 2.0);
        float tileSpan = (worldScreenHeight / tileNumber) * spanPercentual / 100;
        float tileScreenHeight = (worldScreenHeight - tileSpan * (tileNumber + 1)) / tileNumber;
        float tileHeight = tileScreenHeight / tileOriginalHeight;
        setComponentScale(tileHeight);
        Vector2 position = new Vector2(0, worldScreenHeight / 2 - tileScreenHeight / 2 - tileSpan);
        for (int i = 0; i < tileNumber; i++)
        {
            position.x = -(worldScreenHeight / 2 - tileScreenHeight / 2 - tileSpan);
            for (int j = 0; j < tileNumber; j++)
            {
                GameObject t = Instantiate(tile, position, tile.transform.rotation);
                Tile tileI = t.GetComponent<Tile>();
                tiles[i, j] = tileI;
                if ((i == 0 && j == 0) || (i == tileNumber - 1 && j == tileNumber - 1) || (i == 0 && j == tileNumber - 1) || (j == 0 && i == tileNumber - 1)) tileI.SetTileType(TileType.Corner);
                else if (i == 0 || i == tileNumber - 1) tileI.SetTileType(TileType.Border);
                else if (j == 0 || j == tileNumber - 1) tileI.SetTileType(TileType.Margin);
                else tileI.SetTileType(TileType.None);
                tileI.SetCoordinate(i, j);
                position.x += tileScreenHeight + tileSpan;
            }
            position.y -= tileScreenHeight + tileSpan;
        }
        cornerPosition = new Vector2(-(worldScreenHeight / 2 - tileScreenHeight / 2 - tileSpan), worldScreenHeight / 2 - tileScreenHeight / 2 - tileSpan);
        tileSize = tileScreenHeight + tileSpan;
        CalculateCollisionInterval(tileScreenHeight + tileSpan);
    }


    /**
     * Return the Tile in the specified position
     */
    public Tile GetTile(Vector3 position)
    {
        position.x -= cornerPosition.x;
        position.y -= cornerPosition.y;
        int row = Mathf.RoundToInt(-position.y / tileSize);
        if (row >= tilesNumber) row = tilesNumber - 1;
        int column = Mathf.RoundToInt(position.x / tileSize);
        if (column >= tilesNumber) column = tilesNumber - 1;
        return tiles[row, column];
    }

    #endregion
   
    #region GUI Management

    public void ShowOptionMenu(Tile tile)
    {
        if (canvasModal.enabled == false)
        {
            canvasModal.enabled = true;
            if (tile.GetComponentInChildren<Arrow>() != null)
            {
                btnModifyArrow.SetActive(true);
                btnAddArrow.SetActive(false);
            }
            else
            {
                btnModifyArrow.SetActive(false);
                btnAddArrow.SetActive(true);
            }
            btnDeleteAllNotes.SetActive(tile.getNotesCount() > 0);
            this.selectedTile.GetComponent<SpriteRenderer>().color = btnNormalColor;
            if (selectedVisitor != null)
            {
                selectedVisitor.GetComponent<SpriteRenderer>().color = Color.white;
                selectedVisitor = null;
            }
            defaultPanel.SetActive(true);
            scroolDynamic.SetActive(false);
            scroolOctave.SetActive(false);
            this.selectedTile = tile;
            this.selectedTile.GetComponent<SpriteRenderer>().color = btnSelectedColor;
            HideAllEditor();

        }
    }

    public void HideOptionMenu()
    {
        canvasModal.enabled = false;
    }

    public void ShowDinamicScroll(String folderName)
    {
        DirectoryInfo dir = new DirectoryInfo(@folderName);
        FileInfo[] info = dir.GetFiles("*.mp3");
        foreach(Transform child in scroolDynamicContent.transform){
            Destroy(child.gameObject);
        }
        scroolDynamic.SetActive(true);
        int numFiles = 0;
        foreach (FileInfo f in info)
        {
            numFiles += 1;
            GameObject btn = Instantiate(btnPrefab);
            btn.transform.SetParent(scroolDynamicContent.transform);
            string name = f.Name.Substring(0, f.Name.LastIndexOf('.'));
            int index = name.LastIndexOf('.');
            if (index < 0) index = 0;
            else index += 1;
            name = name.Substring(index);
            btn.GetComponentInChildren<Text>().text = name;
            String resourcePath = f.FullName;
            String find = "Resources";
            resourcePath = resourcePath.Substring(resourcePath.IndexOf(find, find.Length) + find.Length + 1);
            btn.GetComponent<Button>().onClick.AddListener(() => AddNote(resourcePath));
        }
        RectTransform contentTransform = scroolDynamicContent.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, btnPrefab.GetComponent<RectTransform>().rect.height * numFiles + 5 * numFiles + 5);
    }

    #endregion

}
