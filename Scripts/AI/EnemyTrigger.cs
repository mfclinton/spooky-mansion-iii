using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyTrigger : MonoBehaviour
{
    private AIEnemy aiEnemy;
    private void Awake() {
        aiEnemy = GetComponentInParent<AIEnemy>();
    }

    private void OnTriggerEnter(Collider other) {
        TriggerChecks(other);
    }

    private void OnTriggerStay(Collider other) {
        TriggerChecks(other);
    }

    void TriggerChecks(Collider other) {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        DoorTrigger doorTrigger = other.GetComponent<DoorTrigger>();

        if(doorTrigger != null) {
            doorTrigger.ForceDoorOpen(aiEnemy);
        }
        else if (player != null) {
            aiEnemy.OnPlayerEnter(player);
        }
    }
}
