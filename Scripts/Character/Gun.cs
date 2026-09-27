using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FMODUnity;
using UnityEngine.SceneManagement;

public class Gun : MonoBehaviour
{
    private bool IsInAction => isSwapping || isShooting || isReloading || isAiming;
    private bool ShouldFire => gunIsEquipped && canFire && !IsInAction && isAimedIn && Input.GetKey(fireKey) && currentMagSize > 0;
    private bool ShouldReload => gunIsEquipped && canReload && !IsInAction && ((Input.GetKeyDown(reloadKey) && currentMagSize < maxMagSize) || currentMagSize == 0) && currentAmmo > 0;
    private bool TryingToAimIn => gunIsEquipped && Input.GetKey(aimKey);
    private bool ShouldToggleAim => gunIsEquipped && !IsInAction && (TryingToAimIn != isAimedIn);
    private bool ShouldSwapWeapon => canSwap && !IsInAction && Input.GetKeyDown(swapKey);

    [Header("Functional Settings")]
    [SerializeField] private bool canFire = true;
    [SerializeField] private bool canReload = true;
    [SerializeField] private bool canSwap = true;
    
    [Header("Gun Attributes")]
    [SerializeField] private int damage = 20;
    [SerializeField] private float range = 100f;

    [Header("Gun Ammo Stats")]
    [SerializeField] private int maxAmmo = 40;
    public int MaxAmmo => maxAmmo;
    public int currentAmmo { get; private set;}
    [SerializeField] private int maxMagSize = 10;
    public int MaxMagSize => maxMagSize;
    public int currentMagSize { get; private set;}
    [SerializeField] private int startingAmmo = 15;

    [Header("Controls")]
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode reloadKey = KeyCode.R;
    [SerializeField] private KeyCode aimKey = KeyCode.Mouse1;
    [SerializeField] private KeyCode swapKey = KeyCode.Q;

    [Header("Audio Settings")]
    [SerializeField] private StudioEventEmitter fireEmitter;
    [SerializeField] private StudioEventEmitter reloadEmitter;

    [Header("Bullet Impact Effects")]
    [SerializeField] private ParticleSystem bulletImpactEffectStone;
    [SerializeField] private ParticleSystem bulletImpactEffectWood;
    [SerializeField] private ParticleSystem bulletImpactEffectFlesh;

    [Header("Light Settings")]
    [SerializeField] private ParticleSystem muzzleFlashParticles;
    [SerializeField] private Light lanternLight;
    [SerializeField] private float lanternAwayIntMultiplier = 0.5f;
    [SerializeField] private float lanternAwayRangeMultiplier = 0.5f;
    [SerializeField] private float lanternIntTransitionTime = 3f;
    private float lanternOutIntensity;
    private float lanternOutRange;
    private Coroutine lanternCoroutine;

    // Gun State
    private bool isShooting = false;
    private bool isReloading = false;
    private bool isAiming = false;
    private bool isAimedIn = false;
    public bool IsAimedIn => isAimedIn;
    private bool gunIsEquipped = false;
    private bool isSwapping = false;

    // Actions
    public static Action<RaycastHit> OnFire;
    public static Action OnReload;
    public static Action OnPickupAmmo;

    // Components
    private Camera cam;
    private Animator animator;

    private void Awake() {
        cam = GetComponentInParent<Camera>();
        animator = GetComponent<Animator>();

        currentAmmo = startingAmmo;
        currentMagSize = maxMagSize;

        lanternOutIntensity = lanternLight.intensity;
        lanternOutRange = lanternLight.range;
    }

    // Subscribe to sceneUnloaded when enabled
    private void OnEnable() {
        ResetActions();
    }

    // Unsubscribe when disabled
    private void OnDisable() {
        ResetActions();
    }

    private void ResetActions() {
        OnFire = null;
        OnReload = null;
        OnPickupAmmo = null;
    }

    private void Update() {
        HandleFire();
        HandleReload();
        HandleAim();
        HandleSwap();
    }

    private void HandleFire() {
        if (!ShouldFire)
            return;
        print("Firing");
        isShooting = true;
        animator.SetTrigger("fireTriggered");
    }

    private void HandleReload() {
        if (!ShouldReload)
            return;
        print("Reloading");
        
        reloadEmitter?.Play();
        reloadEmitter?.SetParameter("LeftToLoad", 1);

        isReloading = true;
        animator.SetBool("isReloading", isReloading);
    }

    private void HandleAim() {
        if (!ShouldToggleAim)
            return;
        print("Toggling Aim");
        isAiming = true;
        animator.SetBool("isFocused", !isAimedIn);
    }

    private void HandleSwap() {
        if (!ShouldSwapWeapon)
            return;
        
        print("Swapping Weapon");
        
        isSwapping = true;
        gunIsEquipped = !gunIsEquipped;
        animator.SetBool("gunIsEquipped", gunIsEquipped);

        AimedOut();
        animator.SetBool("isFocused", false);

        HandleUpdateLanternCoroutine();
    }

    private void HandleUpdateLanternCoroutine() {
        if (lanternCoroutine != null)
            StopCoroutine(lanternCoroutine);
        
        lanternCoroutine = StartCoroutine(LanternCoroutine());
    }

    private IEnumerator LanternCoroutine() {
        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        float t = 0;
        float startIntensity = lanternLight.intensity;
        float targetIntensity = gunIsEquipped ? lanternOutIntensity * lanternAwayIntMultiplier : lanternOutIntensity;

        float startRange = lanternLight.range;
        float targetRange = gunIsEquipped ? lanternOutRange * lanternAwayRangeMultiplier : lanternOutRange;

        while (t < 1) {
            t += Time.deltaTime / lanternIntTransitionTime;
            
            lanternLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            lanternLight.range = Mathf.Lerp(startRange, targetRange, t);
            
            yield return wait;
        }
    }

    private (Ray, RaycastHit) RaycastBullet() {

        // Deal with Ammo
        currentMagSize--;

        // Handle the raycast
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
    
        LayerMask mask = ~(LayerMask.GetMask("InteractableTrigger") | LayerMask.GetMask("Player") | LayerMask.GetMask("Ignore Raycast"));

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, range, mask))
        {
            print("Hit: " + hit.transform.name);
            // Check if the raycast hit an enemy check the layer
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                AIEnemy enemy = hit.transform.GetComponentInParent<AIEnemy>();

                // Give the enemy damage
                if (enemy != null)
                    enemy.TakeDamage(damage, gameObject);
            }

            OnFire?.Invoke(hit);
        }

        return (ray, hit);
    }

    public void ReloadAmmo() {
        int ammoNeeded = maxMagSize - currentMagSize;
        int ammoToReload = Mathf.Min(ammoNeeded, currentAmmo);

        currentMagSize += ammoToReload;
        currentAmmo -= ammoToReload;

        OnReload?.Invoke();
    }

    public void SetGunState(bool canFire, bool canReload) {
        this.canFire = canFire;
        this.canReload = canReload;
    }

    public void SetAimState(bool isAimedIn) {
        this.isAimedIn = isAimedIn;
    }

    void CreateBulletImpactEffect(Ray ray, RaycastHit hitInfo) {
        ParticleSystem bulletImpactEffect = bulletImpactEffectWood;

        if(hitInfo.transform != null)
        {
            switch (hitInfo.transform.tag) {
                case "Footsteps/Stone":
                    bulletImpactEffect = bulletImpactEffectStone;
                    break;
                case "Footsteps/Wood":
                    bulletImpactEffect = bulletImpactEffectWood;
                    break;
                case "Enemy":
                    bulletImpactEffect = bulletImpactEffectFlesh;
                    break;
                default:
                    bulletImpactEffect = bulletImpactEffectWood;
                    Debug.Log("No bullet impact effect found for object: " + hitInfo.transform.name);
                    break;
            }
        }

        GameObject spawnedEffect = ObjectPooler.Instance.SpawnFromPool(bulletImpactEffect.gameObject, hitInfo.point, Quaternion.LookRotation(-ray.direction));
        
        // if(hitInfo.transform != null)
        //     spawnedEffect.transform.SetParent(hitInfo.transform);
    }

    public void OnDeath() {
        canFire = false;
        canReload = false;
        canSwap = false;
        animator.SetBool("isDead", true);
        animator.SetBool("gunIsEquipped", false);
    }

    public void PickupAmmo(int ammoAmount) {
        currentAmmo += ammoAmount;
        currentAmmo = Mathf.Min(currentAmmo, maxAmmo);

        OnPickupAmmo?.Invoke();
    }

    // Animation Events
    public void FireBullet() {
        fireEmitter?.Play();
        muzzleFlashParticles?.Play();
        (Ray ray, RaycastHit hitInfo) = RaycastBullet();
        CreateBulletImpactEffect(ray, hitInfo);
    }

    public void EndRecoil() {
        isShooting = false;
    }

    public void EndReload() {
        reloadEmitter.SetParameter("LeftToLoad", 0);
        ReloadAmmo();
        isReloading = false;
        animator.SetBool("isReloading", isReloading);
    }

    public void AimedIn() {
        SetAimState(true);
        isAiming = false;
    }

    public void AimedOut() {
        SetAimState(false);
        isAiming = false;
    }

    public void EndSwap() {
        isSwapping = false;
    }
}
