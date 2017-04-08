using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))] //http://opengameart.org/content/rpg-gui-selection-arrow
public class TimeFlowUIController : MonoBehaviour {
    public Sprite forward;

    Image image;

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Image>();
	}
	
	public void SetForward()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }

    public void SetBackward()
    {
        transform.localScale = new Vector3(-1, 1, 1);
    }
}
