using System;
using System.Collections;
using UnityEngine;

public class MarcoGonzalezCampo : MonoBehaviour
{
    enum SpawnStealing
    {
        Oldest,
        None
    }

    [SerializeField]
    [Range(1, 16)]
    int polyphony = 1;
    AudioSource[] sources;
    ulong[] timestamps;
    ulong nextTimeStamp = 0;
    int nextIndex = 0;

    [SerializeField]
    SpawnStealing spawnStealingPolicy = SpawnStealing.None;

    [Header("Play rate")]
    [SerializeField]
    [Range(0f, 30f )]
    float minTime;
    [SerializeField]
    [Range(0f, 30f )]
    float maxTime; 

    [Header("Volume")]
    [SerializeField]
    [Range(0f, 1f )]
    float minVol;
    [SerializeField]
    [Range(0f, 1f )]
    float maxVol;

    [Header("Pan")]
    [SerializeField]
    [Range(-1f, 1f )]
    float minPan;
    [SerializeField]
    [Range(-1f, 1f )]
    float maxPan;

    [Header("Pitch")]
    [SerializeField]
    [Range(0f, 1f )]
    float pitchVar = 0;

    [Header("Clips")]
    [SerializeField]
    AudioClip[] clipsVariants = new AudioClip[0];

    System.Random randomizer = new System.Random();

    bool canPlaySound = true;

    void OnValidate()
    {
        minTime = Mathf.Clamp(minTime, 0, maxTime);
        minVol = Mathf.Clamp(minVol, 0, maxVol);
        minPan = Mathf.Clamp(minPan, -1, maxPan);
    }

    void Start()
    {
        sources = new AudioSource[polyphony];
        timestamps = new ulong[polyphony];

        for (int i = 0; i < polyphony; ++i)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
            sources[i].playOnAwake = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canPlaySound && clipsVariants.Length > 0) StartCoroutine(PlaySound()); 
    }

    int GetNextIndex()
    {
        int sourceFound = -1;
        int tmp;

        for (int i = 0; i < polyphony && sourceFound == -1; ++i)
        {
            tmp = (nextIndex + i) % polyphony;
            if (!sources[tmp].isPlaying) 
                sourceFound = tmp;
        }

        return sourceFound;
    }

    int GetNextIndexStealOldest()
    {
        int sourceFound = -1;

        ulong oldestTimestampDistance = 0;
        int oldestIndex = nextIndex;

        int tmp;
        for (int i = 0; i < polyphony && sourceFound == -1; ++i)
        {
            tmp = (nextIndex + i) % polyphony;
            if (!sources[tmp].isPlaying) 
                sourceFound = tmp;
            else if (unchecked(nextTimeStamp - timestamps[tmp]) > oldestTimestampDistance)
                oldestIndex = tmp;
        }

        return sourceFound == -1 ? oldestIndex : sourceFound ;
    }

    IEnumerator PlaySound()
    {
        canPlaySound = false;

        yield return new WaitForSeconds(UnityEngine.Random.Range(minTime, maxTime));

        int sourceIndex = 
            spawnStealingPolicy == SpawnStealing.Oldest ? GetNextIndexStealOldest() : GetNextIndex();

        if (sourceIndex > -1)
        {
            nextIndex = (sourceIndex + 1) % polyphony;
            timestamps[sourceIndex] = nextTimeStamp++;

            sources[sourceIndex].clip = clipsVariants[randomizer.Next(0, clipsVariants.Length)];
            sources[sourceIndex].volume = UnityEngine.Random.Range(minVol, maxVol);
            sources[sourceIndex].panStereo = UnityEngine.Random.Range(minPan, maxPan);
            sources[sourceIndex].pitch = 1f + UnityEngine.Random.Range(-pitchVar, pitchVar);

            sources[sourceIndex].Play(); 
        }

        canPlaySound = true;
    }
}
