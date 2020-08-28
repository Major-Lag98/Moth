using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class Laser : MonoBehaviour
{

    [SerializeField]
    int _maxRecursions = 10;
    [SerializeField]
    float _maxStepDistance = 200;


    float _airIndex = 1.0f;
    [SerializeField]
    float _glassIndex = 1.5f;
    

    [SerializeField]
    Color white = Color.white;
    [SerializeField]
    Color red = Color.red;
    [SerializeField]
    Color redPurple = new Color(0.5019608f, 0, 0.2509804f, 1);
    [SerializeField]
    Color purple = new Color(0.5019608f, 0, 0.5019608f, 1);
    [SerializeField]
    Color bluePurple = new Color(0.5411765f, 0.1686275f, 0.8862745f, 1);
    [SerializeField]
    Color blue = Color.blue;
    [SerializeField]
    Color blueGreen = new Color(0.05098039f, 0.5960785f, 0.7294118f, 1);
    [SerializeField]
    Color green = Color.green;
    [SerializeField]
    Color yellowGreen = new Color(0.6039216f, 0.8039216f, 0.1960784f, 1);
    [SerializeField]
    Color yellow = Color.yellow;
    [SerializeField]
    Color yellowOrange = new Color(1, 0.8f, 0.25f, 1);
    [SerializeField]
    Color orange = new Color(1, 0.6470588f, 0, 1);
    [SerializeField]
    Color redOrange = new Color(1, 0.3254902f, 0.2862745f, 1);


    public GameObject laserToSpawn;
    public MyColor.ColorState myCurrentColorState = new MyColor.ColorState();

    List<GameObject> laserList = new List<GameObject>(); // Holds the currently used active lasers
    List<GameObject> lightList = new List<GameObject>(); // Holds the currently used active lights

    int laserCounter = 0; // The number of lasers we create each update. Needed for keeping the laserList in check
    int lightCounter = 0; // The number of lights we create each update. Needed for keeping the lightList in check


    // Update is called once per frame
    private void Update()
    {
        //myCurrentColorState = MyColor.ColorState.BLUEPURPLE;
        

        if (!Application.isPlaying)
        {
            return;
        }

        // ------- TODO I wouldn't do this every frame. Not needed

        //foreach (GameObject laser in ObjectPooler.SharedInstance.pooledObjects) //every update redraw laser
        //    laser.SetActive(false);

        //foreach (GameObject light in GameObject.FindGameObjectsWithTag("Light")) //every update redraw pointlights
        //    light.SetActive(false);

        //ObjectPooler.SharedInstance.pooledObjects;

        DrawPredictedReflection(this.transform.position, this.transform.up, _maxRecursions, myCurrentColorState);

        // Cull the laser list, which stores the unused lasers in the object pool
        laserList = CullList(laserList, laserCounter, "laser");
        lightList = CullList(lightList, lightCounter, "light");

        // Reset the counter at the end
        laserCounter = 0;
        lightCounter = 0;
    }

    void DrawPredictedReflection(Vector2 origin, Vector2 direction, int recursionsRemaining, MyColor.ColorState colorState)// int reflectionsRemaining, int splitsRemaining)
    {

        var gizmoHue = (recursionsRemaining / (this._maxRecursions + 1f));

        RaycastHit2D hit2D = Physics2D.Raycast(origin, direction, _maxStepDistance);

        if (hit2D && recursionsRemaining > 0) //did we hit somthing?
        {

            DrawLaser(origin, hit2D.point, colorState, colorState); //draw a line to it

            if (hit2D.transform.gameObject.tag == "Receiver")
            {

                Receiver receiver = hit2D.transform.gameObject.GetComponent<Receiver>();
                if (receiver.myColor == colorState)
                {
                    receiver.isCharging = true;
                }

            }

            else if (hit2D.transform.gameObject.tag == "Mirror") //mirror hit. set new pos where hit. reflect angle and make that new direction
            {
                    
                    direction = Vector2.Reflect(direction, hit2D.normal);
                    origin = hit2D.point + direction * 0.01f;
                    DrawPredictedReflection(origin, direction, --recursionsRemaining, colorState);

            }

            else if (hit2D.transform.gameObject.tag == "Splitter") //reflect and go ahead
            {

                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, _airIndex, _glassIndex, colorState, colorState); //enter and refract
                    direction = Vector2.Reflect(direction, hit2D.normal);
                    origin = hit2D.point + direction * 0.01f;
                    DrawPredictedReflection(origin, direction, --recursionsRemaining, colorState); //reflect too
                
            }

            else if (hit2D.transform.gameObject.tag == "Lens")
            {

                Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, _airIndex, _glassIndex, colorState, colorState);

            }
            else if (hit2D.transform.gameObject.tag == "Prism")
            {

                if (colorState == MyColor.ColorState.WHITE)
                {
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, _airIndex, _glassIndex - .1f , colorState, MyColor.ColorState.RED, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, _airIndex, _glassIndex - .06f, colorState, MyColor.ColorState.ORANGE, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, _airIndex, _glassIndex - .03f, colorState, MyColor.ColorState.YELLOW, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, _airIndex, _glassIndex       , colorState, MyColor.ColorState.GREEN, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, _airIndex, _glassIndex + .03f, colorState, MyColor.ColorState.BLUE, 0.4f);
                    Refract(origin, hit2D.normal, direction, hit2D.point, hit2D.transform.gameObject, recursionsRemaining, _airIndex, _glassIndex + .06f, colorState, MyColor.ColorState.PURPLE, 0.4f);
                }

            }
            else if (hit2D.transform.gameObject.tag == "Filter") // change the state of the color
            {
                Filter filter = hit2D.transform.GetComponent<Filter>();

                if (   colorState != MyColor.ColorState.BLUEGREEN 
                    && colorState != MyColor.ColorState.BLUEPURPLE 
                    && colorState != MyColor.ColorState.REDPURPLE 
                    && colorState != MyColor.ColorState.YELLOWGREEN 
                    && colorState != MyColor.ColorState.YELLOWORANGE) //tirtiary colors cant be changed
                {
                    if (   !(colorState == MyColor.ColorState.PURPLE && filter.isYellow) 
                        && !(colorState == MyColor.ColorState.ORANGE && filter.isBlue) 
                        && !(colorState == MyColor.ColorState.GREEN && filter.isRed)) //complemantary colors cant be changed
                    {

                        MyColor.ColorState startColor = colorState;
                        if (filter.isBlue) //I feel like i could make this better...
                        {

                            if (colorState == MyColor.ColorState.WHITE)
                            {
                                colorState = MyColor.ColorState.BLUE;
                            }
                            else if (colorState == MyColor.ColorState.BLUE)
                            {
                                colorState = MyColor.ColorState.BLUE;
                            }
                            else if (colorState == MyColor.ColorState.RED)
                            {
                                colorState = MyColor.ColorState.PURPLE;
                            }
                            else if (colorState == MyColor.ColorState.YELLOW)
                            {
                                colorState = MyColor.ColorState.GREEN;
                            }
                            else if (colorState == MyColor.ColorState.GREEN)
                            {
                                colorState = MyColor.ColorState.BLUEGREEN;
                            }
                            else if (colorState == MyColor.ColorState.PURPLE)
                            {
                                colorState = MyColor.ColorState.BLUEPURPLE;
                            }

                        }
                        if (filter.isRed)
                        {
                            if (colorState == MyColor.ColorState.WHITE)
                            {
                                colorState = MyColor.ColorState.RED;
                            }
                            else if (colorState == MyColor.ColorState.BLUE)
                            {
                                colorState = MyColor.ColorState.PURPLE;
                            }
                            else if (colorState == MyColor.ColorState.RED)
                            {
                                colorState = MyColor.ColorState.RED;
                            }
                            else if (colorState == MyColor.ColorState.YELLOW)
                            {
                                colorState = MyColor.ColorState.ORANGE;
                            }
                            else if (colorState == MyColor.ColorState.ORANGE)
                            {
                                colorState = MyColor.ColorState.REDORANGE;
                            }
                            else if (colorState == MyColor.ColorState.PURPLE)
                            {
                                colorState = MyColor.ColorState.REDPURPLE;
                            }
                        }
                        if (filter.isYellow)
                        {
                            if (colorState == MyColor.ColorState.WHITE)
                            {
                                colorState = MyColor.ColorState.YELLOW;
                            }
                            else if (colorState == MyColor.ColorState.BLUE)
                            {
                                colorState = MyColor.ColorState.GREEN;
                            }
                            else if (colorState == MyColor.ColorState.RED)
                            {
                                colorState = MyColor.ColorState.ORANGE;
                            }
                            else if (colorState == MyColor.ColorState.YELLOW)
                            {
                                colorState = MyColor.ColorState.YELLOW;
                            }
                            else if (colorState == MyColor.ColorState.GREEN)
                            {
                                colorState = MyColor.ColorState.YELLOWGREEN;
                            }
                            else if (colorState == MyColor.ColorState.ORANGE)
                            {
                                colorState = MyColor.ColorState.YELLOWORANGE;
                            }
                        }
                        origin = hit2D.point;
                        RaycastHit2D oppositePosition = FindOpp(origin + direction, -direction, hit2D.transform.gameObject);
                        Vector2 oppPos = oppositePosition.point;
                        DrawLaser(origin, oppPos, startColor, colorState);
                        DrawPredictedReflection(oppPos + direction * 0.01f, direction, --recursionsRemaining, colorState);
                    }
                }
            }
        }
        else //nothing hit
        {
            DrawLaser(origin, origin + direction * _maxStepDistance, colorState, colorState);
        }
    }

    void DrawLaser(Vector2 start, Vector2 end, MyColor.ColorState startColor, MyColor.ColorState endColor, float lightIntensity = 1.0f)
    {
        //GameObject laser = ObjectPooler.SharedInstance.GetPooledObject("Laser");

        // Validate lasers makes sure our list has enough lasers in it. It returns itself so that we can easily access the array in one call
        GameObject laser = ValidateList(laserList, laserCounter, "laser")[laserCounter];
        laserCounter++; // Increment the counter

        if (laser != null)
        {

            laser.transform.position = Vector2.zero;
            laser.transform.rotation = Quaternion.identity;
            LineRenderer lr = laser.GetComponent<LineRenderer>();
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.startColor = GetColorFromColorState( startColor);
            lr.endColor = GetColorFromColorState(endColor);
            laser.SetActive(true);
        }

        // We get the light by 'validating' the list. Then we can pull the light from the list using our light counter
        GameObject light = ValidateList(lightList, lightCounter, "light")[lightCounter];
        lightCounter++;

        if (light != null)
        {
            light.transform.position = (Vector3)end;
            light.transform.rotation = Quaternion.identity;
            Light2D l = light.GetComponent<Light2D>();
            //l.range = range;
            l.intensity = lightIntensity;
            l.color = GetColorFromColorState(endColor);
            light.SetActive(true);
        }
    }

    /// <summary>
    /// Validates that the list has enough objects in it. If not, the list will be appended to by objects
    /// from the object pool
    /// </summary>
    /// <param name="listOfObjects">The list of objects to validate</param>
    /// <param name="length">The length that the list needs to be </param>
    /// <param name="objectPoolName">The name to use to get objects from the ObjectPooler</param>
    /// <returns></returns>
    List<GameObject> ValidateList(List<GameObject> listOfObjects, int length, string objectPoolName)
    {
        // While our index is still larger than the list
        while(length >= listOfObjects.Count)
        {
            var obj = ObjectPooler.Instance.GetPooledObject(objectPoolName);
            //TODO Probably want to instantiate a laser here if null. 
            // if(obj == null)
            // Instantiate(laser)
            listOfObjects.Add(obj);
        }

        return listOfObjects;
    }

    /// <summary>
    /// Culls a list and sends the unused objects to the ObjectPooler
    /// </summary>
    /// <param name="list">The list of objects to cull</param>
    /// <param name="lengthToKeep">The length of the list to keep</param>
    /// <param name="name">The name to use when the objects get pooled</param>
    /// <returns>The culled list</returns>
    List<GameObject> CullList(List<GameObject> list, int lengthToKeep, string name)
    {
        for (int i = lengthToKeep; i < list.Count; i++) // Pool each object from the length ot keep to the end of the list
            ObjectPooler.Instance.PoolObject(name, list[i]);

        // Get a range of the list. 0 to whichever is less, the length of the list or the length to keep.
        list = list.GetRange(0, Math.Min(list.Count, lengthToKeep));
        return list; // Return it for chaining
    }

    void Refract(Vector2 origin, Vector2 normal, Vector2 direction, Vector2 point, GameObject lastHit, int recursionsRemainging, float n1, float n2, MyColor.ColorState startColor, MyColor.ColorState endColor, float lightIntensity = 1.0f) //entering a lens and exiting
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

    void CriticalAngle(Vector2 origin, Vector2 direction, Vector2 normal, GameObject lastHit, int recursionsRemainging, float n1, float n2, MyColor.ColorState color, float lightIntensity)
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

    public Color GetColorFromColorState(MyColor.ColorState myCurrentColorState)
    {
        Color color = new Color();

        switch (myCurrentColorState)
        {
            case MyColor.ColorState.WHITE:
                color = white;
                break;
            case MyColor.ColorState.BLUEGREEN:
                color = blueGreen;
                break;
            case MyColor.ColorState.BLUEPURPLE:
                color = bluePurple;
                break;
            case MyColor.ColorState.GREEN:
                color = green;
                break;
            case MyColor.ColorState.ORANGE:
                color = orange;
                break;
            case MyColor.ColorState.PURPLE:
                color = purple;
                break;
            case MyColor.ColorState.RED:
                color = red;
                break;
            case MyColor.ColorState.REDORANGE:
                color = redOrange;
                break;
            case MyColor.ColorState.REDPURPLE:
                color = redPurple;
                break;
            case MyColor.ColorState.YELLOW:
                color = yellow;
                break;
            case MyColor.ColorState.YELLOWGREEN:
                color = yellowGreen;
                break;
            case MyColor.ColorState.YELLOWORANGE:
                color = yellowOrange;
                break;
            case MyColor.ColorState.BLUE:
                color = blue;
                break;
        }
            

        return color;
    }
}