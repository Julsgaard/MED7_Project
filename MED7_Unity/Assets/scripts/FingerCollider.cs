using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;

public class FingerCollider : MonoBehaviour
{
    public static PostItNoteNetwork postIt = null;
    private Vector3 oldPos;
    Material thisMaterial;
    private LineRenderer debugLine;
    private GameObject debugBall;
    Camera camera;

    //ManomotionManager manomotionManager;
    // Start is called before the first frame update
    void Start()
    {
        thisMaterial = gameObject.GetComponent<Renderer>().material;
        //manomotionManager = ManomotionManager.Instance;
        thisMaterial.color = Color.blue;
        debugLine = GetComponent<LineRenderer>();
        debugLine.startWidth = 0.001f; // Width at the start of the line
        debugLine.endWidth = 0.001f;   // Width at the end of the line
        camera = Camera.main;
        
    }

    // Update is called once per frame
    void Update()
    {
        HandInfo handInfo = ManomotionManager.Instance.Hand_infos[0].hand_info;
        if (handInfo.gesture_info.mano_gesture_trigger == ManoGestureTrigger.PICK)
        {
            thisMaterial.color = Color.cyan;
            
            if(postIt == null)
                CatchPostIt();
            
        }
        else if (handInfo.gesture_info.mano_gesture_trigger == ManoGestureTrigger.DROP)
        {
            /* TODO: Make this state "searching" where a ray is continually
             *  cast to find a post it. When one is found, bind it.
             */
            thisMaterial.color = Color.blue;
            postIt = null;
        }
        if (postIt != null)
        {
            MovePostIt(postIt);
        }
    }
    private void CatchPostIt()
    {
        Vector3 screenPoint = camera.WorldToScreenPoint(gameObject.transform.position);
        Ray cameraRay = camera.ScreenPointToRay(screenPoint);
        
        DisplayDebugLine(cameraRay);
        CheckRayHitsForPostIts(Physics.RaycastAll(cameraRay, 3));
    }

    private void CheckRayHitsForPostIts(RaycastHit[] hits)
    {
        foreach (RaycastHit hit in hits) 
        {
            GetComponentInChildren<TextMeshPro>().text = hit.collider.gameObject.tag;
            
            if (hit.collider.gameObject.CompareTag("Post-it"))
            {
                postIt = hit.collider.gameObject.GetComponent<PostItNoteNetwork>();
                
                if (postIt != null)
                    GetComponentInChildren<TextMeshPro>().text = postIt?.gameObject.name;
                
                break;
            }
        }
    }

    private void DisplayDebugLine(Ray cameraRay)
    {
        debugLine.positionCount = 2;
        debugLine.SetPosition(0, cameraRay.origin - new Vector3(0, 0.01f, 0));
        debugLine.SetPosition(1, cameraRay.origin + cameraRay.direction * 3);
    }
    
    private void MovePostIt(PostItNoteNetwork currentPostIt)
    {
        thisMaterial.color = Color.green;
        
        Vector3 screenPoint = camera.WorldToScreenPoint(gameObject.transform.position);
        Ray cameraRay = camera.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        
        DisplayDebugLine(cameraRay);
        
        /* @frederik, I added back the "table" layer mask. Here it;s fine, but when
         * we before looked for post its, it needed to be removed.
         */
        if (Physics.Raycast(cameraRay, out hit, 3, LayerMask.GetMask("Table"), QueryTriggerInteraction.Ignore))
        {
            currentPostIt.RequestMoveNoteServerRpc(hit.point);
            
            if (debugBall == null)
            {
                debugBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                debugBall.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            }
            debugBall.transform.position = hit.point;
        }
    }
}
