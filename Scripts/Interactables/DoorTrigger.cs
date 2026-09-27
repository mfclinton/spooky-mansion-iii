using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class DoorTrigger : Interactable
{
    private bool isOpen = false;
    bool canBeInteractedWith = true;
    
    [Header("Door Settings")]
    [SerializeField] private string requiredKeyName;
    [SerializeField] private float brokenDoorTime = 5f;

    [Header("Audio Settings")]
    [SerializeField] private StudioEventEmitter openEmitter;
    [SerializeField] private StudioEventEmitter closeEmitter;
    [SerializeField] private StudioEventEmitter lockedEmitter;
    [SerializeField] private StudioEventEmitter unlockEmitter;
    [SerializeField] private StudioEventEmitter forcedOpenEmitter;

    Animator anim;
    public override void Awake() {
        base.Awake();

        anim = GetComponentInChildren<Animator>();
        canBeInteractedWith = requiredKeyName == "";
    }

    public override void OnFocus()
    {
        FirstPersonController player = FirstPersonController.Instance;

        bool isLocked = requiredKeyName != "";
        bool hasKey = player.HasKey(requiredKeyName);

        if(isLocked)
        {
            if(hasKey)
                UIManager.Instance.SetFocusText($"Press <b>{player.InteractKey.ToString()}</b> to unlock");
            else
                UIManager.Instance.SetFocusText($"Door is locked and requires a key");
        }
        else
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
                // door is broken, play the broken sound

            }
            else
            {
                // door is locked
                lockedEmitter?.Play();
            }
        }
    }

    public override void OnInteract()
    {
        FirstPersonController player = FirstPersonController.Instance;

        HandleLockedDoor();

        HandleInteractionSounds();

        if(!canBeInteractedWith)
            return;

        Vector3 playerTransformDirection = player.transform.forward;
        UpdateDoorState(!isOpen, playerTransformDirection);
    }

    void HandleLockedDoor() {
        FirstPersonController player = FirstPersonController.Instance;

        if(!canBeInteractedWith && requiredKeyName != "" && player.UseKey(requiredKeyName))
        {
            // Play the locked sound
            unlockEmitter?.Play();
            canBeInteractedWith = true;
        }
    }

    public override void OnLoseFocus()
    {
        UIManager.Instance.ClearFocusText();
    }

    void HandleAIInteractionSounds(AIEnemy aiEnemy) {
        if(aiEnemy.enemyState == AIEnemy.EnemyState.Pursuing)
        {
            // Door slammed open
            if(!isOpen)
            {
                // Play the open sound
                forcedOpenEmitter?.Play();
            }
        }
        else
        {
            // Door regularly opened
            if(!isOpen)
            {
                // Play the open sound
                openEmitter?.Play();
            }
        }
    }

    public void ForceDoorOpen(AIEnemy aiEnemy) {
        // If it's an AIEnemy, force the door open
        if (aiEnemy != null && !isOpen)
        {
            HandleAIInteractionSounds(aiEnemy);

            Vector3 otherTransformDirection = aiEnemy.transform.forward;
            UpdateDoorState(true, otherTransformDirection);

            StartCoroutine(DisableDoor());
        }
    }

    void UpdateDoorState(bool state, Vector3 interactorTransformDirection = default(Vector3))
    {
        isOpen = state;

        Vector3 doorTransformDirection = transform.forward;
        float dotProduct = Vector3.Dot(doorTransformDirection, interactorTransformDirection);

        anim.SetFloat("dotProduct", dotProduct);
        anim.SetBool("isOpen", isOpen);
    }

    // IEnumerator that disables the door after a certain amount of time
    IEnumerator DisableDoor()
    {
        canBeInteractedWith = false;
        yield return new WaitForSeconds(brokenDoorTime);
        UpdateDoorState(false);
        canBeInteractedWith = true;
    }
}
