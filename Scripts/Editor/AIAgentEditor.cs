using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AIAgent))]
public class AIAgentEditor : Editor
{
    void OnSceneGUI()
    {
        AIAgent agent = (AIAgent)target;

        // Draw a wire sphere to represent the wander bounds
        Handles.color = Color.blue;
        float cubeLength = agent.WanderRadius * 2;
        Handles.DrawWireCube(agent.transform.position, new Vector3(cubeLength, cubeLength, cubeLength));
    }

    public override void OnInspectorGUI()
    {
        AIAgent agent = (AIAgent)target;

        DrawDefaultInspector();
    }
}
