using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDA_State_Manger : MonoBehaviour
{
    private static IDA_State_Manger _instance;

    public float Energy;
    public float Like;
    public float Stress;
    private float max = 100f;
    private float min = 0f;
    public bool isSleeping;

    public Transform energyParticlePosition;
    public Transform likeParticlePosition;
    public Transform stressParticlePosition;

    public GameObject energyParticlePrefab;
    public GameObject likeParticlePrefab;
    public GameObject stressParticlePrefab;

    private string current_state;
    private float cnt;

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

        current_state = "Idle";
        cnt = 0;
    }

    public static IDA_State_Manger Instance
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

    // Update is called once per frame
    void Update()
    {
        Debug.Log(current_state);
        if(Energy >= max) Energy = max;
        if(Energy <= min)
        {
            Energy = min;
            isSleeping = true;
            current_state = "Sleep";
        }
        if(Like >= max) Like = max;
        if(Like <= min) Like = min;
        if(Stress >= max) Stress = max;
        if(Stress <= min) Stress = min;
        if(isSleeping)
        {
            if(!TutorialManager.Instance.isUnlock_SleepTuto)
            {
                TutorialManager.Instance.ShowSleepTuto();
            }
            Energy += Time.deltaTime * 3;
            Stress -= Time.deltaTime * 1;
            cnt += Time.deltaTime;
            if(cnt > 3f)
            {
                cnt = 0f;
                StartCoroutine(Particle_Inst(energyParticlePrefab, energyParticlePosition, 0));
            }
            if (Energy >= max) isSleeping = false;
            return;
        }
        else
        {
            Energy -= Time.deltaTime * 0.5f;
            Stress -= Time.deltaTime * 0.2f;
        }
        if(Like >= 30)
        {
            if(!TutorialManager.Instance.isUnlock_Like_Up_30)
            {
                TutorialManager.Instance.ShowLike30Tuto();
            }
        }
        if(Like >= 60)
        {
            if(!TutorialManager.Instance.isUnlock_Like_Up_60)
            {
                TutorialManager.Instance.ShowLike60Tuto();
            }
        }
        if(Energy < 20)
        {
            current_state = "Scared";
            if(!TutorialManager.Instance.isUnlock_EnergyTuto)
            {
                TutorialManager.Instance.ShowEnergyWarning();
            }
        }
        else
        {
            if (Stress > 80)
            {
                current_state = "Angry";
                if (!TutorialManager.Instance.isUnlock_AngryTuto)
                {
                    TutorialManager.Instance.ShowAngryTuto();
                }
            }
            else if (Stress < 20)
            {
                current_state = "Happy";
                if(!TutorialManager.Instance.isUnlock_HappyTuto)
                {
                    TutorialManager.Instance.ShowHappyTuto();
                }
            }
            else
                current_state = "Idle";
        }
    }

    public void Set_Energy(float num)
    {
        StartCoroutine(Particle_Inst(energyParticlePrefab, energyParticlePosition, num));
        if (Energy + num > 100f)
            Energy = 100f;
        else if (Energy + num < 0)
            Energy = 0f;
        else
            Energy += num;
    }
    public void Set_Like(float num)
    {
        StartCoroutine(Particle_Inst(likeParticlePrefab, likeParticlePosition, num));
        if (Like + num > 100f)
            Like = 100f;
        else if (Like + num < 0)
            Like = 0f;
        else
            Like += num;
    }
    public void Set_Stress(float num)
    {
        StartCoroutine(Particle_Inst(stressParticlePrefab, stressParticlePosition, num));
        if (Stress + num > 100f)
            Stress = 100f;
        else if (Stress + num < 0)
            Stress = 0f;
        else
            Stress += num;
    }
    public float Get_Energy()
    {
        return Energy;
    }
    public float Get_Like()
    {
        return Like;
    }
    public float Get_Stress()
    {
        return Stress;
    }

    public string Get_State()
    {
        return current_state;
    }

    IEnumerator Particle_Inst(GameObject Prefab, Transform Pos, float num)
    {
        int temp = Mathf.RoundToInt(Mathf.Abs(num));
        GameObject inst = Instantiate(Prefab, Pos.position, Pos.rotation);
        inst.transform.SetParent(Pos);
        inst.transform.localScale = Vector3.one * 1.5f;
        GameObject HPLabel = inst.transform.Find("HPLabel").gameObject;

        if (num > 0) HPLabel.GetComponent<TextMesh>().text = "+" + temp;
        else if (num < 0) HPLabel.GetComponent<TextMesh>().text = "-" + temp;
        else HPLabel.GetComponent<TextMesh>().text = "+";
        yield return new WaitForSeconds(0.5f);
    }
}
