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

        if (Input.GetKeyDown(KeyCode.Q)) PlayQuack();

    }

    void PlayQuack() {

        AkSoundEngine.PostEvent("Test_Duck_Play", gameObject);
            
    }

    public void PlayRockTestAudio() {


        AkSoundEngine.PostEvent("Rock_Vid_Audio_TEMP", gameObject);

    }

}
