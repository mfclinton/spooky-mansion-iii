using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class AudioTrigger : MonoBehaviour
{
    // Helper functions
    private bool IsPlayer(GameObject obj) => obj == FirstPersonController.Instance.gameObject;
    private bool IsEnemy(GameObject obj) => obj.GetComponent<AIEnemy>() != null; // TODO: is this efficient?

    // Settings
    public enum DepthDirection
    {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ
    }

    [SerializeField] private DepthDirection localDepthDirectionEnum;
    private Vector3 localDepthDir;
    private Vector3 worldDepthDir;

    private static Vector3 DepthDirectionEnumToVector(DepthDirection depthDirectionEnum)
    {
        Vector3 depthDir;
        switch (depthDirectionEnum)
        {
            case DepthDirection.PositiveX:
                depthDir = Vector3.right;
                break;
            case DepthDirection.NegativeX:
                depthDir = Vector3.left;
                break;
            case DepthDirection.PositiveY:
                depthDir = Vector3.up;
                break;
            case DepthDirection.NegativeY:
                depthDir = Vector3.down;
                break;
            case DepthDirection.PositiveZ:
                depthDir = Vector3.forward;
                break;
            case DepthDirection.NegativeZ:
                depthDir = Vector3.back;
                break;
            default:
                depthDir = Vector3.zero;
                break;
        }

        return depthDir;
    }

    // Components
    BoxCollider boxCollider;

    private void Awake() {
        boxCollider = GetComponent<BoxCollider>();

        localDepthDir = DepthDirectionEnumToVector(localDepthDirectionEnum);
        worldDepthDir = boxCollider.transform.TransformDirection(localDepthDir);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other.gameObject)) 
        {
            print("Player entered the region");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsPlayer(other.gameObject)) 
        {
            Transform playerTransform = other.transform;
            print("Player is in the region");

            float distanceToWall = CalculateDistanceToWall(playerTransform);
            float angle = CalculateAngle(playerTransform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other.gameObject)) 
        {
            print("Player left the region");
        }
    }

    // Helper functions

    // Calculate the distance to the wall (0 is at the wall, 1 is at the other side of the box)
    float CalculateDistanceToWall(Transform transform) {
        Vector3 localPosAlongDepthDir = Vector3.Scale(boxCollider.transform.InverseTransformPoint(transform.position), localDepthDir);
        Vector3 endPosAlongDepthDir = Vector3.Scale(boxCollider.size, localDepthDir) / 2f;
        float distanceToWall = (endPosAlongDepthDir - localPosAlongDepthDir).magnitude;
        return distanceToWall;
    }

    // Calculate the angle of the player. 1 facing towards it, 0 facing perpendicular, -1 facing away and such
    float CalculateAngle(Transform transform) {
        float angle = Vector3.Dot(transform.forward, worldDepthDir);
        return angle;
    }
}
