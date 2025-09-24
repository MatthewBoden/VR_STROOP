using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slingshot : MonoBehaviour
{
    //parent for the entire slingshot object
    public GameObject parent;
    //the prongs of the sling
    //THIS IS MAINLY FOR HOW I HAVE SETUP THE SLINGSHOT MODEL
    public GameObject prongs;
    //ball to shoot
    public GameObject ball;
    //grabbable part of the sling
    public GameObject sling;
    //script of the sling
    public Tool grabbable;
    //ball that is shot
    GameObject shotBall;
    //bar that fills up showing amount of pull
    public Image fillBar;
    //center position of the sling
    Vector3 homePos;
    //color for the fillbar when the sling is pulled a minimum amount
    Color minPullColor = new Color(0.0f, 1.0f, 0.0f);
    //color for the fillbar when the sling is pulled to the maximum amount
    Color maxPullColor = new Color(1.0f, 0.0f, 0.0f);
    //maximum distance the sling can be pulled
    const float PULL_DISTANCE = 2.5f;
    //distance away from home from which the sling is considered pulled back
    const float AWAY_FROM_HOME_DIST = 0.1f;
    //force to launch the ball
    const float BALL_FORCE = 1000.0f;
    //velocity mag to consider the shot ball has stopped
    //const float BALL_LOW_VEL_THRES = 0.1f;
    //speed to move the sling back
    float speed = 0.25f;
    //amount to fill the bar
    //also used for pullback amount
    float fillAmount = 0.0f;
    //is the sling grabbed
    bool isGrabbed = false;
    //is the sling awway from home
    bool awayFromHome = false;
    //is the sling loaded
    bool isLoaded = true;

    // Start is called before the first frame update
    void Start()
    {
        homePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //the distance of the sling from the home position
        float distanceFromHome = Vector3.Distance(sling.transform.position, homePos);

        //set if the sling is away from home
        awayFromHome = distanceFromHome > AWAY_FROM_HOME_DIST;

        //set if the sling is grabbed
        isGrabbed = grabbable.IsGrabbed || awayFromHome && grabbable.IsGrabbing;

        if (isGrabbed)
        {
            //rotate the entire object to aim the sling
            Vector3 dir = Vector3.Normalize(homePos - CursorController.Instance.CursorPos);

            parent.transform.rotation = Quaternion.LookRotation(dir, transform.up);
        }

        //FOR DEBUGGING
        /*
        //if the ball has been shot after it reaches the velocity threshold do something
        if (!isLoaded && shotBall)
        {
            float ballFromSlingDist = Vector3.Distance(shotBall.transform.position, ball.transform.position);
            if(shotBall.GetComponent<Rigidbody>().velocity.magnitude <= BALL_LOW_VEL_THRES && ballFromSlingDist > AWAY_FROM_HOME_DIST)
                ReloadSlingshot();
        }
        */
    }

    /// <summary>
    /// Reload the sling to be fired again
    /// </summary>
    public void ReloadSlingshot()
    {
        Destroy(shotBall);
        isLoaded = true;
        ball.SetActive(true);
    }

    private void LateUpdate()
    {
        float distance = Vector3.Distance(CursorController.Instance.CursorPos, homePos);

        //set the fillAmount based on the cursor 
        float slingDist = Vector3.Distance(sling.transform.position, homePos);
        fillAmount = slingDist / PULL_DISTANCE;

        //if the sling is being grabbed
        if (isGrabbed)
        {
            //if below the pull distance move the sling to the cursor position
            if (distance <= PULL_DISTANCE)
                //transform.position = new Vector3(transform.position.x, transform.position.y, clampedCursor.z);
                sling.transform.position = CursorController.Instance.CursorPos;

            //set the fillBar amount as we are pulling
            fillBar.fillAmount = fillAmount;
            fillBar.color = Color.Lerp(minPullColor, maxPullColor, fillAmount);
        }
        //if the sling is released and is not at home yet
        else if(!isGrabbed && awayFromHome)
        {
            //move the sling back to home
            //sling.transform.position = Vector3.Lerp(homePos, sling.transform.position, speed * Time.deltaTime);
            sling.transform.position = homePos;
            distance = Vector3.Distance(sling.transform.position, homePos);

            //if the sling is loaded and we have reached home
            if(isLoaded && distance <= AWAY_FROM_HOME_DIST)
            {
                //shoot the ball by instantiating and launching it from the ball position
                Rigidbody rg;
                isLoaded = false;
                ball.SetActive(false);
                shotBall = Instantiate(ball,ball.transform.position,ball.transform.rotation);
                shotBall.transform.localScale = ball.transform.lossyScale;
                rg = shotBall.GetComponent<Rigidbody>();
                shotBall.SetActive(true);
                rg.useGravity = true;
                rg.AddForce((transform.forward) * (BALL_FORCE * fillAmount));
            }
        }
    }

    public bool IsGrabbed
    {
        get { return isGrabbed; }
    }
    public bool IsAwayFromHome
    {
        get { return awayFromHome; }
    }
    public bool IsLoaded
    {
        get { return isLoaded; }
    }
    public bool SlingFired
    {
        get {
            float ballFromSlingDist = Vector3.Distance(shotBall.transform.position, ball.transform.position);
            return !isLoaded && shotBall && ballFromSlingDist > AWAY_FROM_HOME_DIST; 
        }
    }
    public GameObject ShotBall
    {
        get { return shotBall; }
    }


}
