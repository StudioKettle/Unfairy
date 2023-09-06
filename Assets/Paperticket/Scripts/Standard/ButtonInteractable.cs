using Paperticket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonInteractable : MonoBehaviour {

    protected enum RendererType { Generic, Mesh, Sprite, Image } 


    [Space(10)]
    [SerializeField] protected bool debugging;

    [Header("GRAPHICS")]

    [SerializeField] protected Renderer genRenderer;
    [SerializeField] protected Image imageRenderer;
    protected XRBaseInteractable baseInteractable;
    protected SpriteRenderer spriteRend;
    protected MeshRenderer meshRend;
    [Space(10)]
    [SerializeField] protected float fadeTime = 0.25f;
    [SerializeField] protected Color defaultColor = new Color(1, 1, 1, 0.5f);
    [SerializeField] protected Color hoveredColor = Color.white;
    [SerializeField] protected Color selectedColor = Color.grey;
    [Space(10)]
    [SerializeField] protected Color usedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("CONTROLS")]

    [Space(10)]
    [SerializeField] protected TimeScale timeScale = 0;
    [SerializeField] protected bool oneUse = false;
    protected bool used = false;
    [Space(10)]
    [SerializeField] protected bool locked = false;
    [Space(10)]
    [SerializeField] protected UnityEvent2 selectEvent;
    protected RendererType rendererType = RendererType.Generic;

    protected Coroutine fadingCoroutine = null;

    void Awake() {

        baseInteractable = baseInteractable ?? GetComponent<XRBaseInteractable>() ?? GetComponentInChildren<XRBaseInteractable>(true);
        if (!baseInteractable) {
            if (debugging) Debug.LogError("[ButtonInteractable] ERROR -> No XRSimpleInteractable found on or beneath this button! Please add one. Disabling object.");
            gameObject.SetActive(false);
        }


        genRenderer = genRenderer != null ? genRenderer : 
                       GetComponentInChildren<Renderer>() != null ? GetComponentInChildren<Renderer>() :
                       GetComponentInChildren<Renderer>(true) != null ? GetComponentInChildren<Renderer>(true) :
                       null;

        if (!genRenderer) {
            if (!imageRenderer) {
                Debug.LogError("[ButtonInteractable] ERROR -> No renderer or image found on or beneath this button! Please add one. Disabling object.");
                gameObject.SetActive(false);
            } else {
                rendererType = RendererType.Image;
            }
        } else {
            if (genRenderer as SpriteRenderer != null) {
                rendererType = RendererType.Sprite;
                spriteRend = genRenderer as SpriteRenderer;

            } else if (genRenderer as MeshRenderer != null) {
                rendererType = RendererType.Mesh;
                meshRend = genRenderer as MeshRenderer;
            }
            
            if (spriteRend == null && meshRend == null) {
                rendererType = RendererType.Generic;                
                Debug.LogError("[ButtonInteractable] ERROR -> No appropriate renderer found on or beneath this button! Please add one. Disabling object.");
                gameObject.SetActive(false);
            }
        }

        

    }


    // Start is called before the first frame update
    void OnEnable() {

        Initialise();
    }

    void OnDisable() {

    }


    protected virtual void Initialise() {

        if (oneUse && used) {

            if (fadingCoroutine != null) StopCoroutine(fadingCoroutine);
            switch (rendererType) {
                case RendererType.Mesh:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(meshRend, usedColor, fadeTime, timeScale));
                    break;
                case RendererType.Sprite:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(spriteRend, usedColor, fadeTime, timeScale));
                    break;
                case RendererType.Image:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(imageRenderer, usedColor, fadeTime, timeScale));
                    break;
                case RendererType.Generic:
                default:
                    Debug.LogError("[ButtonInteractable] ERROR -> Bad RendererType passed! Cancelling...");
                    break;
            }

        } else {
            Invoke("HoverOff", 0.01f);
        }

    }

    public virtual void HoverOn() { HoverOn(null); }
    public virtual void HoverOn ( XRBaseInteractor interactor ) {
        if (locked || (oneUse && used)) return;

        if (fadingCoroutine != null) StopCoroutine(fadingCoroutine);
        switch (rendererType) {
            case RendererType.Mesh:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(meshRend, hoveredColor, fadeTime, timeScale));
                    break;
            case RendererType.Sprite:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(spriteRend, hoveredColor, fadeTime, timeScale));
                break;
            case RendererType.Image:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(imageRenderer, hoveredColor, fadeTime, timeScale));
                    break;
            case RendererType.Generic:
            default:
                Debug.LogError("[ButtonInteractable] ERROR -> Bad RendererType passed! Cancelling...");
                break;
        }

        if (debugging) Debug.Log("[ButtonInteractable] Hovering on!");
    }

    public virtual void HoverOff() { HoverOff(null); }
    public virtual void HoverOff( XRBaseInteractor interactor ) {
        if (locked || (oneUse && used)) return;
        
        if (fadingCoroutine != null) StopCoroutine(fadingCoroutine);
        switch (rendererType) {
            case RendererType.Mesh:
                fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(meshRend, defaultColor, fadeTime, timeScale));
                break;
            case RendererType.Sprite:
                fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(spriteRend, defaultColor, fadeTime, timeScale));
                break;
            case RendererType.Image:
                fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(imageRenderer, defaultColor, fadeTime, timeScale));
                break;
            case RendererType.Generic:
            default:
                Debug.LogError("[ButtonInteractable] ERROR -> Bad RendererType passed! Cancelling...");
                break;
        }


        if (debugging) Debug.Log("[ButtonInteractable] Hovering off!");
    }

    public virtual void Select() { Select(null); }
    public virtual void Select( XRBaseInteractor interactor ) {
        if (locked || (oneUse && used)) return;
        
        if (fadingCoroutine != null) StopCoroutine(fadingCoroutine);
        switch (rendererType) {
            case RendererType.Mesh:
                fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(meshRend, selectedColor, fadeTime, timeScale));
                break;
            case RendererType.Sprite:
                fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(spriteRend, selectedColor, fadeTime, timeScale));
                break;
            case RendererType.Image:
                fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(imageRenderer, selectedColor, fadeTime, timeScale));
                break;
            case RendererType.Generic:
            default:
                Debug.LogError("[ButtonInteractable] ERROR -> Bad RendererType passed! Cancelling...");
                break;
        }

        used = true;
        if (selectEvent != null) selectEvent.Invoke();

        if (debugging) Debug.Log("[ButtonInteractable] Selected!");
    }

    public virtual void ToggleButton( bool toggle ) {
        locked = toggle;
    }

    public virtual void FadeButton (bool fadeIn, float duration ) {

        if (fadeIn) {

            if (fadingCoroutine != null) StopCoroutine(fadingCoroutine);
            switch (rendererType) {
                case RendererType.Mesh:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(meshRend, defaultColor, duration, timeScale));
                    break;
                case RendererType.Sprite:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(spriteRend, defaultColor, duration, timeScale));
                    break;
                case RendererType.Image:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(imageRenderer, defaultColor, duration, timeScale));
                    break;
                case RendererType.Generic:
                default:
                    Debug.LogError("[ButtonInteractable] ERROR -> Bad RendererType passed! Cancelling...");
                    break;
            }

            locked = false;

        } else {

            if (fadingCoroutine != null) StopCoroutine(fadingCoroutine);
            switch (rendererType) {
                case RendererType.Mesh:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeAlphaTo(meshRend, 0, duration, timeScale));
                    break;
                case RendererType.Sprite:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeAlphaTo(spriteRend, 0, duration, timeScale));
                    break;
                case RendererType.Image:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeAlphaTo(imageRenderer, 0, duration, timeScale));
                    break;
                case RendererType.Generic:
                default:
                    Debug.LogError("[ButtonInteractable] ERROR -> Bad RendererType passed! Cancelling...");
                    break;
            }

            locked = true;

        }

    }


    #region UNUSED


    public virtual void Deselect() { Deselect(null); }
    public virtual void Deselect( XRBaseInteractor interactor ) {
        if (locked || (oneUse && used)) return;
        
        if (oneUse) {
            if (fadingCoroutine != null) StopCoroutine(fadingCoroutine);
            switch (rendererType) {
                case RendererType.Mesh:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(meshRend, usedColor, fadeTime, timeScale));
                    break;
                case RendererType.Sprite:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(spriteRend, usedColor, fadeTime, timeScale));
                    break;
                case RendererType.Image:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(imageRenderer, usedColor, fadeTime, timeScale));
                    break;
                case RendererType.Generic:
                default:
                    Debug.LogError("[ButtonInteractable] ERROR -> Bad RendererType passed! Cancelling...");
                    break;
            }
        } else {
            if (fadingCoroutine != null) StopCoroutine(fadingCoroutine);
            switch (rendererType) {
                case RendererType.Mesh:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(meshRend, defaultColor, fadeTime, timeScale));
                    break;
                case RendererType.Sprite:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(spriteRend, defaultColor, fadeTime, timeScale));
                    break;
                case RendererType.Image:
                    fadingCoroutine = StartCoroutine(PTUtilities.instance.FadeColorTo(imageRenderer, defaultColor, fadeTime, timeScale));
                    break;
                case RendererType.Generic:
                default:
                    Debug.LogError("[ButtonInteractable] ERROR -> Bad RendererType passed! Cancelling...");
                    break;
            }
        }

        if (debugging) Debug.Log("[ButtonInteractable] Deselected!");
    }

    #endregion

}
