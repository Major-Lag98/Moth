using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MothAI : MonoBehaviour //SUMMARY: Moths always go toward their target which is the gems but always try to avoid eachother
{
    public GameObject target;
    GameObject originalTarget;

    public float speed = 0.5f;
    public float rotateSpeed = 200f;

    public float personalSpace = 0.1f;
    public bool flee = false;
    public bool attracted = true;

    Rigidbody2D rb;

    Vector2 screenBounds;

    float destroyBuffer = 1f; //how many units passed the edge of screen until destroy

    // Start is called before the first frame update
    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        originalTarget = target;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (this.transform.position.x > screenBounds.x + destroyBuffer || this.transform.position.x < -screenBounds.x - destroyBuffer || this.transform.position.y > screenBounds.y + destroyBuffer || this.transform.position.y < -screenBounds.y - destroyBuffer)
        {
            //Destroy(this.gameObject); //moth has left the screen
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        foreach (GameObject moth in GameObject.FindGameObjectsWithTag("Moth"))
        {
            
            if (Vector2.Distance(this.transform.position, moth.transform.position) < personalSpace && this.gameObject != moth) //if a moth is invading my personal space, avoid it
            {
                target = moth;
                flee = true;
                break;
            }
            else
            {
                flee = false;
                target = originalTarget;
            }
        }
        
        

        Vector2 direction = target.transform.position - transform.position;
        direction.Normalize();
        float rotateAmount = Vector3.Cross(direction, transform.up).z * 2;

        if (attracted) //we are attracted to the lit receiver
        {
            if (flee) //fly away
            {
                rb.angularVelocity = rotateAmount * rotateSpeed;
            }
            else //towards
            {
                rb.angularVelocity = -rotateAmount * rotateSpeed;
            }
        }
        else //if we are not attracted to receiver still fly away from any moths invading personal space
        {
            rb.angularVelocity = 0;
            if (flee) //fly away
            {
                rb.angularVelocity = rotateAmount * rotateSpeed;
            }
        }

        rb.velocity = transform.up * speed;
    }
    private void OnDrawGizmos() //visualise personal space for debugging
    {
        if (flee)
            Gizmos.color = Color.red;
        else
            Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(this.transform.position, personalSpace / 2);
    }

}
