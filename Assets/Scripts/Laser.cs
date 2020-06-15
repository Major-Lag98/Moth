using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Laser : MonoBehaviour
{

    public int maxRecursions = 10;
    public float maxStepDistance = 200;

    //public float lightIntensity = 1;

    float airIndex = 1.0f;
    public float glassIndex = 1.5f;

    public GameObject laserToSpawn;


    public Color white = Color.white;
    public Color red = Color.red;
    public Color redPurple = new Color(0.5019608f, 0, 0.2509804f, 1);
    public Color purple = new Color(0.5019608f, 0, 0.5019608f, 1);
    public Color bluePurple = new Color(0.5411765f, 0.1686275f, 0.8862745f, 1);
    public Color blue = Color.blue;
    public Color blueGreen = new Color(0.05098039f, 0.5960785f, 0.7294118f, 1);
    public Color green = Color.green;
    public Color yellowGreen = new Color(0.6039216f, 0.8039216f, 0.1960784f, 1);
    public Color yellow = Color.yellow;
    public Color yellowOrange = new Color(1, 0.8f, 0.25f, 1);
    public Color orange = new Color(1, 0.6470588f, 0, 1);
    public Color redOrange = new Color(1, 0.3254902f, 0.2862745f, 1);


    // Update is called once per frame
    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        foreach (GameObject laser in GameObject.FindGameObjectsWithTag("Laser")) //every update redraw laser
            laser.SetActive(false);
        
        foreach (GameObject light in GameObject.FindGameObjectsWithTag("Light")) //every update redraw pointlights
            light.SetActive(false);


        DrawPredictedReflection(this.transform.position, this.transform.up, maxRecursions, white);
    }

    void DrawPredictedReflection(Vector2 origin, Vector2 direction, int recursionsRemaining, Color color)// int reflectionsRemaining, int splitsRemaining)
    {

        var gizmoHue = (recursionsRemaining / (this.maxRecursions + 1f));

        RaycastHit2D hit2D = Physics2D.Raycast(origin, direction, maxStepDistance);

        if (hit2D && recursionsRemaining > 0) //did we hit somthing?
        {

            DrawLaser(origin, hit2D.point, color, color); //draw a line to it
            if (hit2D.transform.gameObject.tag == "Receiver")
            {
                //Debug.Log("Receiver hit");

                Receiver receiver = hit2D.transform.gameObject.GetComponent<Receiver>();
                if (receiver.isWhite && color == white   //try to make like this ---> if (receiver.color == color) { receiver.charging = true }  //MAYBE USE DICTIONARY??
                    || receiver.isRed && color == red
                    || receiver.isBlue && color == blue
                    || receiver.isYellow && color == yellow
                    || receiver.isGreen && color == green
                    || receiver.isOrange && color == orange
                    || receiver.isPurple && color == purple
                    || receiver.isRedPurple && color == redPurple
                    || receiver.isRedOrange && color == redOrange
                    || receiver.isYellowOrange && color == yellowOrange
                    || receiver.isYellowGreen && color == yellowGreen
                    || receiver.isBlueGreen && color == blueGreen
                    || receiver.isBluePurple && color == bluePurple)
                {
                    receiver.charging = true;
                }


            }
            else if (hit2D.transform.gameObject.tag == "Mirror") //mirror hit. set new pos where hit. reflect angle and make that new direction
            {
                    //Debug.Log("Mirror Hit");
                    direction = Vector2.Reflect(direction, hit2D.normal);
                    origin = hit2D.point + direction * 0.01f;


                    DrawPredictedReflection(origin, direction, --recursionsRemaining, color);
                

            }
            else if (hit2D.transform.gameObject.tag == "Splitter") //reflect and go ahead
            {

                
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, airIndex, glassIndex,color, color); //enter and refract
                    direction = Vector2.Reflect(direction, hit2D.normal);
                    origin = hit2D.point + direction * 0.01f;
                    DrawPredictedReflection(origin, direction, --recursionsRemaining, color); //reflect too
                
            }
            else if (hit2D.transform.gameObject.tag == "Lens")
            {
                Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, airIndex, glassIndex,color, color);
            }
            else if (hit2D.transform.gameObject.tag == "Prism")
            {

                if (color == white)
                {
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, airIndex, glassIndex - .1f,color, red, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, airIndex, glassIndex - .06f,color,  orange, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, airIndex, glassIndex - .03f,color, yellow, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, airIndex, glassIndex, color, green, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, airIndex, glassIndex + .03f,color, blue, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, airIndex, glassIndex + .06f,color, purple, 0.4f);
                }
            }
            else if (hit2D.transform.gameObject.tag == "Filter")
            {
                Filter filter = hit2D.transform.GetComponent<Filter>();

                if (color != blueGreen && color != bluePurple && color != redPurple && color != yellowGreen && color != yellowOrange) //tirtiary colors cant be changed
                {
                    if (!(color == purple && filter.isYellow) && !(color == orange && filter.isBlue) && !(color == green && filter.isRed)) //complemantary colors cant be changed
                    {

                        Color startColor = color;
                        if (filter.isBlue) //I feel like i could make this better...
                        {
                            if (color == white)
                            {
                                color = blue;
                            }
                            else if (color == blue)
                            {
                                color = blue;
                            }
                            else if (color == red)
                            {
                                color = purple;
                            }
                            else if (color == yellow)
                            {
                                color = green;
                            }
                            else if (color == green)
                            {
                                color = blueGreen;
                            }
                            else if (color == purple)
                            {
                                color = bluePurple;
                            }
                        }
                        if (filter.isRed)
                        {
                            if (color == white)
                            {
                                color = red;
                            }
                            else if (color == blue)
                            {
                                color = purple;
                            }
                            else if (color == red)
                            {
                                color = red;
                            }
                            else if (color == yellow)
                            {
                                color = orange;
                            }
                            else if (color == orange)
                            {
                                color = redOrange;
                            }
                            else if (color == purple)
                            {
                                color = redPurple;
                            }
                        }
                        if (filter.isYellow)
                        {
                            if (color == white)
                            {
                                color = yellow;
                            }
                            else if (color == blue)
                            {
                                color = green;
                            }
                            else if (color == red)
                            {
                                color = orange;
                            }
                            else if (color == yellow)
                            {
                                color = yellow;
                            }
                            else if (color == green)
                            {
                                color = yellowGreen;
                            }
                            else if (color == orange)
                            {
                                color = yellowOrange;
                            }
                        }
                        origin = hit2D.point;
                        RaycastHit2D oppositePosition = FindOpp(origin + direction, -direction, hit2D.transform.gameObject);
                        Vector2 oppPos = oppositePosition.point;
                        DrawLaser(origin, oppPos, startColor, color);
                        DrawPredictedReflection(oppPos + direction * 0.01f, direction, --recursionsRemaining, color);
                    }
                }
            }
        }
        else //nothing hit
        {
            DrawLaser(origin, origin + direction * maxStepDistance, color, color);
        }
    }

    void DrawLaser(Vector2 start, Vector2 end, Color startColor, Color endColor, float lightIntensity = 1.0f)
    {

        GameObject laser = ObjectPooler.SharedInstance.GetPooledObject("Laser");
        if (laser != null)
        {

            laser.transform.position = Vector2.zero;
            laser.transform.rotation = Quaternion.identity;
            LineRenderer lr = laser.GetComponent<LineRenderer>();
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.startColor = startColor;
            lr.endColor = endColor;
            laser.SetActive(true);
        }
        GameObject light = ObjectPooler.SharedInstance.GetPooledObject("Light");
        if (light != null)
        {
            light.transform.position = (Vector3)end;
            light.transform.rotation = Quaternion.identity;
            Light2D l = light.GetComponent<Light2D>();
            //l.range = range;
            l.intensity = lightIntensity;
            l.color = endColor;
            light.SetActive(true);
        }
    }

    void Refract(Vector2 origin, Vector2 normal, Vector2 direction, Vector2 point, GameObject lastHit, int recursionsRemainging, float n1, float n2, Color startColor, Color endColor, float lightIntensity = 1.0f) //entering a lens and exiting
    {
        float angleOfIncidence = Vector2.Angle(normal, -direction);
        //Debug.Log("Angle of incidence = " + angleOfIncidence);
        float angleOfRefraction = Mathf.Asin((n1 * Mathf.Sin(angleOfIncidence * Mathf.Deg2Rad)) / n2) * Mathf.Rad2Deg; //snells law
        //Debug.Log("Angle of refraction = " + angleOfRefraction);

        Vector2 normalTestPoint = point + normal * 0.1f;

        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(normalTestPoint, point - normal * 0.1f); //visualize normal



        if ((point.x - origin.x) * (normalTestPoint.y - origin.y) - (point.y - origin.y) * (normalTestPoint.x - origin.x) < 0) //cross product, check which side the normal is relitive to the incoming laser
        {
            angleOfRefraction = -angleOfRefraction;
        }


        direction = RotatePointByDeg(point, -normal, angleOfRefraction); //get direction by rotating a test point about the origin (-normal because were refracting inside the lens so we rotate from that point being 0 degrees)

        RaycastHit2D oppositeSide = FindOpp(point +  direction, -direction, lastHit);



        //prep for exit//

        Vector2 potentialExitPoint = oppositeSide.point; //prep for exit
        DrawLaser(point, potentialExitPoint, startColor, endColor, lightIntensity); //draw our laser from last point to current point
        normal = oppositeSide.normal;

         

        angleOfIncidence = Vector2.Angle(normal, direction);
        angleOfRefraction = Mathf.Asin((n2 * Mathf.Sin(angleOfIncidence * Mathf.Deg2Rad)) / n1) * Mathf.Rad2Deg; //snells law, swap n2 and n1 because were exiting
        origin = point;
        //Gizmos.DrawLine(potentialExitPoint + normal * 0.1f, potentialExitPoint - normal * 0.1f);//visualize normal
        //CHECK FOR CRITICAL ANGLE//
        if (angleOfIncidence > Mathf.Asin(n1 / n2) * Mathf.Rad2Deg) //critical angle formula
        {
            CriticalAngle(potentialExitPoint, direction, normal, lastHit, --recursionsRemainging, n1, n2,  endColor, lightIntensity);
        }
        else
        {
            //START EXIT//

            //Gizmos.DrawLine(potentialExitPoint + normal * 0.1f, potentialExitPoint - normal * 0.1f);//visualize normal

            normalTestPoint = potentialExitPoint + normal * 0.1f;

            if ((potentialExitPoint.x - origin.x) * (normalTestPoint.y - origin.y) - (potentialExitPoint.y - origin.y) * (normalTestPoint.x - origin.x) > 0) //cross product, check which side the normal is relitive to the incoming laser
            {
                angleOfRefraction = -angleOfRefraction;
            }

            direction = RotatePointByDeg(potentialExitPoint, normal, angleOfRefraction);

            //Exit done we did it yay!

            if (recursionsRemainging > 0)
            {
                DrawPredictedReflection(potentialExitPoint + direction * 0.01f, direction, --recursionsRemainging, endColor);
            }
        }


        
    }

    void CriticalAngle(Vector2 origin, Vector2 direction, Vector2 normal, GameObject lastHit, int recursionsRemainging, float n1, float n2, Color color, float lightIntensity)
    {
        //Debug.Log("Critical angle...");
        //Gizmos.DrawLine(origin + normal * 0.1f, origin - normal * 0.1f);//visualize normal

        direction = Vector2.Reflect(direction, normal);
        RaycastHit2D oppositeSide = FindOpp(origin + direction, -direction, lastHit);
        Vector2 potentialExitPoint = oppositeSide.point;
        normal = oppositeSide.normal;
        Vector2 normalTestPoint = potentialExitPoint + normal * 0.1f;
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(origin, oppositeSide.point); //reflection visualized
        DrawLaser(origin, potentialExitPoint, color, color, 1);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(potentialExitPoint + normal * 0.1f, potentialExitPoint - normal * 0.1f); //visualize normal

        float angleOfIncidence = Vector2.Angle(normal, direction);
        //Debug.Log("Angle of incidence = " + angleOfIncidence);
        

        if (angleOfIncidence > Mathf.Asin(n1 / n2) * Mathf.Rad2Deg) //critical angle formula
        {
            //Debug.Log("Double Critical angle...");
            CriticalAngle(potentialExitPoint, direction, normal, lastHit, --recursionsRemainging, n1, n2, color, lightIntensity - 0.1f); //decrease light intesity so it doesnt become unbarable with multiple
        }
        else
        {
            float angleOfRefraction = Mathf.Asin((n2 * Mathf.Sin(angleOfIncidence * Mathf.Deg2Rad)) / n1) * Mathf.Rad2Deg; //snells law
            //Debug.Log("Angle of refraction = " + angleOfRefraction);

            if ((potentialExitPoint.x - origin.x) * (normalTestPoint.y - origin.y) - (potentialExitPoint.y - origin.y) * (normalTestPoint.x - origin.x) > 0) //cross product, check which side the normal is relitive to the incoming laser
            {
                angleOfRefraction = -angleOfRefraction;
            }

            direction = RotatePointByDeg(potentialExitPoint, normal, angleOfRefraction);

            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(potentialExitPoint, potentialExitPoint + direction);

            if (recursionsRemainging > 0)
            {
                DrawPredictedReflection(potentialExitPoint + direction * 0.01f, direction, --recursionsRemainging, color);
            }

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