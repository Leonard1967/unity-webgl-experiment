// File: HandController.cs
using UnityEngine;

public class HandController : MonoBehaviour
{
    public Transform leftTarget;
    public Transform rightTarget;

    private Vector3 originalPos;
    private Vector3 targetPos;

    private bool isMoving = false;
    private float moveStartTime;
    private float moveDuration = 0.4f; // 400 ms
    private float pauseDuration = 0.2f; // 200 ms
    private bool returning = false;

    void Start()
    {
        originalPos = transform.position;
        targetPos = originalPos;
    }

    void Update()
    {
        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.W) && leftTarget != null)
            {
                StartMovement(leftTarget.position);
            }
            else if (Input.GetKeyDown(KeyCode.P) && rightTarget != null)
            {
                StartMovement(rightTarget.position);
            }
        }
        else
        {
            float elapsed = Time.time - moveStartTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);

            if (!returning)
                transform.position = Vector3.Lerp(originalPos, targetPos, t);
            else
                transform.position = Vector3.Lerp(targetPos, originalPos, t);

            // Optional: Hand etwas neigen
            // transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

            if (t >= 1f)
            {
                isMoving = false;
                if (!returning)
                    Invoke(nameof(StartReturn), pauseDuration);
            }
        }
    }

    void StartMovement(Vector3 destination)
    {
        targetPos = destination;
        moveStartTime = Time.time;
        isMoving = true;
        returning = false;
    }

    void StartReturn()
    {
        moveStartTime = Time.time;
        isMoving = true;
        returning = true;
    }
}
