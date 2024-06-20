using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using HuggingFace.API;
using System;
using UnityEngine.Networking;
using Unity.Sentis;
using Unity.VisualScripting;
using LeastSquares.Undertone;
using UnityEngine.UIElements;

public class SpeechRecognition : MonoBehaviour
{
    private static SpeechRecognition _instance;

    public PushToTranscribe _transcriber;

    int sampleRate = 44100;
    private float[] samples_test;
    public float rmsValue;
    public float modulate;
    public int resultValue;
    public int cutValue;

    public SpriteRenderer sprite;
    public TextMeshPro txt;
    public Transform playerposition;
    public Transform UIGroupPosition;

    public AudioClip clip;
    private byte[] bytes;
    public bool recording;

    IWorker engine;
    IBackend backend;

    BackendType backendType = BackendType.GPUCompute;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public static SpeechRecognition Instance
    {
        get
        {
            if (null == _instance)
            {
                return null;
            }
            return _instance;
        }
    }

    private void Start()
    {
        backend = WorkerFactory.CreateBackend(backendType);

        samples_test = new float[sampleRate];
        clip = Microphone.Start(null, true, 1, 44100);
    }

    private void Update()
    {
        UIRotateTo();

        clip.GetData(samples_test, 0);
        float sum = 0;
        for (int i = 0; i < samples_test.Length; i++)
        {
            sum += samples_test[i] * samples_test[i];
        }
        rmsValue = Mathf.Sqrt(sum / samples_test.Length);
        rmsValue = rmsValue * modulate;
        rmsValue = Mathf.Clamp(rmsValue, 0, 100);
        resultValue = Mathf.RoundToInt(rmsValue);
        if (!recording && resultValue >= cutValue)
        {
            StartCoroutine(StartRecording());
            Debug.Log("isStart");
        }
    }

    void UIRotateTo()
    {
        Transform vrCameraTransorm = playerposition.transform;
        Vector3 directionToLook = vrCameraTransorm.position - UIGroupPosition.position;

        var _lookRotation = Quaternion.LookRotation(directionToLook);
        UIGroupPosition.rotation = Quaternion.Slerp(UIGroupPosition.rotation, _lookRotation, 2f*Time.deltaTime);
    }

    IEnumerator StartRecording()
    {
        recording = true;
        UIManager.Instance.Set_Active_UIGroup2();
        UIManager.Instance.Set_Sprite_Question();
        yield return new WaitForSeconds(1f);

        UIManager.Instance.Set_Sprite_Exclaim();
        //text.color = color.white;
        //text.text = "recording...";
        sprite.color = Color.red;
        //clip = Microphone.Start(null, false, 10, 44100);
        _transcriber.StartRecording();
        StartCoroutine(StopRecording());
    }

    IEnumerator StopRecording()
    {
        yield return new WaitForSeconds(3f);
        UIManager.Instance.Set_RPSsprite_None();
        UIManager.Instance.Set_DeActive_UIGroup2();

        Transcrip();
        //var position = Microphone.GetPosition(null);
        //Microphone.End(null);
        //var samples = new float[position * clip.channels];
       // clip.GetData(samples, 0);
        //RunWhisper.Instance.audioClip = clip;
        sprite.color = Color.yellow;

        //yield return new WaitForSeconds(0.1f);
        //RunWhisper.Instance.StartTranscribe();
        //JammoBehavior.Instance.OnOrderGiven(RunWhisper.Instance.outputString);
       // bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
       // SendRecording();
        clip = Microphone.Start(null, true, 1, 44100);
    }

    private async void Transcrip()
    {
        string transcription = await _transcriber.StopRecording();
        txt.text = transcription;
        JammoBehavior.Instance.OnOrderGiven(transcription);
    }

    private void SendRecording()
    {
        sprite.color = Color.yellow;

        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
            sprite.color = Color.green;
            JammoBehavior.Instance.OnOrderGiven(response);
            txt.text = response;
        }, error => {
            sprite.color = Color.green;
            JammoBehavior.Instance.OnOrderGiven(error);
            txt.text = error;
        });
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    AudioClip ConvertAudio(AudioClip inputAudio)
    {
        Debug.Log($"The frequency of the input audio clip is {inputAudio.frequency} Hz with {inputAudio.channels} channels.");
        Model model;
        if (inputAudio.frequency == 44100)
        {
            model = ModelLoader.Load(Application.streamingAssetsPath + "/audio_resample_44100_16000.sentis");
        }
        else if (inputAudio.frequency == 22050)
        {
            model = ModelLoader.Load(Application.streamingAssetsPath + "/audio_resample_22050_16000.sentis");
        }
        else
        {
            Debug.Log("Only frequencies of 44kHz and 22kHz are compatible");
            return null;
        }

        engine = WorkerFactory.CreateWorker(backendType, model);

        int channels = inputAudio.channels;
        int size = inputAudio.samples * channels;
        float[] data = new float[size];
        inputAudio.GetData(data, 0);
        using var input = new TensorFloat(new TensorShape(1, size), data);

        engine.Execute(input);

        float[] outData;

        var output = engine.PeekOutput() as TensorFloat;
        if (inputAudio.frequency == 44100)
        {
            //The model gives 2x as many samples as we would like so we fix it:
            //We need to pad it if it has odd number of samples
            int n = output.shape[1] % 2;
            using var output2 = TensorFloat.AllocNoData(new TensorShape(1, output.shape[1] + n));
            backend.Pad(output, output2, new int[] { 0, n }, Unity.Sentis.Layers.PadMode.Constant, 0);

            //Now we take every second sample:
            output2.Reshape(new TensorShape(output2.shape[1] / 2, 2));
            using var output3 = TensorFloat.AllocNoData(new TensorShape(output2.shape[0], 1));
            backend.Slice(output2, output3, new[] { 0 }, new[] { 1 }, new[] { 1 });
            output3.CompleteOperationsAndDownload();
            outData = output3.ToReadOnlyArray();
        }
        else
        {
            output.CompleteOperationsAndDownload();
            outData = output.ToReadOnlyArray();
        }

        int samplesOut = outData.Length / channels;

        AudioClip outputAudio = AudioClip.Create("outputAudio", samplesOut, channels, 16000, false);
        outputAudio.SetData(outData, 0);

        Debug.Log($"The audio has been converted to 16Khz with {channels} channels.");

        return outputAudio;
    }

    private void OnDestroy()
    {
        engine?.Dispose();
        backend?.Dispose();
    }
}

