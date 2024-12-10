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
        
        //thisMaterial.color = Color.green;
        //movePostIt(postIt);
        //GetComponentInChildren<TextMeshPro>().text = gameObject.transform.position.ToString();

        HandInfo handInfo = ManomotionManager.Instance.Hand_infos[0].hand_info;
        
        if (handInfo.gesture_info.mano_gesture_trigger == ManoGestureTrigger.PICK)
        {
            Debug.Log($"Registered Pick");
            thisMaterial.color = Color.cyan;
            
            if(postIt == null)
            {
                CatchPostIt();
            }
        }
        else if (handInfo.gesture_info.mano_gesture_trigger == ManoGestureTrigger.DROP)
        {
            Debug.Log($"Registered Drop");

            thisMaterial.color = Color.blue;

            if (postIt != null)
            {

                postIt = null;
            }
            
            Destroy(debugBall);
        }
        if (postIt != null)
        {
            Debug.Log($"Trying to move post it");

            thisMaterial.color = Color.green;
            debugBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugBall.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            MovePostIt(postIt);
        }
    }
    private void CatchPostIt()
    {
        Vector3 screenPoint = camera.WorldToScreenPoint(gameObject.transform.position);
        Ray cameraRay = camera.ScreenPointToRay(screenPoint);
        
        DisplayDebugLine(cameraRay);
        CheckRayHitsForPostIts(Physics.RaycastAll(cameraRay, 1000));
    }

    private void CheckRayHitsForPostIts(RaycastHit[] hits)
    {
        Debug.Log($"Ray hit {hits.Length} objects");
        
        foreach (RaycastHit hit in hits) 
        {
            GetComponentInChildren<TextMeshPro>().text = hit.collider.gameObject.tag;
            
            Debug.Log($"Looping through hit objects: {hit.collider.gameObject.tag}: {hit.collider.gameObject.name}");
            
            if (hit.collider.gameObject.tag == "Post-it")
            {
                Debug.Log($"Tag was 'post-it'");
                
                postIt = hit.collider.gameObject.GetComponent<PostItNoteNetwork>();
                if (postIt != null)
                {
                    Debug.Log($"Post it was null. Setting post it to {postIt.gameObject.name}");
                    
                    GetComponentInChildren<TextMeshPro>().text = postIt.gameObject.name;
                }
                break;
            }
        }
    }

    private void DisplayDebugLine(Ray cameraRay)
    {
        debugLine.positionCount = 2;
        debugLine.SetPosition(0, cameraRay.origin - new Vector3(0, 0.01f, 0));
        debugLine.SetPosition(1, cameraRay.origin + cameraRay.direction * 10000);
    }
    
    // TODO: find out why table says untagged
    // TODO: find out how to catch post its
    private void MovePostIt(PostItNoteNetwork currentPostIt)
    {
        Vector3 screenPoint = camera.WorldToScreenPoint(gameObject.transform.position);
        Ray cameraRay = camera.ScreenPointToRay(screenPoint);
        RaycastHit hit;
        
        DisplayDebugLine(cameraRay);
        
        Debug.Log($"Creating new ray from {cameraRay.origin} to {cameraRay.direction}.");
        
        if (Physics.Raycast(cameraRay, out hit, 1000))
        {
            Debug.Log($"Hit object: {hit.collider.gameObject.name}");

            currentPostIt.RequestMoveNoteServerRpc(hit.transform.position);
            debugBall.transform.position = hit.transform.position;
        }
     
        /*
         * Old Movement logic
        Vector3 movePos = gameObject.transform.position - oldPos;
        movePos = new Vector3(movePos.x, 0, movePos.z) * (Vector3.Distance(camera.transform.position, currentPostIt.transform.position)/Vector3.Distance(camera.transform.position, gameObject.transform.position));
        currentPostIt.RequestMoveNote(movePos);
        oldPos = gameObject.transform.position;
        */
    }
}
