using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Focus UI")]
    [SerializeField] private GameObject focusUI;
    [SerializeField] private TextMeshProUGUI focusText;

    [Header("Health Bar / Stamina Bar")]
    [SerializeField] float tInterp = 0.05f;
    [SerializeField] private Material healthBarMaterial;
    private float lastHealthBarT = 1;
    [SerializeField] private Material staminaBarMaterial;
    private float lastStaminaBarT = 1;

    [Header("Ammo UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI magText;

    FirstPersonController player;
    Gun gun;
    public static UIManager Instance { get; private set; }
    private void Start() {
        Instance = this;
        player = FindObjectOfType<FirstPersonController>();
        gun = FindObjectOfType<Gun>();

        ClearFocusText();

        Gun.OnFire += OnFireTriggered;
        Gun.OnReload += OnReloadTriggered;
        Gun.OnPickupAmmo += OnPickupAmmoTriggered;

        InitializeUI();
    }

    void InitializeUI() {
        UpdateHealthBar();
        UpdateStaminaBar();
        SetAmmoText();
        SetMagText();
    }

    private void Update() {
        UpdateHealthBar();
        UpdateStaminaBar();
    }

    private void OnDisable() {
        // Needed to prevent version control stuff
        healthBarMaterial.SetFloat("_Health", 1f);
        staminaBarMaterial.SetFloat("_Health", 1f);
    }

    void UpdateHealthBar() {
        float tA = lastHealthBarT;
        float tB = player.currentHealth / player.MaxHealth;
        float t = Mathf.Lerp(tA, tB, tInterp);

        healthBarMaterial.SetFloat("_Health", t);
        lastHealthBarT = t;
    }

    void UpdateStaminaBar() {
        float tA = lastStaminaBarT;
        float tB = player.currentStamina / player.MaxStamina;
        float t = Mathf.Lerp(tA, tB, tInterp);

        staminaBarMaterial.SetFloat("_Health", t);
        lastStaminaBarT = t;
    }

    public void SetFocusText(string text) {
        focusUI.SetActive(true);
        focusText.text = text;
    }

    public void ClearFocusText() {
        focusUI.SetActive(false);
        focusText.text = "";
    }

    // Ammo UI

    void OnReloadTriggered() {
        SetAmmoText();
        SetMagText();
    }

    void OnFireTriggered(RaycastHit hit) {
        SetAmmoText();
        SetMagText();
    }

    void OnPickupAmmoTriggered() {
        SetAmmoText();
        SetMagText();
    }

    public void SetMagText() {
        int currentMagSize = gun.currentMagSize;
        int maxMagSize = gun.MaxMagSize;

        magText.text = currentMagSize.ToString() + " / " + maxMagSize.ToString();
    }

    public void SetAmmoText() {
        int currentAmmo = gun.currentAmmo;
        int maxAmmo = gun.MaxAmmo;

        ammoText.text = currentAmmo.ToString() + " / " + maxAmmo.ToString();
    }
}
