using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
public class Receiver : MonoBehaviour
{

    float progress = 0;
    public bool isCharging = false;
    public bool charged = false;

    public MyColor.ColorState myColor = new MyColor.ColorState();

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
        if (isCharging && progress < 100)
        {
            progress += 33 * Time.deltaTime;
        }
        if (!isCharging && progress > 0)
        {
            progress -= 11 * Time.deltaTime;
        }
        progress = Mathf.Clamp(progress, 0, 100);
        this.transform.parent.GetComponentInChildren<Light2D>().intensity = progress / 100;
        //Debug.Log(progress);
        isCharging = false;

        if (progress >= 100)
        {
            charged = true;
            foreach (MothAI moth in transform.parent.gameObject.GetComponentsInChildren<MothAI>())
            {
                moth.attracted = true;
            }

            if (spawned == false) //make sure there is always (amount) of moths per receiver
            {
                spawned = true;
                for (int i = transform.parent.gameObject.GetComponentsInChildren<MothAI>().Length; i < amountOfMothsToSpawn; i = transform.parent.gameObject.GetComponentsInChildren<MothAI>().Length) //get all moths related to a reciever and check if we have enough or need to spawn more
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
            foreach (MothAI moth in transform.parent.gameObject.GetComponentsInChildren<MothAI>())
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
            moth.GetComponent<MothAI>().target = this.gameObject;
            moth.SetActive(true);
        }
    }
}
