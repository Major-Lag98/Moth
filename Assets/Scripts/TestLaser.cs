using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class TestLaser : MonoBehaviour
{

    public int maxRecursions = 10;
    public float maxStepDistance = 200;

    float airIndex = 1.0f;
    public float glassIndex = 1.5f;

    public GameObject laserToSpawn;


    // Update is called once per frame
    private void OnDrawGizmos() //Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        foreach (GameObject laser in GameObject.FindGameObjectsWithTag("Laser")) //every update redraw laser
        {
            laser.SetActive(false);
        }
            

        DrawPredictedReflection(this.transform.position, this.transform.up, maxRecursions);
    }

    void DrawPredictedReflection(Vector2 origin, Vector2 direction, int recursionsRemaing)// int reflectionsRemaining, int splitsRemaining)
    {

        var gizmoHue = (recursionsRemaing / (this.maxRecursions + 1f));

        RaycastHit2D hit2D = Physics2D.Raycast(origin, direction, maxStepDistance);

        if (hit2D) //did we hit somthing?
        {

            DrawLaser(origin, hit2D.point); //draw a line to it

            if (hit2D.transform.gameObject.tag == "Lens")
            {
                //Debug.Log("Lens Hit...");
                Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex);
            }
        }
        else //nothing hit
        {
            DrawLaser(origin, origin + direction * maxStepDistance);
        }
    }

    void DrawLaser(Vector2 start, Vector2 end)
    {

        GameObject laser = ObjectPooler.SharedInstance.GetPooledObject("Laser");
        if (laser != null)
        {

            laser.transform.position = Vector2.zero;
            laser.transform.rotation = Quaternion.identity;
            LineRenderer lr = laser.GetComponent<LineRenderer>();
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            laser.SetActive(true);
        }
    }

    void Refract(Vector2 origin, Vector2 normal, Vector2 direction, Vector2 point, GameObject lastHit, int recursionsRemainging, float n1, float n2) //entering a lens and exiting
    {
        float angleOfIncidence = Vector2.Angle(normal, -direction);
        //Debug.Log("Angle of incidence = " + angleOfIncidence);
        float angleOfRefraction = Mathf.Asin((n1 * Mathf.Sin(angleOfIncidence * Mathf.Deg2Rad)) / n2) * Mathf.Rad2Deg; //snells law
        //Debug.Log("Angle of refraction = " + angleOfRefraction);

        Vector2 normalTestPoint = point + normal * 0.1f;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(normalTestPoint, point - normal * 0.1f); //visualize normal



        if ((point.x - origin.x) * (normalTestPoint.y - origin.y) - (point.y - origin.y) * (normalTestPoint.x - origin.x) < 0) //cross product, check which side the normal is relitive to the incoming laser
        {
            angleOfRefraction = -angleOfRefraction;
        }


        direction = RotatePointByDeg(point, -normal, angleOfRefraction); //get direction by rotating a test point about the origin (-normal because were refracting inside the lens so we rotate from that point being 0 degrees)

        RaycastHit2D oppositeSide = FindOpp(point +  direction, -direction, lastHit);
        Vector2 potentialExitPoint = oppositeSide.point; //prep for exit
        DrawLaser(point, potentialExitPoint); //draw our laser from last point to current point


        //CHECK FOR CRITICAL ANGLE//


        //START EXIT//

        normal = oppositeSide.normal;

        Gizmos.DrawLine(potentialExitPoint + normal * 0.1f, potentialExitPoint - normal * 0.1f); //visualize normal

        angleOfIncidence = Vector2.Angle(normal, direction);
        angleOfRefraction = Mathf.Asin((n2 * Mathf.Sin(angleOfIncidence * Mathf.Deg2Rad)) / n1) * Mathf.Rad2Deg; //snells law, swap n2 and n1 because were exiting

        origin = point;
        normalTestPoint = potentialExitPoint + normal * 0.1f;

        if ((potentialExitPoint.x - origin.x) * (normalTestPoint.y - origin.y) - (potentialExitPoint.y - origin.y) * (normalTestPoint.x - origin.x) > 0) //cross product, check which side the normal is relitive to the incoming laser
        {
            angleOfRefraction = -angleOfRefraction;
        }

        direction = RotatePointByDeg(potentialExitPoint, normal, angleOfRefraction);

        //Exit done we did it yay!

        if (recursionsRemainging > 0)
        {
            DrawPredictedReflection(potentialExitPoint + direction * 0.01f, direction, --recursionsRemainging);
        }










    }

   
    Vector3 RotatePointByDeg(Vector2 origin, Vector2 normal, float angle) //rotate a point about its pivot and get its direction from its origin
    {
        Vector3 pointToRotate = origin + normal;
        Vector3 pivot = origin;
        Vector3 angles = new Vector3(0, 0, angle);
        Vector3 dir = pointToRotate - pivot;
        dir = Quaternion.Euler(angles) * dir;
        return dir;
    }

    RaycastHit2D FindOpp(Vector2 startPos, Vector2 reverseDirection, GameObject lastHit)
    {
        RaycastHit2D exitHit = new RaycastHit2D();
        RaycastHit2D[] hits = Physics2D.RaycastAll(startPos, reverseDirection);
        for (int i = 0; i < hits.Length; i++)//find a match
        {
            if (hits[i].transform.gameObject == lastHit)
            {
                exitHit = hits[i];
                break;
            }
        }
        return exitHit;
    }
}
