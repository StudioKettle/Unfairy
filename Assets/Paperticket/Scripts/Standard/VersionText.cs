using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Paperticket {

    [RequireComponent(typeof(TextMeshPro))]
    public class VersionText : MonoBehaviour {

        [SerializeField] TextMeshPro textMeshPro;

        void OnEnable() {
            textMeshPro = textMeshPro ?? GetComponent<TextMeshPro>();
            textMeshPro.text = Application.version;    
        }

    }
}
