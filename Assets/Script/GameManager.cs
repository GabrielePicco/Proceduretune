using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * Manage all the main behaviour of the game
 */
public class GameManager : MonoBehaviour
{

    public enum GameState { Running, Paused };

    public static float SPEED = 3f;
    public static float PITCH = 1f;
    public static int TILES_NUMBER = -1;

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
    public Canvas canvasNewGame;
    public Slider sliderVelocity;
    public Slider sliderPitch;
    public Slider sliderTilesNumbers;
    public Text txtTilesNumber;

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
    private List<GameObject> newVisitors = new List<GameObject>();
    private Vector2 cornerPosition;
    private Tile[,] tiles;
    private List<Visitor> visitors = new List<Visitor>();

    public delegate void StartVisitorEvent();
    public static StartVisitorEvent OnStartVisitor;
    public delegate void StopVisitorEvent();
    public static StopVisitorEvent OnStopVisitor;
    public delegate void VisitorsCollideEvent();
    public static VisitorsCollideEvent OnVisitorsCollide;

    private void Awake()
    {
        if (TILES_NUMBER != -1) tilesNumber = TILES_NUMBER;
    }

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
            if (newVisitors.Count > 0)
            {
                for (int i = newVisitors.Count - 1; i >= 0; i--){
                    newVisitors[i].GetComponent<Visitor>().ImmediateStartVisitor();
                    newVisitors.RemoveAt(i);
                }
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

    public void ChangeVisitorSpeed(float speed)
    {
        SPEED = speed;
        sliderVelocity.value = speed;
        CalculateCollisionInterval(tileSize);
    }

    public void ChangeNotesPitch(float pitch)
    {
        PITCH = pitch;
        sliderPitch.value = pitch;
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

    public void NewGame()
    {
        TILES_NUMBER = (int)sliderTilesNumbers.value;
        SceneManager.LoadScene("Main");
    }

    public void SaveGame()
    {
        SaveLoad.Save(this);
    }

    public void LoadGame()
    {
        List<GameIstance> instances = SaveLoad.GetGameIstances();
        canvasModal.enabled = true;
        defaultPanel.SetActive(false);
        ShowGameIstance(instances);
    }

    private void LoadGameInstance(GameIstance gameInstance)
    {
        HideOptionMenu();
        SaveLoad.LoadIstance(this, gameInstance);
    }

    private void ShowGameIstance(List<GameIstance> gameIstances)
    {
        foreach (Transform child in scroolDynamicContent.transform)
        {
            Destroy(child.gameObject);
        }
        scroolDynamic.SetActive(true);
        foreach (GameIstance gi in gameIstances)
        {
            GameObject btn = Instantiate(btnPrefab);
            btn.transform.SetParent(scroolDynamicContent.transform);
            btn.GetComponentInChildren<Text>().text = gi.name;
            btn.GetComponent<Button>().onClick.AddListener(() => LoadGameInstance(gi));
        }
        RectTransform contentTransform = scroolDynamicContent.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, btnPrefab.GetComponent<RectTransform>().rect.height * gameIstances.Count + 5 * gameIstances.Count + 5);
    }


    #region Visitors

    public void AddVisitor(int row, int column, Vector3 direction)
    {
        selectedTile = GetTile(row, column);
        AddVisitor(direction);
    }

    public void AddVisitor()
    {
        AddVisitor(Vector3.right);
    }

    public void AddVisitor(Vector3 direction)
    {
        HideOptionMenu();
        Vector3 pos = selectedTile.transform.position;
        if(selectedVisitor != null)selectedVisitor.GetComponent<SpriteRenderer>().color = Color.white;
        selectedVisitor = Instantiate(visitor, pos, visitor.transform.rotation);
        selectedVisitor.GetComponent<Visitor>().ChangeDirection(direction, 0, selectedTile.transform.position);
        StartCoroutine(Effect.AnimationScale(selectedVisitor, 0.3f));
        selectedVisitor.GetComponent<SpriteRenderer>().color = btnSelectedColor;
        this.selectedTile.GetComponent<SpriteRenderer>().color = btnNormalColor;
        ShowVisitorEditor();
        visitors.Add(selectedVisitor.GetComponent<Visitor>());
        newVisitors.Add(selectedVisitor);
        if (gameState == GameState.Paused)
        {
            selectedVisitor.GetComponent<Visitor>().OnStop();
        }
    }


    public void DeleteVisitor()
    {
        HideAllEditor();
        visitors.Remove(selectedVisitor.GetComponent<Visitor>());
        Destroy(selectedVisitor);
        selectedVisitor = null;
    }


    public void DeleteAllVisitors()
    {
        for (int i = visitors.Count - 1; i >= 0; i--)
        {
            Destroy(visitors[i].gameObject);
            visitors.RemoveAt(i);
        }
    }

    public List<Visitor> GetVisitors()
    {
        return visitors;
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
        AddArrow(Arrow.Direction.Rigth);
    }

    public void AddArrow(int row, int column, Arrow.Direction direction)
    {
        selectedTile = GetTile(row, column);
        AddArrow(direction);
    }

    public void AddArrow(Arrow.Direction direction)
    {
        HideOptionMenu();
        StartCoroutine(AddArrowCrt(direction));
        ShowArrowEditor();
    }

    IEnumerator AddArrowCrt(Arrow.Direction direction)
    {
        selectedTile.GetComponent<Animator>().Play("Animate");
        GameObject gm = Instantiate(arrow, selectedTile.transform.position, arrow.transform.rotation);
        gm.GetComponent<Arrow>().setDirection(direction);
        gm.SetActive(false);
        gm.transform.parent = selectedTile.transform;
        yield return new WaitForSeconds(0.5f);
        gm.SetActive(true);
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


    public void AddNote(int row, int column, String bundle, AudioClip noteClip)
    {
        selectedTile = GetTile(row, column);
        AddNote(bundle, noteClip);
    }

    public void AddNote(String bundle, AudioClip noteClip)
    {
        scroolDynamic.SetActive(false);
        canvasModal.enabled = false;
        Note newNote = new Note();
        newNote.bundle = bundle;
        newNote.clip = noteClip;
        newNote.sprite = AddNoteGraphic();
        selectedTile.AddNote(newNote);
    }

    private GameObject AddNoteGraphic()
    {
        GameObject noteSprite = null;
        int idx = selectedTile.getNotesCount();
        if (idx < 9)
        {
            Vector3 position = selectedTile.transform.position;
            position.x += (tileSize / 7) * 2 * (idx % 3 - 1);
            position.y -= (tileSize / 7) * 2 * (Mathf.FloorToInt(idx / 3) - 1);
            noteSprite = Instantiate(note, position, note.transform.rotation);
            StartCoroutine(Effect.AnimationSlideFromUp(noteSprite, 0.3f));
        }
        return noteSprite;
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
    public void GenerateGrid(int tileNumber)
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


    public void DestroyGrid()
    {
        for (int i = 0; i < tilesNumber; i++)
        {
            for (int j = 0; j < tilesNumber; j++)
            {
                Destroy(tiles[i, j].gameObject);
            }
        }
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
        if (row < 0) row = 0;
        int column = Mathf.RoundToInt(position.x / tileSize);
        if (column >= tilesNumber) column = tilesNumber - 1;
        if (column < 0) column = 0;
        return tiles[row, column];
    }


    public Tile GetTile(int row, int column)
    {
        return tiles[row, column];
    }

    #endregion

    #region GUI Management

    public void ShowOptionMenu(Tile tile)
    {
        if (canvasModal.enabled == false && canvasNewGame.enabled == false)
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


    public void ChangeTilesNumber()
    {
        txtTilesNumber.text = "Size: " + (int)sliderTilesNumbers.value + " x " + (int)sliderTilesNumbers.value;
    }

    public void ShowNewGameCanvas()
    {
        canvasNewGame.enabled = true;
    }


    public void ShowDinamicScroll(String bundleName)
    {
        if(AssetBundleManager.Instance.IsInCache(bundleName)){
            ShowDinamicScrool(AssetBundleManager.Instance.GetBundle(bundleName), bundleName);
        }else{
            StartCoroutine(DownloadAndShowDimamicScroolCrt(bundleName));
        }
    }

    IEnumerator DownloadAndShowDimamicScroolCrt(String bundleName)
    {
        yield return StartCoroutine(AssetBundleManager.Instance.DownloadAssetBundle(bundleName));
        ShowDinamicScrool(AssetBundleManager.Instance.GetBundle(), bundleName);
    }


    private void ShowDinamicScrool(AssetBundle bundle, String bundleName){
        String[] notes = bundle.GetAllAssetNames();
        //DirectoryInfo dir = new DirectoryInfo(@folderName);
        //FileInfo[] info = dir.GetFiles("*.mp3");

        foreach (Transform child in scroolDynamicContent.transform)
        {
            Destroy(child.gameObject);
        }
        scroolDynamic.SetActive(true);
        notes = ReorderNotes(notes);
        foreach (String f in notes)
        {
            String noteName = getFileShortName(f);
            GameObject btn = Instantiate(btnPrefab);
            btn.transform.SetParent(scroolDynamicContent.transform);
            btn.GetComponentInChildren<Text>().text = noteName;
            AudioClip noteClip = bundle.LoadAsset<AudioClip>(f);
            btn.GetComponent<Button>().onClick.AddListener(() => AddNote(bundleName, noteClip));
        }
        RectTransform contentTransform = scroolDynamicContent.GetComponent<RectTransform>();
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, btnPrefab.GetComponent<RectTransform>().rect.height * notes.Length + 10 * notes.Length + 5);
    }

    private String[] ReorderNotes(String[] notes)
    {
        int[] substitution = new int[] { 3, 4, 1, 2, 12, 0, 10, 11, 8, 9, 7, 5, 6};
        if (substitution.Length != notes.Length) return notes;
        String[] result = new String[substitution.Length];
        for (int i = 0; i < substitution.Length; i++){
            result[substitution[i]] = notes[i];
        }
        return result;
    }

    static string getFileShortName(string name)
    {
        name = name.Substring(0, name.LastIndexOf('.'));
        int index = name.LastIndexOf('.');
        if (index < 0) index = 0;
        else index += 1;
        return name.Substring(index);
    }

    #endregion

}
