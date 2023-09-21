using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseTest : MonoBehaviour {

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.W)) PlayWoof();
        if (Input.GetKeyDown(KeyCode.G)) PlayGlass();

    }

    void PlayWoof() {

        AkSoundEngine.PostEvent("Manager_Test_Woof", gameObject);
            
    }

    void PlayGlass() {

        AkSoundEngine.PostEvent("Manager_Test_Glass", gameObject);

    }

}
