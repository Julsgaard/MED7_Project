using System;
using System.Collections;
using UnityEngine;

public class RotatePostItText : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private Coroutine _rotateCoroutine;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private IEnumerator RotateTextTowardsPlayer()
    {
        // Infinite loop until the coroutine is stopped
        while (true)
        {
            // Calculate the direction from the text to the player
            Vector3 newDir = mainCamera.transform.position - transform.position;
            newDir.y = 0; // Ignore the y-axis

            // Calculate the angle between the text and the player
            float angle = Mathf.Atan2(newDir.x, newDir.z) * Mathf.Rad2Deg;
            
            // Clamp angle to nearest 90 deg
            float clampedAngle = (float)Math.Round(((angle + 180) / 360) * 4) * 90;
            
            // Rotate the text towards the player
            transform.localRotation = Quaternion.Euler(90, 0, -clampedAngle);
            
            yield return new WaitForSeconds(0.5f); // Update every 0.5 seconds
        }
    }

    private void OnEnable()
    {
        _rotateCoroutine = StartCoroutine(RotateTextTowardsPlayer());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
