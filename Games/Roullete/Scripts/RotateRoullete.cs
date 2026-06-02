using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRoullete : MonoBehaviour
{
    public float rotationSpeed = 90f; // Speed of rotation in degrees per second
    public float rotationSpeedSlow = 90f; // Speed of rotation in degrees per second
    public AnimationCurve rotationCurve;

    private bool isRotating = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRotating)
        {
            StartCoroutine(RotateObject());
        }
    }

    IEnumerator RotateObject()
    {
        isRotating = true;
        float elapsedTime = 0f;
        float totalRotationTime = 1f; // Total time for the rotation

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 180); // Rotate to 180 degrees around Z axis

        while (elapsedTime < totalRotationTime)
        {
            float curveTime = elapsedTime / totalRotationTime;
            float curveValue = rotationCurve.Evaluate(curveTime);

            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, curveValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        isRotating = false;
    }
    
    IEnumerator RotateObjectSlow()
    {
        isRotating = true;
        float elapsedTime = 0f;
        float totalRotationTime = 1f; // Total time for the rotation

        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 180); // Rotate to 180 degrees around Z axis

        while (elapsedTime < totalRotationTime)
        {
            float curveTime = elapsedTime / totalRotationTime;
            float curveValue = rotationCurve.Evaluate(curveTime);

            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, curveValue);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        isRotating = false;
    }
}

