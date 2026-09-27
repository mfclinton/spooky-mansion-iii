using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractable : Interactable
{

    public override void OnFocus()
    {
        print($"TestInteractable.OnFocus() {gameObject.name}");
    }

    public override void OnInteract()
    {
        print($"TestInteractable.OnInteract() {gameObject.name}");
    }

    public override void OnLoseFocus()
    {
        print($"TestInteractable.OnLoseFocus() {gameObject.name}");
    }
}
