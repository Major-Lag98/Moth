using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectController : MonoBehaviour
{
    float startPosX;
    float startPosY;

    bool isBeingDragged = false;
    bool isBeingRotated = false;

    GameObject dragging = null;

    // Update is called once per frame
    void Update()
    {
        bool isDown = false;
        bool isUp = false;
        bool isDragged = false;
        bool isRotated = false;

#if UNITY_STANDALONE_WIN
        isDown = Input.GetMouseButtonDown(0);
        isUp = Input.GetMouseButtonUp(0);
        isDragged = isBeingDragged;
        isRotated = isBeingRotated && !Input.GetMouseButtonUp(1);
#endif

#if UNITY_ANDROID
        isDown = Input.touchCount == 1;
        isUp = Input.touches.Any(touch => touch.phase == TouchPhase.Ended);
    isDragged = Input.touches.Any(touch => touch.phase == TouchPhase.Moved);
    //isRotated = 
#endif


        if (isDown)
        {
            GetClickedOn();
            if (!dragging)
                return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startPosX = mousePos.x - dragging.transform.position.x;
            startPosY = mousePos.y - dragging.transform.position.y;

            isBeingDragged = true;
        }
        //Input.touches.Any(touch => touch.phase == TouchPhase.Began)
        if (isUp)
        {
            //Debug.Log("left click up");
            isBeingDragged = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            //Debug.Log("right click up");
            isBeingRotated = false;
        }
        if (isDragged)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dragging.transform.position = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY, 0);
        }
        else if (isBeingRotated) //if youre already being dragged you shouldnt be able to rotate.
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dir = mousePos - dragging.transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            dragging.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    bool GetClickedOn()
    {

        // If left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // Your raycast handling
            if (hit)
            {
                if (hit.collider.GetComponent<Interactable>() != null)
                    dragging = hit.collider.gameObject;
            }
        }

        return false;
    }

}
