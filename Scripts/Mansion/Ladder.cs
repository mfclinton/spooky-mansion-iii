using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Ladder : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player != null) {
            player.isClimbingLadder = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        FirstPersonController player = other.GetComponent<FirstPersonController>();
        if (player != null) {
            player.isClimbingLadder = false;
        }
    }
}
