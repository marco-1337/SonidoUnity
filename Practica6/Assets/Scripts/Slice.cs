using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SchedEvent: MonoBehaviour {
    private AudioSource head;
    private AudioSource tail;
    //public AudioClip sound01, sound02;
    public AudioClip[] pcmDataHeads, pcmDataTails;
    private int nHeads, nTails;


    void Awake(){
        nHeads = pcmDataHeads.Length;
        nTails = pcmDataTails.Length;
        head = gameObject.AddComponent<AudioSource>();        
        tail = gameObject.AddComponent<AudioSource>();        
    }

    void Start(){
    }

    // Update is called once per frame
    void Update(){
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            int h = Random.Range(0, nHeads), t = Random.Range(0, nTails);
            head.clip = pcmDataHeads[h];
            tail.clip = pcmDataTails[t];
            
            double clipLength = (double) head.clip.samples / head.pitch;
            
            int sRATE = AudioSettings.outputSampleRate;
            Debug.Log($"head {h} length {clipLength}  p tail {t}  sRATE: {sRATE}");

            head.Play();
            tail.PlayScheduled(AudioSettings.dspTime+clipLength/sRATE);
        }
    }
}
