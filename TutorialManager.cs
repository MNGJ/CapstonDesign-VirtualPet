using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public AudioSource CenterAudio;
    public AudioClip Tutorial_appear;

    private static TutorialManager _instance;
    public Transform playerCamera;
    public float distanceFromCamera = 2.0f;
    public Vector3 offset = Vector3.zero;
    public GameObject StartTutorial;
    public GameObject EnergyTutorial;
    public GameObject AngryTutorial;
    public GameObject HappyTutorial;
    public GameObject SleepTutorial;
    public GameObject Like_Up_30_Dialog;
    public GameObject Like_Up_60_Dialog;
    public bool isUnlock_EnergyTuto = false;
    public bool isUnlock_AngryTuto = false;
    public bool isUnlock_HappyTuto = false;
    public bool isUnlock_SleepTuto = false;
    public bool isUnlock_Like_Up_30 = false;
    public bool isUnlock_Like_Up_60 = false;

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
    public static TutorialManager Instance
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
        StartCoroutine(ShowStartTutorial());
    }

    public void PositionUI(Transform transform)
    {
        // 카메라의 forward 벡터를 가져오되, y축 회전만 적용하도록 변경
        Vector3 cameraForward = new Vector3(playerCamera.forward.x, 0, playerCamera.forward.z).normalized;

        // 카메라 앞 일정 거리로 위치 설정
        Vector3 newPosition = playerCamera.position + cameraForward * distanceFromCamera;

        // 카메라의 y축과 동일하게 설정
        newPosition.y = playerCamera.position.y;

        // 오프셋 적용
        newPosition += offset;

        // UI의 위치 설정
        transform.position = newPosition;

        // UI가 카메라를 바라보도록 회전 설정, y축 회전만 고려
        transform.rotation = Quaternion.LookRotation(cameraForward, Vector3.up);
    }

    IEnumerator ShowStartTutorial()
    {
        yield return new WaitForSeconds(2f);
        PositionUI(StartTutorial.transform);
        StartTutorial.SetActive(true);
        CenterAudio.clip = Tutorial_appear;
        CenterAudio.Play();
    }

    public void CloseStartTutorial()
    {
        StartTutorial.SetActive(false);
    }

    public void ShowEnergyWarning()
    {
        //Time.timeScale = 0;
        PositionUI(EnergyTutorial.transform);
        EnergyTutorial.SetActive(true);
        isUnlock_EnergyTuto = true;
        CenterAudio.clip = Tutorial_appear;
        CenterAudio.Play();
    }

    public void CloseEnergyWarning()
    {
        //Time.timeScale = 1;
        EnergyTutorial.SetActive(false);
    }

    public void ShowAngryTuto()
    {
        PositionUI(AngryTutorial.transform);
        AngryTutorial.SetActive(true);
        isUnlock_AngryTuto = true;
        CenterAudio.clip = Tutorial_appear;
        CenterAudio.Play();
    }

    public void CloseAngryTuto()
    {
        AngryTutorial.SetActive(false);
    }

    public void ShowHappyTuto()
    {
        PositionUI(HappyTutorial.transform);
        HappyTutorial.SetActive(true);
        isUnlock_HappyTuto = true;
        CenterAudio.clip = Tutorial_appear;
        CenterAudio.Play();
    }

    public void CloseHappyTuto()
    {
        HappyTutorial.SetActive(false);
    }

    public void ShowSleepTuto()
    {
        PositionUI(SleepTutorial.transform);
        SleepTutorial.SetActive(true);
        isUnlock_SleepTuto = true;
        CenterAudio.clip = Tutorial_appear;
        CenterAudio.Play();
    }

    public void CloseSleepTuto()
    {
        SleepTutorial.SetActive(false);
    }

    public void ShowLike30Tuto()
    {
        PositionUI(Like_Up_30_Dialog.transform);
        Like_Up_30_Dialog.SetActive(true);
        isUnlock_Like_Up_30 = true;
        CenterAudio.clip = Tutorial_appear;
        CenterAudio.Play();
    }

    public void CloseLike30Tuto()
    {
        Like_Up_30_Dialog.SetActive(false);
    }

    public void ShowLike60Tuto()
    {
        PositionUI(Like_Up_60_Dialog.transform);
        Like_Up_60_Dialog.SetActive(true);
        isUnlock_Like_Up_60 = true;
        CenterAudio.clip = Tutorial_appear;
        CenterAudio.Play();
    }

    public void CloseLike60Tuto()
    {
        Like_Up_60_Dialog.SetActive(false);
    }
}
