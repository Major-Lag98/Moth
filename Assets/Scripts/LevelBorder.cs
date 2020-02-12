using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * THIS SCRIPT IS FOR TESTING & CALCULATING SCREEN SPACE
 * CURRENTLY ISNT USED FOR ANYTHING
 * MOSTLIKELY NOT GOING TO BE USED FOR ANYTHING
 * 
 * */
public class LevelBorder : MonoBehaviour
{
    public bool isTop = false;
    public bool isBot = false;
    public bool isLeft = false;
    public bool isRight = false;

    Vector2 screenBounds;



    // Start is called before the first frame update
    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));

        if (isTop)
        {
            this.transform.position = new Vector3(0,screenBounds.y + 1,0);
        }
        if (isBot)
        {
            this.transform.position = new Vector3(0, -screenBounds.y -1, 0);
        }
        if (isLeft)
        {
            this.transform.position = new Vector3(-screenBounds.x -1, 0, 0);
        }
        if (isRight)
        {
            this.transform.position = new Vector3(screenBounds.x +1, 0, 0);
        }
    }
}
