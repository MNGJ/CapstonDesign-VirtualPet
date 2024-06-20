using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private static PlayerInput _instance;

    [SerializeField] private GameObject UI_ida_Group;
    public AudioClip clickSnd;
    [SerializeField] private GameObject UI_HandPose;
    public string current_HandPose;
    public Transform index_Finger;
    public LineRenderer lineRenderer;
    public float rayLength = 10f;
    public string hitting_name;

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

    public static PlayerInput Instance
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
        if (current_HandPose == "Pointing")
        {
            ShootRayFromFinger();
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    public void OnOff_UI_Group()
    {
        if (UI_ida_Group.activeSelf == true)
        {
            UI_ida_Group.SetActive(false);
            SoundManager.Instance.Left_Hand_Play(clickSnd);
        }
        else if (UI_ida_Group.activeSelf == false)
        {
            UI_ida_Group.SetActive(true);
            TutorialManager.Instance.PositionUI(UI_ida_Group.transform);
            UI_ida_Group.transform.position += new Vector3(0, 0.3f, 0);
            SoundManager.Instance.Left_Hand_Play(clickSnd);
        }
    }

    public void OnOff_UI_HandPose()
    {
        if (UI_HandPose.activeSelf == true)
        {
            UI_HandPose.SetActive(false);
        }
        else if (UI_HandPose.activeSelf == false)
        {
            UI_HandPose.SetActive(true);
        }
    }

    public void Set_HandPose(string str)
    {
        current_HandPose = str;
    }

    public string Get_HandPose() { return current_HandPose; }

    public string Get_Hit_Object() { return hitting_name; }

    private void ShootRayFromFinger()
    {
        Vector3 rayOrigin = index_Finger.position;
        Vector3 rayDirection = index_Finger.right;

        Ray ray = new Ray(rayOrigin, rayDirection);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, rayLength))
        {
            Debug.Log($"Hit object: {hitInfo.collider.gameObject.name}");
            hitting_name = hitInfo.collider.gameObject.name;
            // hitInfo를 사용하여 원하는 작업을 수행합니다.
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, hitInfo.point);
        }
        else
        {
            lineRenderer.SetPosition(0, rayOrigin);
            lineRenderer.SetPosition(1, rayOrigin + rayDirection * rayLength);
        }
        lineRenderer.enabled = true;
    }
}
