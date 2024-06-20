using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mictest : MonoBehaviour
{

    public AudioClip aud;
    int sampleRate = 44100;
    private float[] samples;
    public float rmsValue;
    public float modulate;
    public int resultValue;
    public int cutValue;

    // Start is called before the first frame update
    void Start()
    {
        samples = new float[sampleRate];
        aud = Microphone.Start(Microphone.devices[0].ToString(), true, 1, sampleRate);
    }

    // Update is called once per frame
    void Update()
    {
        aud.GetData(samples, 0); // -1f ~ 1f
        float sum = 0;
        for(int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }
        rmsValue = Mathf.Sqrt(sum / samples.Length);
        rmsValue = rmsValue * modulate;
        rmsValue = Mathf.Clamp(rmsValue, 0, 100);
        resultValue = Mathf.RoundToInt(rmsValue);
        if (resultValue < cutValue)
            resultValue = 0;
    }
}
