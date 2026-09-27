using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(Animator))]
public class HiddenWall : MonoBehaviour
{
    // Components
    AudioSource audioSource;
    Animator anim;


    private void Awake() {
        audioSource = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
    }

    public void SetState(bool isOpen) {
        PlaySoundEffect(isOpen);
        anim.SetBool("isOpen", isOpen);
    }

    void PlaySoundEffect(bool isOpen) {
        if(isOpen) {
            // Play the open sound

        } else {
            // Play the close sound

        }
    }
}
