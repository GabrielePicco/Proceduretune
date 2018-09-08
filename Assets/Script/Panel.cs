using UnityEngine;

public class Panel : MonoBehaviour {

	void Start () {
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (Screen.width - Screen.height) / 2);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height);
    }
	
}
