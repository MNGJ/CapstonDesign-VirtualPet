using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    public Transform playerPosition;
    public Transform handUIPosition;
    public Transform flatCanvas;
    public Slider Energy_Slider;
    public TextMeshPro Energy_num;
    public Slider Like_Slider;
    public TextMeshPro Like_num;
    public Slider Stress_Slider;
    public TextMeshPro Stress_num;
    public GameObject UI_Group2;
    public Sprite[] sprites;
    public SpriteRenderer RPS_sprite;
    public TextMeshPro count_text;

    public ParticleSystem[] Icons; // 0 = Happy, 1 = Defeat, 2 = Sleep, 3 = Puzzled

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

    public static UIManager Instance
    {
        get
        {
            if(null == _instance)
            {
                return null;
            }
            return _instance;
        }
    }

    private void Update()
    {
        //UIRotate(flatCanvas);
        Set_UI_Slider();
    }

    public void UIRotate(Transform obj)
    {
        Transform vrCameraTransorm = playerPosition.transform;
        Vector3 directionToLook = obj.position - vrCameraTransorm.position;

        directionToLook.y = 0;

        var _lookRotation = Quaternion.LookRotation(directionToLook);
        obj.rotation = Quaternion.Slerp(obj.rotation, _lookRotation, 3f * Time.deltaTime);
    }

    public void Set_UI_Slider()
    {
        Energy_Slider.value = IDA_State_Manger.Instance.Get_Energy();
        Energy_num.text = Mathf.RoundToInt(Energy_Slider.value).ToString();
        Like_Slider.value = IDA_State_Manger.Instance.Get_Like();
        Like_num.text = Mathf.RoundToInt(Like_Slider.value).ToString();
        Stress_Slider.value = IDA_State_Manger.Instance.Get_Stress();
        Stress_num.text = Mathf.RoundToInt(Stress_Slider.value).ToString();
    }

    public void Set_RPSsprite_None()
    {
        RPS_sprite.sprite = null;
    }

    public void Set_RPSsprite(int idx)
    {
        RPS_sprite.sprite = sprites[idx];
    }

    public void Set_Sprite_Question()
    {
        RPS_sprite.sprite = sprites[3];
    }

    public void Set_Sprite_Exclaim()
    {
        RPS_sprite.sprite = sprites[4];
    }

    public void Set_Active_UIGroup2()
    {
        if(!UI_Group2.activeSelf)
            UI_Group2.SetActive(true);
    }

    public void Set_DeActive_UIGroup2() 
    { 
        if(UI_Group2.activeSelf)
            UI_Group2.SetActive(false); 
    }

    public void Set_count_text(string text)
    {
        count_text.text = text;
    }

    public void IconActive(int idx)
    {
        Icons[idx].Play();
    }

    public void IconDeActive(int idx)
    {
        Icons[idx].Stop();
    }
}
