using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
public enum MoveState
{
    Walking,
    Running,
    Crouching,
    Repeling
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
    public LayerMask repel;
    public Vector3 repelRight;

    bool atRepelPos = false;


    private bool groundedPlayer;
    [Header("Parameters")]
    public float playerSpeed = 9f;
    public float runningSpeed = 14f;
    public float crouchingSpeed = 4f;
    public float jumpHeight = 1.0f;
    public float gravityValue = -20.81f;
    public bool canMove = true;

    [Header("Repel and Window Jumping")]
    bool canJumpIntoWindow = false, isJumpingIntoSomething = false;
    public float repelSpeed = 1;
    public float jumpSpeedModifier = 1.6f;
    public TextMeshProUGUI jumpText;


    float normalHeight;
    public MoveState currentMoveState { get; private set; }
    Vector3 yVelocity;
    Vector3 readVelocity;
    float speed;


    Vector3 targetLerp;

    //Refrences
    PlayerAnimationHelper animHelper;
    MouseLook playerMouseLook;
    Animator animator;
    PhotonView PV;
    PlayerAudioManager audioManager;
    PlayerShooting playerShooting;

    public Transform test;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        playerMouseLook = GetComponent<MouseLook>();
        controller = gameObject.GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioManager = GetComponent<PlayerAudioManager>();
        animHelper = GetComponent<PlayerAnimationHelper>();
        playerShooting = GetComponent<PlayerShooting>();
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
        if(PlayerManager.Instance.localPlayerTeam == GameManager.Instance.currentTerroTeam)
            RepelCheck();
        Move();

    }
    #region Repeling and window jumping
    bool RepelMoveCheck(int dir)
    {
        //RaycastHit hit;
        Ray r = new Ray((transform.up + Vector3.down * Mathf.Abs(dir) * 0.2f) + transform.position + repelRight / 6 * dir, Vector3.up);
        return Physics.Raycast(r, Mathf.Infinity, repel);
    }
    bool canStartRepel()
    {
        if (currentMoveState == MoveState.Repeling)
            return false;
        if (!atRepelPos)
            return false;

        return true;
    }
    private bool CanJumpIntoWindow()
    {
        if (currentMoveState != MoveState.Repeling)
            return false;
        if (!canJumpIntoWindow)
            return false;
        if (isJumpingIntoSomething)
            return false;

        return true;
    }
    private bool CanJumpOnTheRoof()
    {
        if (RepelMoveCheck(0))
            return false;
        if (currentMoveState != MoveState.Repeling)
            return false;
        return true;
    }


    IEnumerator jumpIntoWindow()
    {
        DisablePlayer();
        isJumpingIntoSomething = true;
        Vector3 targetBack = -transform.forward;
        targetBack.y = 0;

        Vector3 startPos = transform.position;
        Vector3 change = Vector3.zero;
        float angle = 0;

        //up movement
        float t = 0;
        float speed = 1.5f;
        float sin;
        while(t < 1)
        {
            angle = ((3 * Mathf.PI) / 2);
            angle += Mathf.PI * t / 2;

            sin = Mathf.Sin(angle);

            change = targetBack * (sin+1) * 1.5f;
            change.y = sin + 1;
            transform.position = startPos + change;

            t += Time.deltaTime * speed;

            yield return new WaitForEndOfFrame();
        }
        //down movement
        t = 0;
        Vector3 startPos2 = transform.position;
        speed *= jumpSpeedModifier;
        while (t < 1)
        {
            transform.position = Vector3.Lerp(startPos2, startPos, t);
            t += Time.deltaTime * speed;
            yield return new WaitForEndOfFrame();
        }


        //Destroy window here
        RaycastHit hit;
        if(Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, Mathf.Infinity))
        {
            Debug.Log(LayerMask.LayerToName(hit.transform.gameObject.layer));
            Destructible_Barricade db = hit.transform.GetComponentInParent<Destructible_Barricade>();
            if(db != null)
                db.OnJump();
        }


        //front movement
        t = 0;
        while (t < 1)
        {
            change = -targetBack * 1.5f * t;
            transform.position = startPos + change;
            t += Time.deltaTime * speed;
            yield return new WaitForEndOfFrame();
        }
        EnablePlayer();
        isJumpingIntoSomething = false;
        canJumpIntoWindow = false;
    }
    IEnumerator jumpOnTheRoof()
    {
        Debug.Log("Jumping on the roof");
        DisablePlayer();
        isJumpingIntoSomething = true;
        

        //calculating where is the player front
        Vector3 playerFront = Quaternion.Euler(0, -90, 0) * repelRight;
        RaycastHit hit;
        
        //ray from the player up and front towards the ground - should detect roof to step on
        Ray ray = new Ray(transform.position + playerFront + Vector3.up*3,Vector3.down);


        float t = 0;
        float lerpSpeed = 2;

        //if detected ground to step on
        if(Physics.Raycast(ray,out hit,5,ground))
        {
            Debug.Log("Detected ground");
            //player moves up at start
            Vector3 startPos = transform.position;
            Vector3 targetPos = transform.position;
            targetPos.y = hit.point.y;

            while(t<=1)
            {
                t = Mathf.Clamp01(t);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                t += Time.deltaTime * lerpSpeed;
                yield return new WaitForEndOfFrame();
            }

            //and then moves on the point 
            t = 0;
            startPos = transform.position;
            targetPos = hit.point;
            while (t <= 1)
            {
                t = Mathf.Clamp01(t);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                t += Time.deltaTime * lerpSpeed;
                yield return new WaitForEndOfFrame();
            }
        }
        EnablePlayer();
        isJumpingIntoSomething = false;
    }
    void RepelCheck()
    {
        jumpText.text = "";
        if (CanJumpIntoWindow())
        {
            jumpText.text = "Press space to jump!";
            if (Input.GetButtonDown("Jump"))
                StartCoroutine(jumpIntoWindow());
        }

        if (isJumpingIntoSomething)
            return;


        if(CanJumpOnTheRoof())
        {
            jumpText.text = "Press space to jump on the roof!";
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(jumpOnTheRoof());
            }
        }
        else if (canStartRepel())
        {
            jumpText.text = "Press space to start repel!";

            if (!Input.GetButtonDown("Jump"))
                return;

            RaycastHit h;
            if (Physics.Raycast(camParent.position, camParent.forward, out h, 5f, ground))
            {
                StartCoroutine(StartRepelCorut(-h.normal, h.point));
            }

        }
        else if (currentMoveState == MoveState.Repeling)
        {
            if (jumpText.text.Equals(""))
                jumpText.text = "Press space to stop repel!";

            if (Input.GetButtonDown("Jump"))
                StopRepel();
        }
    }

    IEnumerator StartRepelCorut(Vector3 normal, Vector3 pos)
    {
        playerShooting.HideWeapons();

        controller.enabled = false;
        
        //Calculating how far player is from a wall
        Vector3 alongWall = Quaternion.Euler(0,90,0) * normal;
        Vector3 pointOnWall = new Vector3(pos.x, transform.position.y, pos.z);
        Ray ray = new Ray(pointOnWall, alongWall);
        float distance = Vector3.Cross(ray.direction, transform.position - ray.origin).magnitude;

        //Moving player so its always fixed distance from a wall
        transform.position = transform.position + normal*distance + -normal/1.5f;

        //Rotating player towards a wall
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, normal);
        playerMouseLook.SetRepel(rotation.eulerAngles.y, 45);

        //Playing animations
        animHelper.PlayLocalAndOnlineAnimation("StartRepel", "StartRepelOnline");
        
        yield return new WaitForSeconds(0.9f);


        controller.enabled = true;
        
        
        StartRepel(normal);
        playerShooting.ShowWeapons();
    }
    void StartRepel(Vector3 normal)
    {
        currentMoveState = MoveState.Repeling;
        repelRight = Quaternion.Euler(0, 90, 0) * normal;
    }
    void StopRepel()
    {
        playerMouseLook.StopRepel();
        currentMoveState = MoveState.Walking;
    }

#endregion
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
        if (Input.GetKeyDown(KeyCode.LeftShift) && currentMoveState != MoveState.Repeling)
        {
            if (currentMoveState == MoveState.Crouching)
                StopCrouching();

            currentMoveState = MoveState.Running;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) && currentMoveState == MoveState.Running)
        {
            currentMoveState = MoveState.Walking;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) && currentMoveState != MoveState.Repeling)
        {
            StartCrouching();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) && currentMoveState == MoveState.Crouching)
        {
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

        float x = 0;
        float z = 0;
        if (canMove)
        {
            x = Input.GetAxis("Horizontal");
            z = Input.GetAxis("Vertical");
        }

        if(currentMoveState == MoveState.Repeling)
        {
            Vector3 input = repelRight * x + transform.up * z;
            Vector3 finalMove = Vector3.zero;

            if ((x > 0 && RepelMoveCheck(1)) || (x < 0 && RepelMoveCheck(-1)))
            {
                finalMove.x += input.x;
                finalMove.z += input.z;
            }
            if (input.y > 0 && RepelMoveCheck(0))
            {       
                finalMove.y += input.y;
                
            }
            else if (input.y < 0)
                finalMove.y += input.y;

            readVelocity = finalMove;



            controller.Move(finalMove* repelSpeed * Time.deltaTime); 
            return;
        }


        if (groundedPlayer && yVelocity.y < 0)
        {
            yVelocity.y = -2f;
        }
        //Setting the animatior
        animator.SetFloat("velx", x);
        animator.SetFloat("vely", z);

        //Calculating move vector
        Vector3 move = transform.right * x + transform.forward * z;

        //Setting the read velocity for other scripts
        readVelocity = move;

        if (!controller.enabled)
            return;
            
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
        controller.enabled = false;
        playerMouseLook.StopRepel();
        playerShooting.enabled = false;
        playerShooting.HardStopAiming();
        canMove = false;
        currentMoveState = MoveState.Walking;
        playerShooting.AnimatorUpdate();
        GetComponent<PlayerInteractHandle>().cancelInteract();

    }
    public void EnablePlayer()
    {
        GetComponent<CharacterController>().enabled = true;
        playerShooting.enabled = true;
        canMove = true;
        playerShooting.AnimatorUpdate();
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Window")
            canJumpIntoWindow = true;

        if (other.tag == "RepelStartPos")
            atRepelPos = true;

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Window")
            canJumpIntoWindow = false;

        if (other.tag == "RepelStartPos")
            atRepelPos = false;
    }
}
