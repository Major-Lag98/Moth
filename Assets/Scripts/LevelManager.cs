using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    bool hasWon = false;

    public GameObject pauseMenu;
    public GameObject winScreen;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWin();
        if (hasWon)
        {
            winScreen.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape) && !hasWon)
        {
            
            if (pauseMenu.activeInHierarchy) //escape both opens and closes pause
            {
                pauseMenu.SetActive(false);
            }
            else
            {
                pauseMenu.SetActive(true);
            }
            
        }
        
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
            hasWon = true;
        }
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Continue()
    {
        pauseMenu.SetActive(false);
    }
    public void NextLevel(string level)
    {
        SceneManager.LoadScene(level);
    }
}
