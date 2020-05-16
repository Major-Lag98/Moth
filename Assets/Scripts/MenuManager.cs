using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    string state;
    public GameObject mainMenu;
    public GameObject levelSelect;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (state == "Menu")
        {
            mainMenu.SetActive(true);
            levelSelect.SetActive(false);
        }
        else if (state == "LevelSelect")
        {
            mainMenu.SetActive(false);
            levelSelect.SetActive(true);
        }
    }

    public void Exit()
    {
        Debug.Log("Quitting...");
        Application.Quit();
    }

    public void ToMenu()
    {
        state = "Menu";
    }

    public void ToLevelSelect()
    {
        state = "LevelSelect";
    }

    public void Select(string level)
    {
        Debug.Log("Load " + level);
        SceneManager.LoadScene(level);
    }
}
