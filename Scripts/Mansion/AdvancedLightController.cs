using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;

public class LightData {
    public Light light {get; private set;}
    public float originalIntensity {get; private set;}
    public Coroutine flickerCoroutine { get; private set;}

    // Renderer and MaterialPropertyBlock are used to get the original emission color of the light
    public Renderer renderer {get; private set;}
    public MaterialPropertyBlock propBlock {get; private set;}
    public Color originalEmissionColor {get; private set;}

    public LightData(Light light) {
        this.light = light;
        originalIntensity = light.intensity;

        renderer = light.GetComponentInParent<Renderer>();
        if(renderer != null)
        {
            propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);
            originalEmissionColor = renderer.material.GetColor("_EmissionColor");
        }
    }

    public void SetFlickerCoroutine() {
        if(flickerCoroutine != null)
            return;

        flickerCoroutine = AdvancedLightController.Instance.CreateFlicker(this);
    }

    public void StopFlickerCoroutine() {
        if(flickerCoroutine == null)
            return;

        AdvancedLightController.Instance.StopFlicker(flickerCoroutine);
        flickerCoroutine = null;
    }

    public void SetLightState(bool on, bool flicker = true) {
        if (on) {
            light.intensity = originalIntensity;
            if (flicker)
                SetFlickerCoroutine();
        }
        else {
            light.intensity = 0;

            if(renderer != null)
            {
                propBlock.SetColor("_EmissionColor", Color.black);
                renderer.SetPropertyBlock(propBlock);
            }

            StopFlickerCoroutine();
        }
    }
}

public class AdvancedLightController : MonoBehaviour
{
    [Header("Flicker Settings")]
    [SerializeField] private float minTimeBetweenFlickerWaves = 5f;
    [SerializeField] private float maxTimeBetweenFlickerWaves = 60f;
    [SerializeField] private float minNumberOfFlickers = 1f;
    [SerializeField] private float maxNumberOfFlickers = 5f;
    [SerializeField] private float minFlickerTimeLength = 0.1f;
    [SerializeField] private float maxFlickerTimeLength = 1f;
    [SerializeField] private float minIntensity = 0.0f;
    [SerializeField] private float maxIntensity = 1.0f;

    [Header("Blackout Settings")]
    [SerializeField] private float blackoutDuration = 5.0f; // Time for total blackout
    private bool inBlackout = false;

    [Header("Player Lights")]
    [SerializeField] private Light headLight;
    [SerializeField] private Light lanternLight;
    [SerializeField] private ParticleSystem lanternParticles;
    [SerializeField] private Light muzzleFlashLight;

    // Actions
    public static Action OnTotalDarkness;

    // Helper variables
    Dictionary<Light, LightData> lightData;

    // Light Components
    private List<Light> environmentLights;

    public static AdvancedLightController Instance { get; private set; }

    private void Awake() {
        Instance = this;
        environmentLights = new List<Light>(FindObjectsOfType<Light>());

        environmentLights.Remove(headLight);
        environmentLights.Remove(lanternLight);
        environmentLights.Remove(muzzleFlashLight);

        lightData = new Dictionary<Light, LightData>();
        InitializeLightData();
    }

    void Start() {
        SetLightsState(true);
    }

    void InitializeLightData()
    {
        foreach (Light light in environmentLights)
            lightData.Add(light, new LightData(light));
        
        lightData.Add(headLight, new LightData(headLight));
        lightData.Add(lanternLight, new LightData(lanternLight));
    }

    // Helpers
    public Coroutine CreateFlicker(LightData data) {
        return StartCoroutine(Flicker(data));
    }

    public void StopFlicker(Coroutine flickerCoroutine) {
        StopCoroutine(flickerCoroutine);
    }

    IEnumerator Flicker(LightData data)
    {
        while (true)
        {
            // Generate the randomness of flickers
            int numberOfFlickers = Random.Range((int)minNumberOfFlickers, (int)maxNumberOfFlickers);
            float timeBetweenFlickers = Random.Range(minTimeBetweenFlickerWaves, maxTimeBetweenFlickerWaves);
            float flickerTimeLength = Random.Range(minFlickerTimeLength, maxFlickerTimeLength);
            float intensity = Random.Range(minIntensity, maxIntensity);
            
            for (int i = 0; i < numberOfFlickers; i++)
            {
                UpdateIntensity(data, intensity);
                yield return new WaitForSeconds(flickerTimeLength);
                UpdateIntensity(data, data.originalIntensity);
                yield return new WaitForSeconds(flickerTimeLength);
            }

            yield return new WaitForSeconds(timeBetweenFlickers);
        }
    }

    void UpdateIntensity(LightData data, float intensity) {
        Light light = data.light;
        Renderer renderer = data.renderer;
        
        light.intensity = intensity;

        // Emission Color
        if(renderer == null)
            return;
        
        MaterialPropertyBlock propBlock = data.propBlock;
        Color originalEmissionColor = data.originalEmissionColor;

        float emissionIntensity = intensity / data.originalIntensity;
        propBlock.SetColor("_EmissionColor", originalEmissionColor * emissionIntensity);
        renderer.SetPropertyBlock(propBlock);
    }


    void SetLightsState(bool on) {
        foreach (Light light in environmentLights)
            lightData[light].SetLightState(on);

        // Update lantern light
        lightData[headLight].SetLightState(on, false);
        lightData[lanternLight].SetLightState(on, false);
        lanternParticles.gameObject.SetActive(on);

        inBlackout = !on;
    }


    public void StartBlackout()
    {
        StartCoroutine(Blackout());
    }

    IEnumerator Blackout()
    {
        SetLightsState(false);

        OnTotalDarkness?.Invoke();

        yield return new WaitForSeconds(blackoutDuration);

        SetLightsState(true);
    }

}
