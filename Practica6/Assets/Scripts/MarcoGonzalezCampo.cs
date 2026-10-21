using UnityEngine;

public class MarcoGonzalezCampo : MonoBehaviour
{
    AudioSource head;
    AudioSource tail;
    AudioSource casing;

    [SerializeField]
    AudioClip[] pcmDataHeads, pcmDataTails, pcmDataBursts, pcmDataCasings;

    int nHeads, nTails, nBursts, nCasings;

    [SerializeField]
    [Range(0f, 1f)]
    float pitchVariation = 0.1f;

    [Header("Volume")]
    [SerializeField]
    [Range(0f, 1f)]
    float minVol = 0f;
    [SerializeField]
    [Range(0f, 1f)]
    float maxVol = 1f;

    [SerializeField]
    [Min(0f)]
    float overlapTime = 0.01f;

    int overlapSamples;
    int sRATE;

    [SerializeField]
    [Min(0f)]
    float casingMaxPlayOffset = 1.0f;

    [SerializeField]
    [Range(0.5f, 10f)]
    float burstTime = 5f;
    float burstCurrentDuration = 0f;

    bool burstPlaying = false;

    [SerializeField]
    AudioReverbPreset reverbPreset = AudioReverbPreset.Arena;
    AudioReverbFilter reverb;

    void OnValidate()
    {
        minVol = Mathf.Clamp(minVol, 0, maxVol);
    }

    void Awake()
    {
        sRATE = AudioSettings.outputSampleRate;

        nHeads = pcmDataHeads.Length;
        nTails = pcmDataTails.Length;
        nBursts = pcmDataBursts.Length;
        nCasings = pcmDataCasings.Length;

        head = gameObject.AddComponent<AudioSource>();
        tail = gameObject.AddComponent<AudioSource>();
        casing = gameObject.AddComponent<AudioSource>();

        reverb = gameObject.AddComponent<AudioReverbFilter>();
        reverb.reverbPreset = reverbPreset;

        //  comprobar que son mono
        for (int i = 0; i < nHeads; i++)
        {
            if (pcmDataHeads[i].channels != 1) throw new System.Exception("Tienen que ser muestras mono!!");
        }
        for (int i = 0; i < nTails; i++)
        {
            if (pcmDataTails[i].channels != 1) throw new System.Exception("Tienen que ser muestras mono!!");
        }
        for (int i = 0; i < nBursts; i++)
        {
            if (pcmDataBursts[i].channels != 1) throw new System.Exception("Tienen que ser muestras mono!!");
        }
        for (int i = 0; i < nCasings; i++)
        {
            if (pcmDataCasings[i].channels != 1) throw new System.Exception("Tienen que ser muestras mono!!");
        }

        // convertimos tiempo de solapamiento a samples
        overlapSamples = (int)Mathf.Round(sRATE * overlapTime);
    }

    void Start()
    {
        Debug.Log("Start");
        // los fades se aplican sobre el clip original, el pitch se aplica luego. 
        // No hay que tenerlo en cuenta para aplicar los fades
        
        for (int i = 0; i < nHeads; i++) FadeOut(pcmDataHeads[i]);

        for (int i = 0; i < nTails; i++) FadeIn(pcmDataTails[i]);
        for(int i = 0; i < nTails; i++) FadeOut(pcmDataTails[i]);
        
        for (int i = 0; i < nCasings; i++) FadeIn(pcmDataCasings[i]);
    }

    void PlayShoot(bool isBurst = false) {

        int h,
            t = Random.Range(0, nTails),
            c = Random.Range(0, nCasings);

        tail.clip = pcmDataTails[t];
        casing.clip = pcmDataCasings[c];

        if (isBurst)
        {
            h = Random.Range(0, nBursts);
            head.clip = pcmDataBursts[h];
        }
        else 
        {
            h = Random.Range(0, nHeads);
            head.clip = pcmDataHeads[h];
        }

        float randVol = Mathf.Clamp(Random.Range(minVol, maxVol), 0f, 1f);

        head.volume = randVol;
        tail.volume = randVol;
        casing.volume = randVol;
        
        float randPitch = Mathf.Clamp(1 + Random.Range(-pitchVariation, pitchVariation), 1f, 2f);

        head.pitch = randPitch;
        tail.pitch = randPitch;
        casing.pitch = randPitch;

        double headClipLength = ((head.clip.samples - overlapSamples) / randPitch);
        double tailClipLength = ((tail.clip.samples - overlapSamples) / randPitch);

        Debug.Log($"head {h} p tail {t}  sRATE: {sRATE}     lengthHead {headClipLength}   overlapSamples {overlapSamples}");

        head.Play();
        // tail se reproduce solapado con head en el fragmento determinado por overlapSamples
        tail.PlayScheduled(AudioSettings.dspTime + headClipLength / sRATE);
        casing.PlayScheduled(AudioSettings.dspTime + Random.Range(0f, casingMaxPlayOffset) + ((headClipLength + tailClipLength) / sRATE));
    }

    void Update()
    {
        bool playing = head.isPlaying || tail.isPlaying || casing.isPlaying;

        if (burstPlaying) {
            burstCurrentDuration += Time.deltaTime;
            burstPlaying = burstCurrentDuration <= burstTime;
        }

        if (!playing)
        {
            if (burstPlaying) PlayShoot(true);
            else
            {
                if (Input.GetKeyDown(KeyCode.Space)) PlayShoot();
                else if (Input.GetKeyDown(KeyCode.B))
                {
                    burstPlaying = true;
                    burstCurrentDuration = 0f;
                }
            }
        }
    }

    //  crossFade out en [0,overlapTime]   sqrt((overlapTime-t)/overlapTime)  
    void FadeOut(AudioClip clip)
    {
        // pasamos clip a un array de samples
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        // ini: sample inicial donde aplicar fadeOut
        int ini = samples.Length - overlapSamples;
        for (int i = 0; i < overlapSamples; i++)
        {
            // mutiplicamos sample por valor de fadeOut. 
            // hay que convertir sample i a tiempo t = i/SRATE (regla de 3)
            samples[ini + i] *= Mathf.Sqrt((overlapTime - (i / sRATE)) / overlapTime);
        }

        // volcamos array al clip
        clip.SetData(samples, 0);
    }


    //  crossFadeIn en [0,overlapTime]   sqrt(t/overlapTime)  
    void FadeIn(AudioClip clip)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        for (int i = 0; i < overlapSamples; i++)
        {
            // ojo con la conversión de sample a tiempo
            samples[i] *= Mathf.Sqrt((i / sRATE) / overlapTime);
        }
        clip.SetData(samples, 0);
    }


}
