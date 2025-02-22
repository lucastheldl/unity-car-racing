using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float speed;
    //public float minX, maxX,minY,maxY;

    float ZoomMinBound = 0.1f;
    float ZoomMaxBound = 10000f;


    Vector3 touchStart;
    Camera camera;


    public float zoomSpeed = 20f;
    public float minZoomFOV = 10f;

    //
    private float initialZoom;

    private Vector2 initialTouchPosition;
    private Vector3 initialCameraPosition;
    private float initialCameraSize;

    private bool isDragging = false;
    public bool hasTouch;

    private Vector2 initialTouch1Position;
    private Vector2 initialTouch2Position;
    private float initialDistance;
    private bool isPinching = false;

    private int previousTouchCount = 0;
    private void Start()
    {
        camera = GetComponent<Camera>();
        initialZoom = camera.orthographicSize;
    }

    void Update()
    {
        MoveCam();

    }
    void MoveCam()
    {

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                initialTouchPosition = touch.position;
                initialCameraPosition = camera.transform.position;
                isDragging = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 deltaTouch = touch.position - initialTouchPosition;
                if (!isDragging && deltaTouch.magnitude >= 10f) // Check if dragging distance is greater than a threshold
                {
                    isDragging = true;
                }

                if (isDragging)
                {
                    Vector3 worldDelta = camera.ScreenToWorldPoint(deltaTouch) - camera.ScreenToWorldPoint(Vector2.zero);
                    Vector3 cameraPosition = initialCameraPosition - worldDelta;
                    camera.transform.position = cameraPosition;
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                isDragging = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch2.phase == TouchPhase.Began)
            {
                initialTouch1Position = touch1.position;
                initialTouch2Position = touch2.position;
                initialDistance = Vector2.Distance(initialTouch1Position, initialTouch2Position);
                isPinching = true;
                initialZoom = camera.orthographicSize;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                if (isPinching)
                {
                    Vector2 currentTouch1Position = touch1.position;
                    Vector2 currentTouch2Position = touch2.position;
                    float currentDistance = Vector2.Distance(currentTouch1Position, currentTouch2Position);

                    float touchDelta = currentDistance - initialDistance;
                    float zoomFactor = touchDelta * zoomSpeed;

                    camera.orthographicSize = Mathf.Clamp(initialZoom - zoomFactor, 1f, float.MaxValue);
                }
            }
        }

        // Check if pinch ended
        if (previousTouchCount == 2 && Input.touchCount < 2)
        {
            isPinching = false;
        }

        previousTouchCount = Input.touchCount;
    






    /* Vector3 camPos = transform.position;


     if (Input.GetMouseButtonDown(0))
     {
         touchStart = camera.ScreenToWorldPoint(Input.mousePosition);
     }
     if (Input.GetMouseButton(0))
     {
         Vector3 direction = touchStart - camera.ScreenToWorldPoint(Input.mousePosition);
             camPos.x += direction.x;
             if (warMode)
             {
                 camPos.y += direction.y;
             }
     }

     //Mouse

     if (Input.mousePosition.x > Screen.width - 30)
     {
         if (transform.position.x < maxX)
         {
             camPos.x += speed * Time.deltaTime;
         }
     }
     if (Input.mousePosition.x < 30 )
     {
         if (transform.position.x > minX)
         {
             camPos.x -= speed * Time.deltaTime;
         }
     }


     if (warMode) {
         //Mouse
         if (Input.mousePosition.y > Screen.height - 30  )
         {
             if (transform.position.y < maxY)
             {
                 camPos.y += speed * Time.deltaTime;
             }
         }
         if (Input.mousePosition.y < 30 )
         {
             if (transform.position.y > minY)
             {
                 camPos.y -= speed * Time.deltaTime;
             }
         }

     }
     if (Input.GetAxis("Mouse ScrollWheel") > 0)
     {
         ZoomIn();
     }else if (Input.GetAxis("Mouse ScrollWheel") < 0)
     {
         ZoomOut();
     }

     transform.position = camPos;*/

}
    void Zoom(float deltaMagnitudeDiff, float speed)
    {

        camera.fieldOfView += deltaMagnitudeDiff * speed;
        // set min and max value of Clamp function upon your requirement
        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, ZoomMinBound, ZoomMaxBound);
    }
    public void ZoomIn()
    {
        camera.orthographicSize -= zoomSpeed;
        if (camera.orthographicSize < minZoomFOV)
        {
            camera.orthographicSize = minZoomFOV;
        }
    }public void ZoomOut()
    {
        camera.orthographicSize += zoomSpeed;
    }
}
