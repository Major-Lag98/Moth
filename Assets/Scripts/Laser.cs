using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{

    public int maxRecursions = 10;
    public float maxStepDistance = 50;
    public float intensity = 5;
    public float range = 1;

    float airIndex = 1.0f;
    public float glassIndex = 1.5f;
    public GameObject laserToSpawn;
    string originColor = "WHT";

    public GameObject pointLight;

    /*
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        foreach (GameObject laser in GameObject.FindGameObjectsWithTag("Laser")) //every update redraw laser
            Destroy(laser);
        foreach (GameObject light in GameObject.FindGameObjectsWithTag("Light")) //every update redraw pointlights
            Destroy(light);


        DrawPredictedReflection(this.transform.position, this.transform.up, maxRecursions, originColor);
    }
    */

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        foreach (GameObject laser in GameObject.FindGameObjectsWithTag("Laser")) //every update redraw laser
            Destroy(laser);
        foreach (GameObject light in GameObject.FindGameObjectsWithTag("Light")) //every update redraw pointlights
            Destroy(light);


        DrawPredictedReflection(this.transform.position, this.transform.up, maxRecursions, originColor);
    }

    void DrawPredictedReflection(Vector2 position, Vector2 direction, int recursionsRemaing, string color)// int reflectionsRemaining, int splitsRemaining)
    {
        
        var gizmoHue = (recursionsRemaing / (this.maxRecursions + 1f));
        //Gizmos.color = Color.HSVToRGB(gizmoHue, 1, 1);
        RaycastHit2D hit2D = Physics2D.Raycast(position, direction, maxStepDistance);

        if (hit2D) //did we hit somthing?
        {
            //Gizmos.DrawLine(position, hit2D.point); //draw a line to it
            //Gizmos.DrawWireSphere(hit2D.point, 0.25f);

            DrawLaser(position, hit2D.point, color);

            if (hit2D.transform.gameObject.tag == "Receiver")
            {
                //Debug.Log("Receiver hit");

                Receiver receiver = hit2D.transform.gameObject.GetComponent<Receiver>();
                if (receiver.isWhite && color == "WHT")
                {
                    receiver.charging = true;
                }
                if (receiver.isRed && color == "RED")
                {
                    receiver.charging = true;
                }
                if (receiver.isBlue && color == "BLU")
                {
                    receiver.charging = true;
                }
                if (receiver.isGreen && color == "GRN")
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
                
                if (recursionsRemaing > 0)
                {
                    Refract(hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex, "RED");
                    Refract(hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex + 0.1f, "BLU");
                    Refract(hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaing, airIndex, glassIndex + 0.05f, "GRN"); 
                    //Refract();
                }
            }
        }
        else //nothing hit
        {
            //Gizmos.DrawLine(position, position + direction * maxStepDistance);
            DrawLaser(position, position + direction * maxStepDistance, color);
        }

        
    }

    void Refract(Vector2 normal, Vector2 direction, Vector2 point, GameObject lastHit, int recursionsRemainging, float n1, float n2, string color)
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
        Vector2 exitPosition = new Vector2();
        Vector2 exitNormal = new Vector2();
        Vector2 findOppBegin = point + refractioDirection * 1f;
        RaycastHit2D[] exitHit = Physics2D.RaycastAll(findOppBegin, -refractioDirection);
        for (int i = 0; i <= exitHit.Length; i++)
        {
            if (exitHit[i].transform.gameObject == lastHit)
            {
                exitPosition = exitHit[i].point;
                exitNormal = exitHit[i].normal;
                break;
            }
        }
        //Gizmos.DrawLine(point, exitPosition);
        DrawLaser(point, exitPosition, color);
        //end entry refraction

        //begin exit refraction
        exitNormal = -exitNormal;
        //Gizmos.DrawLine(exitPosition + (-exitNormal * 0.1f), exitPosition + exitNormal * 0.1f);//observe normal
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
    void DrawLaser(Vector2 start, Vector2 end, string color)
    {
        GameObject laser = Instantiate(laserToSpawn, Vector2.zero, Quaternion.identity);
        GameObject light = Instantiate(pointLight, (Vector3)end + new Vector3(0, 0, -0.1f), Quaternion.identity);

        laser.GetComponent<LineRenderer>().SetPosition(0, start);
        laser.GetComponent<LineRenderer>().SetPosition(1, end);
        if (color == "WHT")
        {
            
            SetColor(laser, light, Color.white, 3);
            
        }
        if (color == "RED")
        {
            
            SetColor(laser, light, Color.red, 2);
        }
        if (color == "BLU")
        {
            
            SetColor(laser, light, Color.blue, 1);
        }
        if (color == "GRN")
        {
            
            SetColor(laser, light, Color.green, 0);
        }
        light.GetComponent<Light>().range = range;
        light.GetComponent<Light>().intensity = intensity;

    }

    void SetColor(GameObject laser, GameObject light ,Color color, int layer)
    {
        light.GetComponent<Light>().color = color;
        laser.GetComponent<LineRenderer>().startColor = color;
        laser.GetComponent<LineRenderer>().endColor = color;
        laser.GetComponent<LineRenderer>().sortingOrder = layer;
    }

    void CriticalAngle(Vector2 position, Vector2 inDirection, Vector2 normal, GameObject lastHit, int recursionsRemainging, float n1, float n2, string color)
    {
        Vector2 exitPosition = new Vector2();
        Vector2 exitNormal = new Vector2();
        Vector2 reflectDirection = Vector2.Reflect(inDirection, normal);
        RaycastHit2D[] exitHit = Physics2D.RaycastAll(position + reflectDirection * 1f, -reflectDirection);
        for (int i =0; i < exitHit.Length; i++)//find a match
        {
            if (exitHit[i].transform.gameObject == lastHit)
            {
                exitPosition = exitHit[i].point;
                exitNormal = -exitHit[i].normal;
                break;
            }
        }
        //Gizmos.DrawLine(position, exitPosition);
        DrawLaser(position, exitPosition, color);

        float exit_angleOfIncidence = Vector2.Angle(exitNormal, -reflectDirection);
        if (exit_angleOfIncidence > Mathf.Asin(airIndex / glassIndex) * Mathf.Rad2Deg) //critical angle formula
        {
            //Debug.Log("Critical angle, Total internal reflection.");//reflect, find where it hits, check critical, recur until exit found or max recursions met
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
}
