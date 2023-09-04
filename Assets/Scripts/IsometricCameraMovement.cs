using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricCameraMovement : MonoBehaviour
{
    [Header("Camera Zoom Settings")]
    [SerializeField]private float zoomInRot = 25f;
    [SerializeField]private float zoomOutRot = 45f;
    [Tooltip("Smaller value means zoom in")][SerializeField] private float zoomOutSize = 5f;
    [Tooltip("Smaller value means zoom in")][SerializeField] private float zoomInSize = 3f;
    public float zoomSpd = 90;
    [Header("Camera Rotate Settings")]
    [SerializeField]private float rotateSpd;
    [Header("Camera Move Settings")]
    [SerializeField]private bool enableMouseCam;
    [SerializeField]private float kbMoveSpd;
    [SerializeField]private float moveSpd;
    [SerializeField]private float moveTime;
    [SerializeField]private Vector2 screenThreshold;
    //public Bounds moveBoundingBox;
    private GridManager grid => GridManager.theGridManager;

    [Header("Debug")]
    [SerializeField]private float curXRotTarget;
    [SerializeField]private float curZoomTarget;
    [SerializeField]private float curYRot;
    [SerializeField]private Vector3 newPos;
    [SerializeField]private Quaternion newRot;
    private Vector3 dragStart;
    private Vector3 dragCurPos;
    void Awake()
    {
        newPos = transform.position;
        curXRotTarget = 45;
        curZoomTarget = 5;

        //moveBoundingBox = new Bounds(new Vector3(grid.gridDimention.x/2, 0, grid.gridDimention.y/2), new Vector3(grid.gridDimention.x, 30, grid.gridDimention.y));
    }
    void Update()
    {
        if(enableMouseCam)
        CamMouseMovePosition();
        CamWasdPosition();
        CamMouseDragPosition();
        CamRotation();
        CamZoom();

        curXRotTarget = Mathf.Clamp(curXRotTarget, 25, 45);
        curZoomTarget = Mathf.Clamp(curZoomTarget, 3, 5);

        newRot = Quaternion.Euler(curXRotTarget,curYRot,0);
        newPos = new Vector3(Mathf.Clamp(newPos.x, 0, grid.gridDimention.x), 0, Mathf.Clamp(newPos.z, 0, grid.gridDimention.y));

        void CamMouseMovePosition()
        {
            var screenCenter = new Vector3(Screen.width/2, 0, Screen.height/2);
            if(Mathf.Abs(Input.mousePosition.x - Screen.width/2) > (Screen.width/2) - screenThreshold.x|| 
                Mathf.Abs(Input.mousePosition.y - Screen.height/2) > (Screen.height/2) - screenThreshold.y)//Vector3.Distance(Input.mousePosition, screenCenter) > screenThreshold)
            {
                var axis = new Vector3(Input.mousePosition.x, 0, Input.mousePosition.y) - screenCenter;
                var dir = Quaternion.AngleAxis(transform.localEulerAngles.y, Vector3.up) * axis.normalized;
                newPos += moveSpd * dir;
            }
        }

        void CamWasdPosition()
        {
            var dir = Quaternion.AngleAxis(transform.localEulerAngles.y, Vector3.up) * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            newPos += kbMoveSpd * dir;
        }
        void CamMouseDragPosition()
        {
             if(Input.GetMouseButtonDown(0))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(plane.Raycast(ray, out float enter))
                {
                    dragStart = ray.GetPoint(enter);
                }
            }
            if(Input.GetMouseButton(0))
            {
                Plane plane = new Plane(Vector3.up, Vector3.zero);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(plane.Raycast(ray, out float enter))
                {
                    dragCurPos = ray.GetPoint(enter);
                    newPos = transform.position + dragStart - dragCurPos;
                    transform.position = newPos; // no smoothing on drag
                }
            }
        }
        void CamRotation()
        {
            if(Input.GetKey(KeyCode.Q))
            {
                //rotate -y
                curYRot = Mathf.MoveTowards(curYRot, curYRot - 10f, rotateSpd * Time.deltaTime);
            }
            if(Input.GetKey(KeyCode.E))
            {
                //rotate y
                curYRot = Mathf.MoveTowards(curYRot, curYRot + 10f, rotateSpd * Time.deltaTime);
            }

        }
        void CamZoom()
        {
             if(Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                //zoom in
                curXRotTarget = Mathf.SmoothStep(curXRotTarget, zoomInRot, zoomSpd * Time.deltaTime);
                curZoomTarget = Mathf.SmoothStep(curZoomTarget, zoomInSize, zoomSpd * Time.deltaTime);
            }

            if(Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                //zoom out
                curXRotTarget = Mathf.SmoothStep(curXRotTarget, zoomOutRot, zoomSpd * Time.deltaTime);
                curZoomTarget = Mathf.SmoothStep(curZoomTarget, zoomOutSize, zoomSpd * Time.deltaTime);
            }
        }
    }
    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, newPos, moveTime * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRot, moveTime * Time.deltaTime);
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, curZoomTarget, moveTime * Time.deltaTime);
    }

}
