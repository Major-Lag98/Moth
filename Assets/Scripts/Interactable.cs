using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interactable : MonoBehaviour
{

    private void OnMouseOver()
    {

        //bool isDown = false;

        //#if UNITY_ANDROID
        //        isDown = Input.GetMouseButtonDown(0);
        //#endif

        //#if UNITY_STANDALONE_WIN
        //        isDown = Input.touches.Any(touch => touch.phase == TouchPhase.Began)
        //#endif

        //if (isDown)
        //{
        //    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    startPosX = mousePos.x - this.transform.localPosition.x;
        //    startPosY = mousePos.y - this.transform.localPosition.y;

        //    isBeingDragged = true;
        //}

        //if (Input.touches.Count() == 2)
        //{
        //    isBeingRotated = true;
        //}
    }
}