using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoTrigger : Interactable
{
    [Header("Ammo Settings")]
    [SerializeField] private int ammoAmount = 10;
    
    public override void Awake() {
        base.Awake();


    }

    public override void OnFocus()
    {
        FirstPersonController player = FirstPersonController.Instance;
        UIManager.Instance.SetFocusText($"Press <b>{player.InteractKey.ToString()}</b> to pick up ammo");
    }

    public override void OnInteract()
    {
        FirstPersonController player = FirstPersonController.Instance;
        Gun gun = player.gun;

        gun.PickupAmmo(ammoAmount);
        Destroy(gameObject);
        UIManager.Instance.ClearFocusText();
    }

    public override void OnLoseFocus()
    {
        UIManager.Instance.ClearFocusText();
    }
}
