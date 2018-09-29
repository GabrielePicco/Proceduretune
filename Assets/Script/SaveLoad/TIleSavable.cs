using System.Collections.Generic;

[System.Serializable]
public class TileSavable
{
    public List<string> notes;
    public int arrowDirection = -1;
    public int visitorDirection = -1;

    public TileSavable(){
        notes = new List<string>();
    }
}