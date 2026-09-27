using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class Interactable : MonoBehaviour
{
    public virtual void Awake() {
        Collider collider = GetComponent<Collider>();
        if(collider.isTrigger)
            gameObject.layer = LayerMask.NameToLayer("InteractableTrigger");
        else
            gameObject.layer = LayerMask.NameToLayer("InteractableSolid");
    }

    public abstract void OnInteract();
    public abstract void OnFocus();
    public abstract void OnLoseFocus();
}
