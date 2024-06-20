using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

/// <summary>
/// This class is used to control the behavior of our Robot (State Machine and Utility function)
/// </summary>
public class JammoBehavior : MonoBehaviour
{
    private static JammoBehavior _instance;

    /// <summary>
    /// The Robot Action List
    /// </summary>
    [System.Serializable]
    public struct Actions
    {
        public string sentence;
        public string verb;
        public string noun;
    }

    /// <summary>
    /// Enum of the different possible states of our Robot
    /// </summary>
    private enum State
    {
        Idle,
        Hello, // Say hello
        Happy, // Be happy
        Puzzled, // Be Puzzled
        MoveTo, // Move to a pillar
        BringObject, // Step one of bring object (move to it and grab it)
        BringObjectToPlayer, // Step two of bring object (move to player and drop the object)
        Happy2,
        RockPaperScissor,
        ChamChamCham
    }

    [Header("Robot Brain")]
    public JammoBrain jammoBrain;

    [Header("max score 임계값 0 ~ 1")]
    public float bias;

    [Header("Robot list of actions")]
    public List<Actions> actionsList;

    [Header("Robot list of sounds")]
    public AudioClip[] ida_like_sound;
    public AudioClip ida_walk_sound;

    [Header("NavMesh and Animation")]
    public Animator anim;                       // Robot Animator
    public NavMeshAgent agent;                  // Robot agent (takes care of robot movement in the NavMesh)
    public float reachedPositionDistance;       // Tolerance distance between the robot and object.
    public float reachedObjectPositionDistance; // Tolerance distance between the robot and object.
    public Transform playerPosition;            // Our position
    public GameObject goalObject;               
    public GameObject grabPosition;             // Position where the object will be placed during the grab

    public Camera cam;                          // Main Camera

    [Header("Input UI")]
    public TMPro.TMP_InputField inputField;     // Our Input Field UI

    private State state;

    [HideInInspector]
    public List<string> sentences; // Robot list of sentences (actions)
    public string[] sentencesArray;

    [HideInInspector]
    public float maxScore;
    public int maxScoreIndex;

    public Material characterMaterial;

    public AnimationClip idleNormal;
    public AnimationClip idleScared;
    public AnimationClip idleAngry;
    public AnimationClip idleHappy;

    private bool isPlayRPS = false;
    private bool isSleep;
    private bool isPlayCCC = false;

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

        // Set the State to Idle
        state = State.Idle;
        characterMaterial.SetTextureOffset("_MainTex", new Vector2(0, 0));

        // Take all the possible actions in actionsList
        foreach (JammoBehavior.Actions actions in actionsList)
        {
            sentences.Add(actions.sentence);
        }
        sentencesArray = sentences.ToArray();
    }

    public static JammoBehavior Instance
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

    void SetIdleAnimation()
    {
        anim.SetFloat("Energy", IDA_State_Manger.Instance.Get_Energy());
        anim.SetFloat("Stress", IDA_State_Manger.Instance.Get_Stress());
    }

    /// <summary>
    /// Rotate the agent towards the camera
    /// </summary>
    private void RotateTo()
    {
        Transform vrCameraTransorm = cam.transform;
        Vector3 directionToLook = vrCameraTransorm.position - transform.position;
        directionToLook.y = 0;

        var _lookRotation = Quaternion.LookRotation(directionToLook);
        agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation, _lookRotation, 360);
    }

    /// <summary>
    /// Grab the object by putting it in the grabPosition
    /// </summary>
    /// <param name="gameObject">Cube of color</param>
    void Grab(GameObject gameObject)
    {
        // Set the gameObject as child of grabPosition
        gameObject.transform.parent = grabPosition.transform;

        // To avoid bugs, set object velocity and angular velocity to 0
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Set the gameObject transform position to grabPosition
        gameObject.transform.position = grabPosition.transform.position;
    }

    /// <summary>
    /// Drop the gameObject
    /// </summary>
    /// <param name="gameObject">Cube of color</param>
    void Drop(GameObject gameObject)
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.transform.SetParent(null);
    }

    /// <summary>
    /// Utility function: Given the results of HuggingFaceAPI, select the State with the highest score
    /// </summary>
    /// <param name="maxValue">Value of the option with the highest score</param>
    /// <param name="maxIndex">Index of the option with the highest score</param>
    public void Utility(float maxScore, int maxScoreIndex)
    {
        // First we check that the score is > of 0.2, otherwise we let our agent perplexed;
        // This way we can handle strange input text (for instance if we write "Go see the dog!" the agent will be puzzled).
        if (maxScore < bias)
        {
            state = State.Puzzled;
        }
        else
        {
            // Get the verb and noun (if there is one)
            if (actionsList[maxScoreIndex].noun == "Point")
            {
                goalObject = GameObject.Find(PlayerInput.Instance.Get_Hit_Object());
            }
            else
            {
                goalObject = GameObject.Find(actionsList[maxScoreIndex].noun);
            }

            string verb = actionsList[maxScoreIndex].verb;

            // Set the Robot State == verb
            state = (State)System.Enum.Parse(typeof(State), verb, true);

            if(verb == "BringObject" &&  goalObject == null) { state = State.Puzzled; }

            Debug.Log(actionsList[maxScoreIndex].verb + " : " +  maxScore);
        }
    }

    /// <summary>
    /// When the user finished to type the order
    /// </summary>
    /// <param name="prompt"></param>
    public void OnOrderGiven(string prompt)
    {
        Debug.Log(prompt + "기븐 중");
        jammoBrain.RankSimilarityScores(prompt, sentencesArray);
    }
        
    public Vector3 GetDownPosition(Vector3 pos)
    {
        RaycastHit hit;
        float maxDistance = 100f;

        if (Physics.Raycast(pos, Vector3.down, out hit, maxDistance))
        {
            return hit.point;
        }
        else
            return pos;
    }

    public void Ida_Step()
    {
        SoundManager.Instance.Ida_walk_play(ida_walk_sound);
    }

    public void Set_EyePose()
    {
        characterMaterial.SetTextureOffset("_MainTex", new Vector2(0, 0));
    }

    private void Update()
    {
        if (isPlayRPS || isPlayCCC)
            return;
        if(IDA_State_Manger.Instance.isSleeping)
        {
            if (isSleep)
                return;
            isSleep = true;
            anim.SetTrigger("isSleep");
            anim.SetBool("Sleep", true);
            UIManager.Instance.IconActive(2);
            characterMaterial.SetTextureOffset("_MainTex", new Vector2(0, .66f));
        }
        else
        {
            if(isSleep)
            {
                isSleep = false;
                SpeechRecognition.Instance.sprite.color = Color.green;
                SpeechRecognition.Instance.recording = false;
                state = State.Idle;
                anim.SetBool("Sleep", false);
                UIManager.Instance.IconDeActive(2);
                characterMaterial.SetTextureOffset("_MainTex", new Vector2(0, 0));
            }
        }

        SetIdleAnimation();
        // Here's the State Machine, where given its current state, the agent will act accordingly
        switch (state)
        {
            default:
            case State.Idle:
                //characterMaterial.SetTextureOffset("_MainTex", new Vector2(0, 0));
                break;

            case State.Hello:
                agent.SetDestination(GetDownPosition(playerPosition.position));
                if (Vector3.Distance(transform.position, GetDownPosition(playerPosition.position)) < reachedPositionDistance)
                {
                    IDA_State_Manger.Instance.Set_Like(1f);
                    IDA_State_Manger.Instance.Set_Energy(-1f);
                    agent.SetDestination(transform.position);
                    RotateTo();
                    anim.SetBool("hello", true);
                    SoundManager.Instance.Ida_like_play(ida_like_sound[Random.Range(0, ida_like_sound.Length)]);
                    SpeechRecognition.Instance.sprite.color = Color.green;
                    SpeechRecognition.Instance.recording = false;
                    state = State.Idle;
                }
                break;

            case State.Happy:
                agent.SetDestination(GetDownPosition(playerPosition.position));
                if (Vector3.Distance(transform.position, GetDownPosition(playerPosition.position)) < reachedPositionDistance)
                {
                    IDA_State_Manger.Instance.Set_Stress(2f);
                    IDA_State_Manger.Instance.Set_Energy(-5f);
                    agent.SetDestination(transform.position);
                    RotateTo();
                    anim.SetBool("happy", true);
                    SpeechRecognition.Instance.sprite.color = Color.green;
                    SpeechRecognition.Instance.recording = false;
                    state = State.Idle;
                }
                break;

            case State.Puzzled:
                agent.SetDestination(GetDownPosition(playerPosition.position));
                if (Vector3.Distance(transform.position, GetDownPosition(playerPosition.position)) < reachedPositionDistance+0.5f)
                {
                    agent.SetDestination(transform.position);
                    RotateTo();
                    anim.SetBool("puzzled", true);
                    UIManager.Instance.IconActive(3);
                    SoundManager.Instance.PuzzledSound();
                    SpeechRecognition.Instance.sprite.color = Color.green;
                    SpeechRecognition.Instance.recording = false;
                    state = State.Idle;
                }
                break;

            case State.MoveTo:
                agent.SetDestination(GetDownPosition(goalObject.transform.position));
                
                if (Vector3.Distance(transform.position, GetDownPosition(goalObject.transform.position)) < reachedPositionDistance)
                {
                    agent.SetDestination(transform.position);
                    IDA_State_Manger.Instance.Set_Stress(2f);
                    IDA_State_Manger.Instance.Set_Energy(-1f);
                    SpeechRecognition.Instance.sprite.color = Color.green;
                    SpeechRecognition.Instance.recording = false;
                    state = State.Idle;
                }
                break;

            case State.BringObject:
                // First move to the object
                agent.SetDestination(goalObject.transform.position);
                if (Vector3.Distance(transform.position, goalObject.transform.position) < reachedObjectPositionDistance)
                {
                    Grab(goalObject);
                    state = State.BringObjectToPlayer;
                }
                break;

            case State.BringObjectToPlayer:
                agent.SetDestination(GetDownPosition(playerPosition.transform.position));
                if (Vector3.Distance(transform.position, GetDownPosition(playerPosition.transform.position)) < reachedObjectPositionDistance)
                {
                    agent.SetDestination(transform.position);
                    RotateTo();
                    IDA_State_Manger.Instance.Set_Stress(3f);
                    IDA_State_Manger.Instance.Set_Energy(-3f);
                    Drop(goalObject);
                    SpeechRecognition.Instance.sprite.color = Color.green;
                    SpeechRecognition.Instance.recording = false;
                    state = State.Idle;
                }
                break;
            case State.Happy2:
                if(PlayerInput.Instance.Get_HandPose() == "ThumbsUp")
                {
                    IDA_State_Manger.Instance.Set_Stress(-3f);
                    IDA_State_Manger.Instance.Set_Like(1f);
                    RotateTo();
                    anim.SetTrigger("happy2");
                    SpeechRecognition.Instance.sprite.color = Color.green;
                    SpeechRecognition.Instance.recording = false;
                    state = State.Idle;
                }
                else
                {
                    state = State.Puzzled;
                }
                break;
            case State.RockPaperScissor:
                if (TutorialManager.Instance.isUnlock_Like_Up_30)
                {
                    agent.SetDestination(GetDownPosition(playerPosition.position));
                    if (Vector3.Distance(transform.position, GetDownPosition(playerPosition.position)) < reachedPositionDistance)
                    {
                        isPlayRPS = true;
                        agent.SetDestination(transform.position);
                        RotateTo();
                        StartCoroutine(play_RPS());
                    }
                }
                else
                {
                    state = State.Puzzled;
                }
                break;
            case State.ChamChamCham:
                if (TutorialManager.Instance.isUnlock_Like_Up_60)
                {
                    agent.SetDestination(GetDownPosition(playerPosition.position));
                    if (Vector3.Distance(transform.position, GetDownPosition(playerPosition.position)) < reachedPositionDistance)
                    {
                        isPlayCCC = true;
                        agent.SetDestination(transform.position);
                        RotateTo();
                        StartCoroutine(play_CCC());
                    }
                }
                else
                {
                    state = State.Puzzled;
                }
                break;
        }
    }

    IEnumerator play_RPS()
    {
        Debug.Log("play_RPS Start");

        int value = -1; // 0 = Rock : 1 = Paper 2 : = Scissors
        int result = -1; // 0 = Draw : 1 = Player Win : 2 = Player Lose


        UIManager.Instance.Set_Active_UIGroup2();
        UIManager.Instance.Set_RPSsprite_None();
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle_Battle"))
        {
            SoundManager.Instance.BattleSound();
            anim.SetBool("isBattle", true);
        }

        UIManager.Instance.Set_count_text("Let's Play!");
        yield return new WaitForSeconds(3f);
        anim.SetBool("RPSState", true);

        UIManager.Instance.Set_count_text("3");
        yield return new WaitForSeconds(1f);
        UIManager.Instance.Set_count_text("2");
        yield return new WaitForSeconds(1f);
        UIManager.Instance.Set_count_text("1");
        yield return new WaitForSeconds(1f);
        UIManager.Instance.Set_count_text("");

        if (value == -1)
        {
            value = Random.Range(0, 3);
            Debug.Log("value : " + value);
            if(value == 0) { anim.SetBool("HandPose_Rock", true); }
            else if(value == 1) { anim.SetBool("HandPose_Paper", true); }
            else { anim.SetBool("HandPose_Scissors", true); }
        }
        UIManager.Instance.Set_RPSsprite(value);
        
        yield return new WaitForSeconds(0.5f);
        if (result == -1)
        {
            string player_handpose = PlayerInput.Instance.Get_HandPose();
            Debug.Log("hand_pose : " + player_handpose);
            if (value == 0)
            {
                if (player_handpose == "Paper" || player_handpose == "Paper_Flexion" || player_handpose == "Paper_Extension") result = 1;
                else if (player_handpose == "Scissors") result = 2;
                else result = 0;
            }
            else if (value == 1)
            {
                if (player_handpose == "Rock") result = 2;
                else if (player_handpose == "Scissors") result = 1;
                else result = 0;
            }
            else
            {
                if (player_handpose == "Rock") result = 1;
                else if (player_handpose == "Paper" || player_handpose == "Paper_Flexion" || player_handpose == "Paper_Extension") result = 2;
                else result = 0;
            }
        }

        yield return new WaitForSeconds(3f);

        UIManager.Instance.Set_RPSsprite_None();
        UIManager.Instance.Set_DeActive_UIGroup2();
        if(anim.GetBool("isBattle"))
            anim.SetBool("isBattle", false);
        if(result == 0)
        {
            result = -1;
            IDA_State_Manger.Instance.Set_Energy(-2f);
            anim.SetBool("puzzled", true);
            SoundManager.Instance.PuzzledSound();
        }
        else if(result == 1)
        {
            result = -1;
            IDA_State_Manger.Instance.Set_Energy(-2f);
            IDA_State_Manger.Instance.Set_Stress(2f);
            IDA_State_Manger.Instance.Set_Like(1f);
            anim.SetBool("Defeated", true);
            UIManager.Instance.IconActive(1);
            SoundManager.Instance.DefeatedSound();
        }
        else if(result == 2)
        {
            result = -1;
            IDA_State_Manger.Instance.Set_Energy(-2f);
            IDA_State_Manger.Instance.Set_Stress(-2f);
            IDA_State_Manger.Instance.Set_Like(1f);
            anim.SetBool("happy", true);
            UIManager.Instance.IconActive(0);
            SoundManager.Instance.WinSound();
        }

        yield return new WaitForSeconds(5f);
        Debug.Log("play_RPS Stop");
        SpeechRecognition.Instance.sprite.color = Color.green;
        SpeechRecognition.Instance.recording = false;
        state = State.Idle;
        isPlayRPS = false;
    }

    IEnumerator play_CCC()
    {
        Debug.Log("is Play CCC Game");

        int value = -1; // 0 = Left Rotate, 1 = Right Rotate
        int result = -1; // 0 = Draw : 1 = Player Win : 2 = Player Lose

        UIManager.Instance.Set_Active_UIGroup2();
        UIManager.Instance.Set_RPSsprite_None();
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle_Battle"))
        {
            SoundManager.Instance.BattleSound();
            anim.SetBool("isBattle", true);
        }

        UIManager.Instance.Set_count_text("Your Turn!");
        yield return new WaitForSeconds(3f);

        UIManager.Instance.Set_count_text("3");
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.Set_count_text("2");
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.Set_count_text("1");
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.Set_count_text("");

        UIManager.Instance.Set_RPSsprite_None();
        UIManager.Instance.Set_DeActive_UIGroup2();

        if (value == -1)
        {
            value = Random.Range(0, 2);
            Debug.Log("value : " + value);
        }

        yield return new WaitForSeconds(0.5f);

        if (result == -1) // 0 = Draw : 1 = Player Win : 2 = Player Lose
        {
            string player_handpose = PlayerInput.Instance.Get_HandPose();
            Debug.Log("hand_pose : " + player_handpose);
            if (value == 0)
            {
                anim.SetBool("HeadRotate_Left", true);
                if (player_handpose == "Paper_Extension") result = 1;
                else result = 2;
            }
            else if (value == 1)
            {
                anim.SetBool("HeadRotate_Right", true);
                if (player_handpose == "Paper_Extension") result = 2;
                else result = 1;
            }
        }

        yield return new WaitForSeconds(3f);

        if (anim.GetBool("isBattle"))
            anim.SetBool("isBattle", false);
        if (result == 1)
        {
            result = -1;
            IDA_State_Manger.Instance.Set_Energy(-2f);
            IDA_State_Manger.Instance.Set_Stress(2f);
            IDA_State_Manger.Instance.Set_Like(1f);
            anim.SetBool("Defeated", true);
            UIManager.Instance.IconActive(1);
            SoundManager.Instance.DefeatedSound();
        }
        else if (result == 2)
        {
            result = -1;
            IDA_State_Manger.Instance.Set_Energy(-2f);
            IDA_State_Manger.Instance.Set_Stress(-2f);
            IDA_State_Manger.Instance.Set_Like(1f);
            anim.SetBool("happy", true);
            UIManager.Instance.IconActive(0);
            SoundManager.Instance.WinSound();
        }


        ////////////////////////////////////// pase 2
        ///
        yield return new WaitForSeconds(5f);

        value = -1; // 0 = Left Rotate, 1 = Right Rotate
        result = -1; // 0 = Draw : 1 = Player Win : 2 = Player Lose

        UIManager.Instance.Set_Active_UIGroup2();
        UIManager.Instance.Set_RPSsprite_None();
        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle_Battle"))
        {
            SoundManager.Instance.BattleSound();
            anim.SetBool("isBattle", true);
        }

        UIManager.Instance.Set_count_text("My Turn!");
        yield return new WaitForSeconds(3f);
        anim.SetBool("CCCState", true);

        UIManager.Instance.Set_count_text("3");
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.Set_count_text("2");
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.Set_count_text("1");
        yield return new WaitForSeconds(0.5f);
        UIManager.Instance.Set_count_text("");

        UIManager.Instance.Set_RPSsprite_None();
        UIManager.Instance.Set_DeActive_UIGroup2();

        if (value == -1)
        {
            value = Random.Range(0, 2);
            Debug.Log("value : " + value);
        }

        yield return new WaitForSeconds(0.5f);

        if (result == -1) // 0 = Draw : 1 = Player Win : 2 = Player Lose
        {
            Vector3 playerForward = playerPosition.forward; // 플레이어 시점 벡터
            Vector3 toTarget = transform.position - playerPosition.position; // 플레이어에서 오브젝트로 향하는 벡터
            Vector3 crossProduct = Vector3.Cross(playerForward, toTarget); // 두 벡터의 교차곱

            Debug.Log("교차 곱 : " + crossProduct.y);
            // 교차곱 y값을 통해 왼쪽/ 오른쪽 판단. y > 0 이면 왼쪽, y < 0이면 오른쪽
            if (value == 0)
            {
                anim.SetBool("HandRotate_Left", true);
                if (crossProduct.y >= 0) result = 1; 
                else result = 2;
            }
            else if (value == 1)
            {
                anim.SetBool("HandRotate_Right", true);
                if (crossProduct.y < 0) result = 1;
                else result = 2;
            }
            Debug.Log("결과 : " + result);
        }

        yield return new WaitForSeconds(3f);

        if (anim.GetBool("isBattle"))
            anim.SetBool("isBattle", false);
        if (result == 1)
        {
            result = -1;
            IDA_State_Manger.Instance.Set_Energy(-2f);
            IDA_State_Manger.Instance.Set_Stress(2f);
            IDA_State_Manger.Instance.Set_Like(1f);
            anim.SetBool("Defeated", true);
            UIManager.Instance.IconActive(1);
            SoundManager.Instance.DefeatedSound();
        }
        else if (result == 2)
        {
            result = -1;
            IDA_State_Manger.Instance.Set_Energy(-2f);
            IDA_State_Manger.Instance.Set_Stress(-2f);
            IDA_State_Manger.Instance.Set_Like(1f);
            anim.SetBool("happy", true);
            UIManager.Instance.IconActive(0);
            SoundManager.Instance.WinSound();
        }

        yield return new WaitForSeconds(5f);
        Debug.Log("play_CCC Stop");
        SpeechRecognition.Instance.sprite.color = Color.green;
        SpeechRecognition.Instance.recording = false;
        state = State.Idle;
        isPlayCCC = false;
    }
}
