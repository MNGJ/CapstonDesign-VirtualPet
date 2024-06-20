using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;

    public AudioSource ida_sound;
    public AudioSource left_hand_UI;
    public AudioClip[] clips;

    private enum Snd_State
    {
        Idle,
        Win,
        Defeated,
        Puzzled,
        Battle
    }

    private Snd_State _state;

    private void Awake()
    {
        if(_instance == null)
        {
            _instance = this;

        }
        else if(_instance != this) 
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public static SoundManager Instance
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

    private void Update()
    {
        switch(_state)
        {
            default:
            case Snd_State.Idle:
                break;

            case Snd_State.Win:
                play_clip(clips[0], 0.5f, 1.5f);
                _state = Snd_State.Idle;
                break;
            case Snd_State.Defeated:
                play_clip(clips[1], 0.5f, 1f);
                _state = Snd_State.Idle;
                break;
            case Snd_State.Puzzled:
                play_clip(clips[2], 0.5f, 1.5f);
                _state = Snd_State.Idle;
                break;
            case Snd_State.Battle:
                play_clip(clips[3], 0.5f, 1.5f);
                _state = Snd_State.Idle;
                break;
        }
    }

    private void play_clip(AudioClip clip, float vol, float pitch)
    {
        ida_sound.clip = clip;
        ida_sound.loop = false;
        ida_sound.volume = vol;
        ida_sound.pitch = pitch;
        ida_sound.Play();
    }

    public void WinSound()
    {
        _state = Snd_State.Win;
    }
    public void DefeatedSound()
    {
        _state = Snd_State.Defeated;
    }
    public void PuzzledSound()
    {
        _state = Snd_State.Puzzled;
    }
    public void BattleSound()
    {
        _state = Snd_State.Battle;
    }
    public void Ida_like_play(AudioClip clip)
    {
        ida_sound.clip = clip;
        ida_sound.loop = false;
        ida_sound.volume = 1f;
        ida_sound.pitch = 1f;
        ida_sound.Play();
    }

    public void Ida_walk_play(AudioClip clip)
    {
        ida_sound.clip = clip;
        ida_sound.loop = false;
        ida_sound.volume = 0.2f;
        ida_sound.pitch = 1f;
        ida_sound.Play();
    }

    public void Ida_walk_stop(AudioClip clip)
    {
        ida_sound.clip = clip;
        ida_sound.Stop();
    }

    public void Left_Hand_Play(AudioClip clip)
    {
        left_hand_UI.clip = clip;
        left_hand_UI.loop = false;
        left_hand_UI.volume = 0.3f;
        left_hand_UI.Play();
    }

}
