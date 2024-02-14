using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleController : MonoBehaviour {

    [SerializeField] MinMaxGradientConstructor[] _ColorOptions;
    [SerializeField] bool debugging = false;

    ParticleSystem _ParticleSystem;
    ParticleSystem.MainModule mainModule;
    ParticleSystem.EmissionModule emissionModule;
    ParticleSystem.NoiseModule noiseModule;

    ParticleSystem.MinMaxCurve gravityModifierDefault;
    ParticleSystem.MinMaxGradient startColorDefault;
    ParticleSystem.MinMaxCurve emissionOverTimeDefault;
    ParticleSystem.MinMaxCurve noiseStrengthDefault;
    float simulationSpeedDefault;




    void Awake() {
        _ParticleSystem = GetComponent<ParticleSystem>();
        mainModule = _ParticleSystem.main;
        noiseModule = _ParticleSystem.noise;
        emissionModule = _ParticleSystem.emission;

        SaveInitialValues();
    }

    void SaveInitialValues() {
        gravityModifierDefault = mainModule.gravityModifier;
        startColorDefault = mainModule.startColor;
        noiseStrengthDefault = noiseModule.strength;
        emissionOverTimeDefault = emissionModule.rateOverTime;
    }


    public void ChangeGravity(float value) {
        mainModule.gravityModifier = value;
    }
    public void ResetGravity() {
        mainModule.gravityModifier = gravityModifierDefault;
    }

    public void ChangeSimulationSpeed(float value) {
        mainModule.simulationSpeed = value;
    }
    public void ResetSimulationSpeed() {
        mainModule.simulationSpeed = simulationSpeedDefault;
    }

    public void ChangeEmissionOverTime(float value) {
        ParticleSystem.MinMaxCurve minMaxCurve = emissionModule.rateOverTime;
        minMaxCurve.mode = ParticleSystemCurveMode.Constant;
        minMaxCurve.constant = value;

        emissionModule.rateOverTime = minMaxCurve;
    }
    public void ResetEmissionOverTime() {
        emissionModule.rateOverTime = emissionOverTimeDefault;
    }



    public void ChangeNoiseStrength(float value) {
        if (!noiseModule.enabled) {Debug.LogError("[ParticleController] No noise module found! Ignoring."); return; }

        noiseModule.strength = value;
    }
    public void ResetNoiseStrength() {
        noiseModule.strength = noiseStrengthDefault;
    }


    public void ChangeStartColor( int minMaxGradientIndex ) {

        ParticleSystem.MinMaxGradient minMaxGradient = new ParticleSystem.MinMaxGradient();
        MinMaxGradientConstructor constructor = _ColorOptions[minMaxGradientIndex];

        minMaxGradient.mode = constructor.mode;
        switch (constructor.mode) {
            case ParticleSystemGradientMode.Color:                
                minMaxGradient = constructor.constantColor;
                break;
            case ParticleSystemGradientMode.Gradient:
            case ParticleSystemGradientMode.RandomColor:
                minMaxGradient = constructor.gradient;
                break;
            case ParticleSystemGradientMode.TwoColors:
                minMaxGradient = new ParticleSystem.MinMaxGradient(constructor.colorMin, constructor.colorMax);
                break;
            case ParticleSystemGradientMode.TwoGradients:
                minMaxGradient = new ParticleSystem.MinMaxGradient(constructor.gradientMin, constructor.gradientMax);
                break;
            default:
                Debug.LogError("[ParticleController] ERROR -> Bad particle system mode!");
                break;
        }

        mainModule.startColor = minMaxGradient;

    }
    public void ResetStartColor() {
        mainModule.startColor = startColorDefault;
    }




    public void FadeSimulationSpeed(float targetSpeed, float duration, TimeScale timeScale) {
        if (fadingSimulationSpeedCo != null) StopCoroutine(fadingSimulationSpeedCo);
        fadingSimulationSpeedCo = StartCoroutine(FadingSimulationSpeed(targetSpeed, duration, timeScale));
    }
    Coroutine fadingSimulationSpeedCo;
    IEnumerator FadingSimulationSpeed(float targetSpeed, float duration, TimeScale timeScale) {

        if (mainModule.simulationSpeed != targetSpeed) {

            if (debugging) Debug.Log("[ParticleController] Fading simulation speed to " + targetSpeed);


            float speed = mainModule.simulationSpeed;
            for (float t = 0.0f; t < 1.0f; t += (timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime) / duration) {
                mainModule.simulationSpeed = Mathf.Lerp(speed, targetSpeed, t); // animCurve.Evaluate(t));
                yield return null;
            }
            mainModule.simulationSpeed = targetSpeed;

            yield return null;


            if (debugging) Debug.Log("[ParticleController] Successfully faded simulation speed to " + targetSpeed);

        } else {
            if (debugging) Debug.Log("[ParticleController] Already at simulation speed " + targetSpeed + ", ignoring.");
        }
    }


    public void FadeEmissionOverTime(float targetRate, float duration, TimeScale timeScale) {
        if (fadingEmissionOverTimeCo != null) StopCoroutine(fadingEmissionOverTimeCo);
        fadingEmissionOverTimeCo = StartCoroutine(FadingEmissionOverTime(targetRate, duration, timeScale));
    }
    Coroutine fadingEmissionOverTimeCo;
    IEnumerator FadingEmissionOverTime(float targetRate, float duration, TimeScale timeScale) {

        if (emissionModule.rateOverTime.constant != targetRate) {

            ParticleSystem.MinMaxCurve minMaxCurve = emissionModule.rateOverTime;
            minMaxCurve.mode = ParticleSystemCurveMode.Constant;
            emissionModule.rateOverTime = minMaxCurve;


            if (debugging) Debug.Log("[ParticleController] Fading emission rate to " + targetRate);


            float rate = emissionModule.rateOverTime.constant;
            for (float t = 0.0f; t < 1.0f; t += (timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime) / duration) {
                minMaxCurve.constant = Mathf.Lerp(rate, targetRate, t); // animCurve.Evaluate(t));
                emissionModule.rateOverTime = minMaxCurve;
                yield return null;
            }
            minMaxCurve.constant = targetRate;
            emissionModule.rateOverTime = minMaxCurve;
            yield return null;

            if (debugging) Debug.Log("[ParticleController] Successfully emission rate to " + targetRate);

        } else {
            if (debugging) Debug.Log("[ParticleController] Already at emission rate " + targetRate + ", ignoring.");
        }
    }


}


[System.Serializable]
public class MinMaxCurveConstructor{

    public ParticleSystemCurveMode mode;
    public Color constant;
    public Color valueMin;
    public Color valueMax;
    public AnimationCurve curve;
    public AnimationCurve curveMin;
    public AnimationCurve curveMax;

}

[System.Serializable]
public class MinMaxGradientConstructor {

    public ParticleSystemGradientMode mode;
    public Color constantColor;
    public Color colorMin;
    public Color colorMax;
    public Gradient gradient;
    public Gradient gradientMin;
    public Gradient gradientMax;

}
