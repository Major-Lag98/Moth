using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWin();
    }
    public void CheckForWin()
    {
        bool winCondMet = true;
        Receiver[] receivers = FindObjectsOfType<Receiver>() as Receiver[];
        foreach(Receiver receiver in receivers)
        {
            if (receiver.charged != true) //if there exists a receiver that isnt charged, then we havent won
            {
                winCondMet = false;
                break;
            }
        }
        if (winCondMet)
        {
            Debug.Log("Win condition met");
        }
    }
}
