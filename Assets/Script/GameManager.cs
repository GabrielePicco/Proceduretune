using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public enum GameState { Running, Paused };

    public static readonly float COLLISION_DISTANCE = 0.1f;
    public static float SPEED = 1;

    public GameObject tile;
    public GameObject visitor;
    public GameObject arrow;
    public Canvas canvasModal;

    [Header("UI")]
    public GameObject btnPrefab;
    public GameObject scroolDynamic;
    public GameObject scroolOctave;
    public GameObject scroolDynamicContent;
    public GameObject defaultPanel;

    [Header("PlayStop")]
    public Color btnNormalColor = new Color32(77, 77, 77, 255);
    public Color btnSelectedColor = new Color32(190, 48, 48, 255);
    public GameObject btnPlay;
    public GameObject btnPause;
    public static GameState gameState = GameState.Running;

    [Header("ModalButton")]
    public GameObject btnAddArrow;
    public GameObject btnModifyArrow;

    [Header("EditorButton")]
    public GameObject btnArrowRotate;
    public GameObject btnArrowDelete;
    public GameObject btnVisitorDelete;

    public int tilesNumber = 7;
    [Range(0, 30)]
    public float spanPercentual = 15;
    private Tile selectedTile;
    private GameObject selectedVisitor;

    private delegate void StopVisitorEvent();
    private StopVisitorEvent OnStopVisitor;
    private delegate void StartVisitorEvent();
    private StartVisitorEvent OnStartVisitor;
    private float collisionInterval = 1;

    void Start () 
    {
        GenerateGrid(tilesNumber);
        this.selectedTile = GameObject.Find("Tile(Clone)").GetComponent<Tile>();
        AddVisitor();
        selectedVisitor.GetComponent<SpriteRenderer>().color = Color.white;
        HideAllEditor();
        StartCoroutine(onClockEvent());
    }

    IEnumerator onClockEvent()
    {
        while (true)
        {
            Debug.Log("Collision");
            yield return new WaitForSeconds(collisionInterval);
        }
    }

    public void ShowOptionMenu(Tile tile){
        if(canvasModal.enabled == false){
            canvasModal.enabled = true;
            if(tile.GetComponentInChildren<Arrow>() != null){
                btnModifyArrow.SetActive(true);
                btnAddArrow.SetActive(false);
            }else{
                btnModifyArrow.SetActive(false);
                btnAddArrow.SetActive(true);
            }
            this.selectedTile.GetComponent<SpriteRenderer>().color = btnNormalColor;
            if(selectedVisitor != null){
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

    public void AddVisitor()
    {
        HideOptionMenu();
        Vector3 initPos = selectedTile.transform.position;
        initPos.x += 0.2f;
        selectedVisitor = Instantiate(visitor, initPos , visitor.transform.rotation);
        OnStopVisitor += selectedVisitor.GetComponent<Visitor>().OnStop;
        OnStartVisitor += selectedVisitor.GetComponent<Visitor>().OnPlay;
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
        if(selectedVisitor != null)selectedVisitor.GetComponent<SpriteRenderer>().color = Color.white;
        if (selectedTile != null) selectedTile.GetComponent<SpriteRenderer>().color = btnNormalColor;
        selectedVisitor = visitor.gameObject;
        selectedVisitor.GetComponent<SpriteRenderer>().color = btnSelectedColor;
        ShowVisitorEditor();
    }

    public void ShowDinamicScroll(String folderName)
    {
        DirectoryInfo dir = new DirectoryInfo(@folderName);
        FileInfo[] info = dir.GetFiles("*.mp3");
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
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, btnPrefab.GetComponent<RectTransform>().rect.height * numFiles + 5*numFiles + 5);
    }

    public void AddNote(String notePath)
    {
        scroolDynamic.SetActive(false);
        canvasModal.enabled = false;
        Note note = new Note();
        note.clip = Resources.Load<AudioClip>(notePath.Substring(0, notePath.LastIndexOf('.')));
        selectedTile.addNote(note);
    }

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

    private void ShowVisitorEditor()
    {
        HideAllEditor();
        btnVisitorDelete.SetActive(true);
    }

    private void HideVisitorEditor()
    {
        btnVisitorDelete.SetActive(false);
    }

    private void HideAllEditor()
    {
        HideArrowEditor();
        HideVisitorEditor();
    }

    private void GenerateGrid(int tileNumber)
    {
        float tileOriginalHeight = tile.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        float worldScreenHeight = (float)(Camera.main.orthographicSize * 2.0);
        float tileSpan = (worldScreenHeight / tileNumber) * spanPercentual / 100;
        float tileScreenHeight = (worldScreenHeight - tileSpan * (tileNumber + 1)) / tileNumber;
        float tileHeight = tileScreenHeight / tileOriginalHeight;
        setComponentScale(tileHeight);
        Vector2 position = new Vector2(0, worldScreenHeight/2 - tileScreenHeight / 2 - tileSpan);
        for (int i = 0; i < tileNumber; i++)
        {
            position.x = -(worldScreenHeight / 2 - tileScreenHeight / 2 - tileSpan);
            for (int j = 0; j < tileNumber; j++)
            {
                GameObject t = Instantiate(tile, position, tile.transform.rotation);
                Tile tileI = t.GetComponent<Tile>();
                if ((i == 0 && j == 0) || (i == tileNumber - 1 && j == tileNumber - 1) || (i == 0 && j == tileNumber - 1) || (j == 0 && i == tileNumber - 1)) tileI.SetTileType(TileType.Corner);
                else if(i == 0 || i == tileNumber - 1) tileI.SetTileType(TileType.Border);
                else if (j == 0 || j == tileNumber - 1) tileI.SetTileType(TileType.Margin);
                else tileI.SetTileType(TileType.None);
                tileI.SetCoordinate(i, j);
                position.x += tileScreenHeight + tileSpan;
            }
            position.y -= tileScreenHeight + tileSpan;
        }
        CalculateCollisionInterval(tileScreenHeight + tileSpan);
    }

    private void CalculateCollisionInterval(float spaceBetweenTile)
    {
        collisionInterval = spaceBetweenTile / GameManager.SPEED;
    }

    private void setComponentScale(float scale)
    {
        tile.transform.localScale = new Vector2(scale, scale);
        visitor.transform.localScale = new Vector2(scale, scale);
        arrow.transform.localScale = new Vector2(scale, scale);
    }


    public void StopVisitor()
    {
        if(gameState == GameState.Running){
            OnStopVisitor();
            gameState = GameState.Paused;
            btnPlay.GetComponent<Image>().color = btnNormalColor;
            btnPause.GetComponent<Image>().color = btnSelectedColor;
        }
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

}
