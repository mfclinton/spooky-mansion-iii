using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header("Field of View")]
    [SerializeField] private float viewRadius;
    public float ViewRadius { get { return viewRadius; } }
    [SerializeField, Range(0, 360)] private float viewAngle;
    public float ViewAngle { get { return viewAngle; } }

    [Header("Layer Mask Settings")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstructionMask;

    [Header("Performance Settings")]
    [SerializeField] private float fovRoutineDelay = 0.2f;

    // Internal Variables
    public bool canSeePlayer { get; private set; }

    // Components Reference
    public FirstPersonController playerRef { get; private set; }

    void Awake() {
        playerRef = FindObjectOfType<FirstPersonController>();
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine() {
        WaitForSeconds wait = new WaitForSeconds(fovRoutineDelay);

        while (true) {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck() {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            if (angleToTarget < viewAngle / 2f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                
                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                {
                    canSeePlayer = true;
                }
                else
                {
                    canSeePlayer = false;
                }
            }
            else
            {
                canSeePlayer = false;
            }
        }
        else if (canSeePlayer)
        {
            canSeePlayer = false;
        }
    }
}
