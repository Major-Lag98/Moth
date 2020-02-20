using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Receiver : MonoBehaviour
{
    float progress = 0;
    public bool charging = false;
    public bool charged = false;

    public bool isWhite = false;
    public bool isRed = false;
    public bool isBlue = false;
    public bool isYellow = false;
    public bool isGreen = false;
    public bool isOrange = false;
    public bool isPurple = false;
    public bool isRedPurple = false;
    public bool isRedOrange = false;
    public bool isYellowGreen = false;
    public bool isYellowOrange = false;
    public bool isBlueGreen = false;
    public bool isBluePurple = false;

    int rotationSpeed = 10;

    public int amountOfMothsToSpawn = 8;

    bool spawned = false;

    public GameObject moth;

    Vector2 screenBounds;

    // Start is called before the first frame update
    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
    }

    // Update is called once per frame
    void Update()
    {
        if (charging && progress < 100)
        {
            progress += 33 * Time.deltaTime;
        }
        if (!charging && progress > 0)
        {
            progress -= 11 * Time.deltaTime;
        }
        progress = Mathf.Clamp(progress, 0, 100);
        this.transform.parent.GetComponentInChildren<Light>().intensity = progress / 10;
        //Debug.Log(progress);
        charging = false;

        if (progress >= 100)
        {
            charged = true;
            foreach (Moth moth in transform.parent.gameObject.GetComponentsInChildren<Moth>())
            {
                moth.attracted = true;
            }

            if (spawned == false) //make sure there is always (amount) of moths per receiver
            {
                spawned = true;
                for (int i = transform.parent.gameObject.GetComponentsInChildren<Moth>().Length; i < amountOfMothsToSpawn; i = transform.parent.gameObject.GetComponentsInChildren<Moth>().Length) //get all moths related to a reciever and check if we have enough or need to spawn more
                {
                    switch (Random.Range(0, 4)) //what side of the screen should moth spawn occure?
                    {
                        case 0: //top
                            Vector2 topSpawn = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y + Random.Range(0.1f, 0.9f));
                            SpawnMoth(topSpawn);
                            break;
                        case 1: //bot
                            Vector2 botSpawn = new Vector2(Random.Range(-screenBounds.x, screenBounds.x), -screenBounds.y - Random.Range(0.1f, 0.9f));
                            SpawnMoth(botSpawn);
                            break;
                        case 2: //left  
                            Vector2 leftSpawn = new Vector2(-screenBounds.x - Random.Range(0.1f, 0.9f), Random.Range(-screenBounds.y, screenBounds.y));
                            SpawnMoth(leftSpawn);
                            break;
                        case 3: //right
                            Vector2 rightSpawn = new Vector2(screenBounds.x + Random.Range(0.1f, 0.9f), Random.Range(-screenBounds.y, screenBounds.y));
                            SpawnMoth(rightSpawn);
                            break;

                    }
                }
            }
        }
        else if(progress < 100 && progress >= 50)
        {
            charged = false;
        }
        else
        {
            spawned = false;
            foreach (Moth moth in transform.parent.gameObject.GetComponentsInChildren<Moth>())
            {
                moth.attracted = false;
            }
        }

    }
    private void FixedUpdate()
    {
        transform.Rotate(Vector3.forward * (rotationSpeed + (progress * 10) * .350f) * Time.deltaTime);
    }

    void SpawnMoth(Vector2 spawnLoc)
    {
        Vector3 lookDirection = (Vector3)spawnLoc - this.transform.position;
        //GameObject m = Instantiate(moth,spawnLoc, Quaternion.Euler(spawnLoc));
        GameObject moth = ObjectPooler.SharedInstance.GetPooledObject("Moth");
        if (moth != null)
        {
            moth.transform.position = spawnLoc;
            moth.transform.rotation = Quaternion.identity;
            moth.transform.SetParent(this.transform.parent);
            moth.GetComponent<Moth>().target = this.gameObject;
            moth.SetActive(true);
        }
    }
}
