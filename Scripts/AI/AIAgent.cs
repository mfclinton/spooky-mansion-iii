using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIAgent : MonoBehaviour
{
    [Header("Model Angle Settings")]
    [SerializeField] private bool updateModelAngle = true;
    [SerializeField] private Transform childModel; // Reference to the child model.
    [SerializeField] private float lerpSpeed = 10f; // Speed of interpolation.
    [SerializeField] private float smoothFactor = 0.1f; // Weight for the EWMA. A smaller value will make the direction change more slowly.
    private Vector3 lastPosition;
    private Vector3 averageDirection;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius;
    public float WanderRadius { get { return wanderRadius; } }
    [SerializeField] private float pauseDuration;
    public float PauseDuration { get { return pauseDuration; } }
    [SerializeField] private bool isWandering;
    public bool IsWandering { get { return isWandering; } }

    private Vector3 targetPosition;
    private float pauseTimer;
    public NavMeshAgent agent { get; private set;}

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        isWandering = true;
        lastPosition = transform.position;
        averageDirection = transform.forward;
    }

    private void Update()
    {
        if(!isWandering)
        {
            GoToTargetPosition();
        }
        else
        {
            Wander();
        }

        UpdateModelAngel();
    }

    private void Wander()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            pauseTimer += Time.deltaTime;

            if (pauseTimer >= pauseDuration)
            {
                pauseTimer = 0f;

                Vector3 newPos = RandomNavSphere(agent.transform.position, wanderRadius);
                agent.SetDestination(newPos);
            }
        }
    }

    private void GoToTargetPosition()
    {
        agent.SetDestination(targetPosition);
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        Debug.DrawRay(randDirection, Vector3.up, Color.magenta, 1.0f);

        NavMeshHit navHit;

        if (NavMesh.SamplePosition(randDirection, out navHit, 10.0f, NavMesh.AllAreas))
        {
            return navHit.position;
        }
        else
        {
            Debug.LogWarning("Failed to find NavMesh position for point " + randDirection.ToString());
            return Vector3.zero; // or some default/fallback position
        }
    }

    public void UpdateIsWanderingState(bool state)
    {
        isWandering = state;
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    private void UpdateModelAngel() {
        if (!updateModelAngle || childModel == null)
            return;

        Vector3 direction = (transform.position - lastPosition).normalized;
        averageDirection = Vector3.Lerp(averageDirection, direction, smoothFactor);

        // If the agent is moving (to avoid LookRotation creating a NaN Quaternion when the direction vector is zero)
        if (averageDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(averageDirection);
            childModel.rotation = Quaternion.Lerp(childModel.rotation, targetRotation, Time.deltaTime * lerpSpeed);
        }

        lastPosition = transform.position;
    }
}
