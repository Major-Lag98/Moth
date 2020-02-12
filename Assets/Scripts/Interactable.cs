using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    float startPosX;
    float startPosY;

    bool isBeingDragged = false;
    bool isBeingRotated = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            //Debug.Log("left click up");
            isBeingDragged = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            //Debug.Log("right click up");
            isBeingRotated = false;
        }
        if (isBeingDragged)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.localPosition = new Vector3(mousePos.x - startPosX, mousePos.y - startPosY, 0);
        }
        else if (isBeingRotated) //if youre already being dragged you shouldnt be able to rotate.
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 dir = mousePos - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnMouseOver()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startPosX = mousePos.x - this.transform.localPosition.x;
            startPosY = mousePos.y - this.transform.localPosition.y;

            isBeingDragged = true;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isBeingRotated = true;
        }
    }
}