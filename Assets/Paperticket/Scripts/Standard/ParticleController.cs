using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleController : MonoBehaviour {

    [SerializeField] MinMaxGradientConstructor[] _ColorOptions;

    ParticleSystem _ParticleSystem;
    ParticleSystem.MainModule mainModule;

    ParticleSystem.MinMaxCurve gravityModifierDefault;
    ParticleSystem.MinMaxGradient startColorDefault;

    

    void Awake() {
        _ParticleSystem = GetComponent<ParticleSystem>();
        mainModule = _ParticleSystem.main;

        SaveInitialValues();
    }

    void SaveInitialValues() {
        gravityModifierDefault = mainModule.gravityModifier;
        startColorDefault = mainModule.startColor;
    }


    public void ChangeGravity(float value) {
        mainModule.gravityModifier = value;
    }
    public void ResetGravity() {
        mainModule.gravityModifier = gravityModifierDefault;
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


}


[System.Serializable]
public class MinMaxGradientConstructor{

    public ParticleSystemGradientMode mode;
    public Color constantColor;
    public Color colorMin;
    public Color colorMax;
    public Gradient gradient;
    public Gradient gradientMin;
    public Gradient gradientMax;

}
