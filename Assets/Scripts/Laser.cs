using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Laser : MonoBehaviour
{

    public int maxRecursions = 10;
    public float maxStepDistance = 200;
    public float intensity = 5;
    public float range = 1;

    float airIndex = 1.0f;
    public float glassIndex = 1.5f;
    public GameObject laserToSpawn;

    //public GameObject pointLight;

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

    

    //public Color originColor;

    private void Start()
    {
        /*if (originColor == null)
        {
            originColor = white;
        }*/
    }


    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        foreach (GameObject laser in GameObject.FindGameObjectsWithTag("Laser")) //every update redraw laser
            //Destroy(laser);
            laser.SetActive(false);
        foreach (GameObject light in GameObject.FindGameObjectsWithTag("Light")) //every update redraw pointlights
            light.SetActive(false);


        DrawPredictedReflection(this.transform.position, this.transform.up, maxRecursions, white);
    }

    void DrawPredictedReflection(Vector2 position, Vector2 direction, int recursionsRemaing, Color color)// int reflectionsRemaining, int splitsRemaining)
    {
        
        var gizmoHue = (recursionsRemaing / (this.maxRecursions + 1f));
        //Gizmos.color = Color.HSVToRGB(gizmoHue, 1, 1);
        RaycastHit2D hit2D = Physics2D.Raycast(position, direction, maxStepDistance);

        if (hit2D) //did we hit somthing?
        {
            //Gizmos.DrawLine(position, hit2D.point); //draw a line to it
            //Gizmos.DrawWireSphere(hit2D.point, 0.25f);

            DrawLaser(position, hit2D.point, color, color);

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
            if (hit2D.transform.gameObject.tag == "Mirror") //mirror hit. set new pos where hit. reflect angle and make that new direction
            {
                if (recursionsRemaing > 0)
                {
                    //Debug.Log("Mirror Hit");
                    direction = Vector2.Reflect(direction, hit2D.normal);
                    position = hit2D.point + direction * 0.01f;


                    DrawPredictedReflection(position, direction, --recursionsRemaing, color);
                }

            }
            if (hit2D.transform.gameObject.tag == "Splitter") //reflect and go ahead
            {
                
                //Debug.Log("Splitter hit");
                if (recursionsRemaing > 0)//go ahead
                {
                    Refract(hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex, color); //enter and refract
                    direction = Vector2.Reflect(direction, hit2D.normal);
                    position = hit2D.point + direction * 0.01f;
                    DrawPredictedReflection(position, direction, --recursionsRemaing, color); //reflect too
                }
            }
            if (hit2D.transform.gameObject.tag == "Prism")
            {
                
                if (recursionsRemaing > 0 && color == white)
                {
                    Refract(hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex, red);
                    Refract(hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex + 0.1f, blue);
                    Refract(hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex + 0.05f, green); 
                    //Refract();
                }
                else Refract(hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex, color);
            }
            if (hit2D.transform.gameObject.tag == "Filter")
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
                        position = hit2D.point;
                        RaycastHit2D oppositePosition = FindOpp(position + direction, -direction, hit2D.transform.gameObject);
                        Vector2 oppPos = oppositePosition.point;
                        DrawLaser(position, oppPos, startColor, color);
                        DrawPredictedReflection(oppPos + direction * 0.01f, direction, --recursionsRemaing, color);
                    }
                }
            }
        }
        else //nothing hit
        {
            //Gizmos.DrawLine(position, position + direction * maxStepDistance);
            DrawLaser(position, position + direction * maxStepDistance, color, color);
        }
    }
    

    void Refract(Vector2 normal, Vector2 direction, Vector2 point, GameObject lastHit, int recursionsRemainging, float n1, float n2, Color color)
    {
        float angleOfIncidence = Vector2.Angle(normal, -direction);
        //Debug.Log("Angle of incidence = " + angleOfIncidence);
        float angleOfRefraction = Mathf.Asin((n1 * Mathf.Sin(angleOfIncidence * Mathf.Deg2Rad)) / n2) * Mathf.Rad2Deg; //snells law
        //Debug.Log("Angle of refraction = " + angleOfRefraction);
        Vector2 yVector = Vector3.Cross(Vector3.Cross(direction, normal), normal);//perpindicular to normal
        Vector2 refractionX = -normal * Mathf.Cos(angleOfRefraction * Mathf.Deg2Rad);
        Vector2 refractionY = -(yVector * Mathf.Sin(angleOfRefraction * Mathf.Deg2Rad));
        Vector2 refractioDirection = refractionX + refractionY; //direction of refraction

        //find exit position

        RaycastHit2D oppositePosition = FindOpp(point + refractioDirection, -refractioDirection, lastHit);
        Vector2 exitPosition = oppositePosition.point;
        Vector2 exitNormal = oppositePosition.normal;
        DrawLaser(point, exitPosition, color, color);

        //end entry refraction
        //begin exit refraction

        exitNormal = -exitNormal;
        float exit_angleOfIncidence = Vector2.Angle(exitNormal, -refractioDirection);
        //Debug.Log("Exit angle of incidence = " + exit_angleOfIncidence);
        if (exit_angleOfIncidence > Mathf.Asin(n1 / n2) * Mathf.Rad2Deg) //critical angle formula
        {
            CriticalAngle(exitPosition, refractioDirection, exitNormal, lastHit, recursionsRemainging, n1, n2, color);
        }
        else
        {

            float exit_angleOfRefraction = Mathf.Asin((n2 * Mathf.Sin(exit_angleOfIncidence * Mathf.Deg2Rad)) / n1) * Mathf.Rad2Deg; //snells law
            //Debug.Log("Exit angle of refraction = " + exit_angleOfRefraction);
            Vector2 exit_yVector = Vector3.Cross(Vector3.Cross(refractioDirection, exitNormal), exitNormal);
            Vector2 exit_refractionX = -exitNormal * Mathf.Cos(exit_angleOfRefraction * Mathf.Deg2Rad);
            Vector2 exit_refractionY = -(exit_yVector * Mathf.Sin(exit_angleOfRefraction * Mathf.Deg2Rad));
            Vector2 exit_refractionVector = exit_refractionX + exit_refractionY;

            //Gizmos.DrawLine(exitPosition, exitPosition + exit_refractionVector * 2);

            if (recursionsRemainging > 0) //we exited the polygon
            {
                DrawPredictedReflection(exitPosition + exit_refractionVector * 0.01f, exit_refractionVector, --recursionsRemainging, color);
            }
            //end exit refraction
        }
    }
    void DrawLaser(Vector2 start, Vector2 end, Color startColor, Color endColor)
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
            l.intensity = intensity;
            l.color = endColor;
            light.SetActive(true);
        }
    }

    void CriticalAngle(Vector2 position, Vector2 inDirection, Vector2 normal, GameObject lastHit, int recursionsRemainging, float n1, float n2, Color color)
    {
        Vector2 reflectDirection = Vector2.Reflect(inDirection, normal);
        RaycastHit2D oppositeSide = FindOpp(position + reflectDirection, -reflectDirection, lastHit);
        Vector2 exitPosition = oppositeSide.point;
        Vector2 exitNormal = -oppositeSide.normal;
        DrawLaser(position, exitPosition, color, color);

        float exit_angleOfIncidence = Vector2.Angle(exitNormal, -reflectDirection);
        if (exit_angleOfIncidence > Mathf.Asin(airIndex / glassIndex) * Mathf.Rad2Deg) //critical angle formula
        {
            CriticalAngle(exitPosition, reflectDirection, exitNormal, lastHit, recursionsRemainging, n1, n2, color);
        }
        else
        {
            float exit_angleOfRefraction = Mathf.Asin((glassIndex * Mathf.Sin(exit_angleOfIncidence * Mathf.Deg2Rad)) / airIndex) * Mathf.Rad2Deg;
            Vector2 exit_yVector = Vector3.Cross(Vector3.Cross(reflectDirection, exitNormal), exitNormal);
            Vector2 exit_refractionX = -exitNormal * Mathf.Cos(exit_angleOfRefraction * Mathf.Deg2Rad);
            Vector2 exit_refractionY = -(exit_yVector * Mathf.Sin(exit_angleOfRefraction * Mathf.Deg2Rad));
            Vector2 exit_refractionVector = exit_refractionX + exit_refractionY;

            if (recursionsRemainging > 0)
            {
                DrawPredictedReflection(exitPosition + exit_refractionVector * 0.01f, exit_refractionVector, --recursionsRemainging, color);
            }
        }
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



