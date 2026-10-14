using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarcoGonzalezCampo : MonoBehaviour
{

    [SerializeField]
    AudioClip[] clips = new AudioClip[0];
    List<AudioSource> sources;
    int currentSongIndex = 0;

    [SerializeField]
    [Range(0f, 1f)]
    float maxVolume = 1f;

    [SerializeField]
    [Range(0.1f, 6f)]
    float crossfadeTime = 0.5f;
    bool fadeOcurring = false;

    readonly System.Random randomizer = new ();

    void Start()
    {
        sources = new List<AudioSource>();

        int lastSourceIndex = 0;

        for (int i = 0; i < clips.Length; ++i)
        {
            if (clips[i] != null)
            {
                sources.Add(gameObject.AddComponent<AudioSource>());
                sources[lastSourceIndex].loop = true;
                sources[lastSourceIndex].clip = clips[i];
                // First play needed for the pause to take effect
                sources[lastSourceIndex].Play();
                sources[lastSourceIndex].Pause();
                sources[lastSourceIndex].volume = 0;

                lastSourceIndex++;
            }
        }

        if (sources.Count > 0)
        {
            currentSongIndex = randomizer.Next(0, sources.Count);
            sources[currentSongIndex].UnPause();
            sources[currentSongIndex].volume = maxVolume;
        }
    }

    int tempNewIndex;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N) && !fadeOcurring && sources.Count > 1) {

            AudioSource fadeOut = sources[currentSongIndex];
            tempNewIndex = randomizer.Next(0, sources.Count - 1);
            // Prevent using the same source
            currentSongIndex = tempNewIndex < currentSongIndex ? tempNewIndex : ++tempNewIndex;
            AudioSource fadeIn = sources[currentSongIndex];

            // Sources are paused and unpaused to return to the same point when the sound was stopped
            fadeIn.UnPause();

            StartCoroutine(Crossfade(fadeIn, fadeOut)); 
        }
    }

    IEnumerator Crossfade(AudioSource fadeIn, AudioSource fadeOut)
    {
        fadeOcurring = true;

        float t = 0f;

        while(t < crossfadeTime)
        {
            t = Math.Min(t + Time.deltaTime, crossfadeTime);

            fadeIn.volume = Mathf.Sqrt(t/crossfadeTime) * maxVolume;
            fadeOut.volume = Mathf.Sqrt((crossfadeTime-t)/crossfadeTime) * maxVolume;

            yield return null;
        }

        fadeOut.Pause();

        fadeOcurring = false;
    }
}
