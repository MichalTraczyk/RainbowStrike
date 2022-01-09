using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public enum MoveState
{
    Walking,
    Running,
    Crouching
}
public class PlayerMove : MonoBehaviour
{
    [Header("Assignables")]
    private CharacterController controller;
    public Transform groundCheck;
    public Transform camParent;

    public Vector3 NormalCamPos;
    public Vector3 CrouchCamPos;
    public Vector3 CrouchWalkCamPos;
    public float crouchHeight;
    public LayerMask ground;


    private bool groundedPlayer;
    [Header("Parameters")]
    public float playerSpeed = 9f;
    public float runningSpeed = 14f;
    public float crouchingSpeed = 4f;
    public float jumpHeight = 1.0f;
    public float gravityValue = -20.81f;
    public bool canMove = true;


    float normalHeight;
    public MoveState currentMoveState { get; private set; }
    Vector3 yVelocity;
    Vector3 readVelocity;
    float speed;


    Vector3 targetLerp;

    //Refrences
    Animator animator;
    PhotonView PV;
    PlayerAudioManager audioManager;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        controller = gameObject.GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioManager = GetComponent<PlayerAudioManager>();
        targetLerp = NormalCamPos;
    }
    void StartCrouching()
    {
        animator.SetBool("Crouching",true);
        controller.height = crouchHeight;
        controller.center = new Vector3(0, 0.52f, 0);
        currentMoveState = MoveState.Crouching;
        targetLerp = CrouchCamPos;
    }
    void StopCrouching()
    {
        animator.SetBool("Crouching", false);
        controller.center = new Vector3(0, 0.9f, 0);
        controller.height = normalHeight;
        currentMoveState = MoveState.Walking;
        targetLerp = NormalCamPos;
    }
    private void Start()
    {
        speed = playerSpeed;
        normalHeight = controller.height;
        if (!PV.IsMine)
            controller.enabled = false;
    }

    void Update()
    {

        if (!PV.IsMine || GlobalUIManager.Instance.paused)
            return;
        CrouchCameraLerp();
        MyInput();

        Move();
    }
    void CrouchCameraLerp()
    {
        if((Mathf.Abs(readVelocity.x) + Mathf.Abs(readVelocity.z) > 0.1f) && currentMoveState == MoveState.Crouching)
        {
            camParent.localPosition = Vector3.Lerp(camParent.localPosition, CrouchWalkCamPos, Time.deltaTime * 10);
        }
        else
        {
            camParent.localPosition = Vector3.Lerp(camParent.localPosition, targetLerp,Time.deltaTime * 10);
        }
    }
    void MyInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (currentMoveState == MoveState.Crouching)
                StopCrouching();

            currentMoveState = MoveState.Running;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            if (currentMoveState == MoveState.Running)
                currentMoveState = MoveState.Walking;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartCrouching();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (currentMoveState == MoveState.Crouching)
                StopCrouching();
        }
    }
    void Move()
    {
        switch (currentMoveState)
        {
            case MoveState.Walking:
                speed = playerSpeed;
                break;
            case MoveState.Running:
                speed = runningSpeed;
                break;
            case MoveState.Crouching:
                speed = crouchingSpeed;
                break;
            default:
                speed = playerSpeed;
                break;
        }

        //Ground check
        groundedPlayer = Physics.CheckSphere(groundCheck.position, 0.2f, ground);
        if (groundedPlayer && yVelocity.y < 0)
        {
            yVelocity.y = -2f;
        }


        float x = 0;
        float z = 0;
        if (canMove)
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }
        //Setting the animatior
        animator.SetFloat("velx", x);
        animator.SetFloat("vely", z);

        //Calculating move vector
        Vector3 move = transform.right * x + transform.forward * z;

        //Setting the read velocity for other scripts
        readVelocity = move;

        //Moving 
        controller.Move(move * speed * Time.deltaTime);

        //Jumping
        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            yVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravityValue);
            animator.SetTrigger("Jump");
        }

        //calculatin gravity
        yVelocity.y += gravityValue * Time.deltaTime;


        //Applying gravity
        controller.Move(yVelocity * Time.deltaTime);
    }
    public void DisablePlayer()
    {
        GetComponent<CharacterController>().enabled = false;
        GetComponent<PlayerShooting>().enabled = false;
        GetComponent<PlayerShooting>().HardStopAiming();
        canMove = false;
        currentMoveState = MoveState.Walking;
        GetComponent<PlayerShooting>().AnimatorUpdate();

    }
    public void EnablePlayer()
    {
        GetComponent<CharacterController>().enabled = true;
        GetComponent<PlayerShooting>().enabled = true;
        canMove = true;
        GetComponent<PlayerShooting>().AnimatorUpdate();
    }
    public bool isWalking()
    {
        bool walking = false;
        Vector2 v = new Vector2(readVelocity.x, readVelocity.z);
        if (v.magnitude > 0.1f)
        {
            walking = true;
        }

        return walking;
    }
}
