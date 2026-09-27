using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyTrigger : Interactable
{
    [SerializeField] private string keyName;

    public override void Awake() {
        base.Awake();


    }

    public override void OnFocus()
    {
        FirstPersonController player = FirstPersonController.Instance;
        UIManager.Instance.SetFocusText($"Press <b>{player.InteractKey.ToString()}</b> to pick up key");
    }

    public override void OnInteract()
    {
        FirstPersonController player = FirstPersonController.Instance;

        player.PickupKey(keyName);

        Destroy(gameObject);
        UIManager.Instance.ClearFocusText();
    }

    public override void OnLoseFocus()
    {
        UIManager.Instance.ClearFocusText();
    }
}
