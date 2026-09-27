using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class CabinetTrigger : Interactable
{
    private bool isOpen = false;
    private bool canBeInteractedWith = true;
    
    private Animator anim;

    [Header("Audio Settings")]
    [SerializeField] private StudioEventEmitter openEmitter;
    [SerializeField] private StudioEventEmitter closeEmitter;

    public override void Awake() {
        base.Awake();

        anim = GetComponentInChildren<Animator>();
    }

    public override void OnFocus()
    {
        FirstPersonController player = FirstPersonController.Instance;
        UIManager.Instance.SetFocusText($"Press <b>{player.InteractKey.ToString()}</b> to open");
    }

    void HandleInteractionSounds() {
        if (canBeInteractedWith)
        {
            if(!isOpen)
            {
                // Play the open sound
                openEmitter?.Play();
            }
            else
            {
                // Play the close sound
                closeEmitter?.Play();
            }
        }
        else
        {
            if(isOpen)
            {
                // cabinet is broken, play the broken sound

            }
            else
            {
                // door is locked or something

            }
        }
    }

    public override void OnInteract()
    {
        HandleInteractionSounds();

        if(!canBeInteractedWith)
            return;

        UpdateState(!isOpen);
    }

    public override void OnLoseFocus()
    {
        UIManager.Instance.ClearFocusText();
    }

    void UpdateState(bool state)
    {
        isOpen = state;

        anim.SetBool("isOpen", isOpen);
    }
}
