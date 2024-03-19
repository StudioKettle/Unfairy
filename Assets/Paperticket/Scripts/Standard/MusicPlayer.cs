using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {

    [SerializeField] float delay = 1;
    [SerializeField] AK.Wwise.Switch[] switches = null;
    [Space(5)]
    [SerializeField] AK.Wwise.Event playEvent = null;
    [SerializeField] AK.Wwise.Event stopEvent = null;
    [Space(5)]
    [SerializeField] GameObject source = null;
    [Space(10)]
    [SerializeField] bool debugging = false;

    int switchIndex = 0;

    Coroutine pickingSongCo = null;


    void OnEnable() {
        if (switches.Length == 0) {
            Debug.LogError("[MusicPlayer] ERROR -> No switches defined! Disabling.");
        }
    }

    public void Play() {
        if (debugging) Debug.Log("[MusicPlayer] Playing music");
        playEvent.Post(source);
    }

    public void Stop() {
        if (debugging) Debug.Log("[MusicPlayer] Stopping music");
        stopEvent.Post(source);
    }


    public void NextSong() {
                
        switchIndex = (switchIndex + 1) % switches.Length;

        PickSong(switchIndex);
    }

    public void PreviousSong() {

        if (switchIndex == 0) switchIndex = switches.Length - 1;
        else switchIndex = switchIndex - 1;

        PickSong(switchIndex);
    }


    public void PickSong(int index) {
        if (pickingSongCo != null) StopCoroutine(pickingSongCo);
        pickingSongCo = StartCoroutine(PickingSong(index));
    }


    IEnumerator PickingSong(int index) {

        Stop();

        var audioSwitch = switches[index];
        audioSwitch.SetValue(source);

        if (debugging) Debug.Log("[MusicPlayer] Picking song {"+index+"}...");

        yield return new WaitForSeconds(delay);

        Play();

    }


}
