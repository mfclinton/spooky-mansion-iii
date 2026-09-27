using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(AudioSource))]
public class HiddenWallTrigger : Interactable
{
    [Header("Hidden Wall Trigger Settings")]
    [SerializeField] private HiddenWall hiddenWall;

    // Attributes
    private bool isOpen = false;
    private bool canBeInteractedWith = true;
    
    // Components
    AudioSource audioSource;
    private Animator anim;

    [Header("Audio Settings")]
    [SerializeField] private StudioEventEmitter openLeverEmitter;
    [SerializeField] private StudioEventEmitter closeLeverEmitter;
    [SerializeField] private StudioEventEmitter openDoorEmitter;
    [SerializeField] private StudioEventEmitter closeDoorEmitter;

    public override void Awake() {
        base.Awake();

        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    public override void OnFocus()
    {
        FirstPersonController player = FirstPersonController.Instance;
        UIManager.Instance.SetFocusText($"Press <b>{player.InteractKey.ToString()}</b> to activate");
    }

    void HandleInteractionSounds() {
        if (canBeInteractedWith)
        {
            if(!isOpen)
            {
                // Play the open sound
                openLeverEmitter?.Play();
                openDoorEmitter?.Play();
            }
            else
            {
                // Play the close sound
                closeLeverEmitter?.Play();
                closeDoorEmitter?.Play();
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
        hiddenWall.SetState(isOpen);
    }
}
