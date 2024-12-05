using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
        while (true)
        {
            Vector3 newDir = mainCamera.transform.position - transform.position;
            newDir.y = 0;
            // Debug.Log(newDir);

            float angle = Mathf.Atan2(newDir.x, newDir.z) * Mathf.Rad2Deg;
            
            // clamp angle to nearest 90 deg
            float clampedAngle = (float)Math.Round(((angle + 180) / 360) * 4) * 90;
            // Debug.Log($"Angle: {angle}. ClampedAngle: {clampedAngle}");
            
            transform.rotation = Quaternion.Euler(90, 0, -clampedAngle);
            
            // controls update frequency
            yield return new WaitForSeconds(0.5f);
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
