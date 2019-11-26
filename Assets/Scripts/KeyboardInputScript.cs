using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardInputScript : MonoBehaviour {

    public List<KeyCode> upKeys;
    public List<KeyCode> downKeys;
    public List<KeyCode> lefKeys;
    public List<KeyCode> rightKeys;
    public List<KeyCode> pushKeys;

    int frames = 0;
    PlayerScript playerScript;

    // Use this for initialization
    void Start () {
        playerScript = GetComponent<PlayerScript>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
        frames++;
    
        foreach (KeyCode code in upKeys)
        {
            if (Input.GetKey(code))
            {
                playerScript.up();
                break;
            }
        }
        foreach (KeyCode code in lefKeys)
        {
            if (Input.GetKey(code))
            {
                playerScript.left();
                break;
            }
        }
        foreach (KeyCode code in downKeys)
        {
            if (Input.GetKey(code))
            {
                playerScript.down();
                break;
            }
        }
        foreach (KeyCode code in rightKeys)
        {
            if (Input.GetKey(code))
            {
                playerScript.right();
                break;
            }
        }
        playerScript.pushPressed = false;
        foreach (KeyCode code in pushKeys)
        {
            if (Input.GetKey(code))
            {
                playerScript.pushPressed = true;
                break;
            }
        }
    }
}
