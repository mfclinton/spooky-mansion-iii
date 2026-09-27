using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FirstPersonController))]
public class FirstPersonControllerEditor : Editor
{
    private FirstPersonController fpc;

    private void OnEnable()
    {
        fpc = (FirstPersonController)target;
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.blue;
        Handles.DrawWireDisc(fpc.transform.position, Vector3.up, fpc.baseNoiseRadius);

        Handles.color = Color.green;
        Handles.DrawWireDisc(fpc.transform.position, Vector3.up, fpc.baseNoiseRadius * fpc.crouchNoiseRadiusMultiplier);

        Handles.color = Color.red;
        Handles.DrawWireDisc(fpc.transform.position, Vector3.up, fpc.baseNoiseRadius * fpc.sprintNoiseRadiusMultiplier);

        Handles.color = Color.gray;
        Handles.DrawWireDisc(fpc.transform.position, Vector3.up, fpc.baseNoiseRadius * fpc.shootNoiseRadiusMultiplier);
    }
}
