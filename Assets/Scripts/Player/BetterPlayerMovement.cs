using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class BetterPlayerMovement : MonoBehaviour
{
    OwnPlayer controls;
    private Camera mainCam;
    [HideInInspector] public float health;

    // Movement
    private Vector2 move;
    private float speed = 10;
    private float playerRotation;
    private Rigidbody playerRB;
    Quaternion endRotation;

    //Jump
    private float jump;
    private float jumpDistance = 10;
    private bool jumping = false;
    private int jumpMax = 1;
    private float jumpCooldown;
    private int currentJump = 1;
    private bool jumpButton = true;
    private bool grounded = true;

    //Interact
    [HideInInspector] public float action;
    private bool movementPU;
    private float movementPUTimer;

    //Camera
    private Vector2 cameraVector;
    private Camera followCamera;
    private Vector3 cameraDistance;
    private Vector3 cameraForward;
    private Vector3 cameraRight;
    private float cameraRotation;
    private float offsetZ;
    private float offsetX;
    private float offsetY;
    private float rotationSpeed;
    private float joystickGap;
    private Vector3 xAxis;
    private Camera splineCamera;

    //Combat
    private float attacking;
    [HideInInspector] public bool doAttack;
    private float attackTimer;
    private float attackCooldown;

    private Animator playerAnimator;

    //0 Idle
    //1 Jumping
    //2 Die
    //3 Attack
    //4 Interact
    //5 Running
    //6 Damaged

    private void Awake()
    {
        health = 100;

        attackCooldown = 0.6f;
        attackTimer = attackCooldown;

        mainCam = Camera.main;
        playerAnimator = GetComponent<Animator>();
        playerRB = GetComponent<Rigidbody>();

        followCamera = Camera.main;
        offsetZ = -7;
        offsetY = 5;
        offsetX = 0;
        rotationSpeed = 60;
        joystickGap = 0.45f;

        //Using Unity's new Input System
        controls = new OwnPlayer();

        controls.Player.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => move = Vector2.zero;

        controls.Player.Jump.performed += ctx => jump = ctx.ReadValue<float>();
        controls.Player.Jump.canceled += ctx => jump = 0.0f;

        controls.Player.ActionInteract.performed += ctx => action = ctx.ReadValue<float>();
        controls.Player.ActionInteract.canceled += ctx => action = 0.0f;

        controls.Player.CameraMove.performed += ctx => cameraVector = ctx.ReadValue<Vector2>();
        controls.Player.CameraMove.canceled += ctx => cameraVector = Vector2.zero;

        controls.Player.Attack.performed += ctx => attacking = ctx.ReadValue<float>();
        controls.Player.Attack.canceled += ctx => attacking = 0.0f;
    }

    private void Start()
    {
        cameraDistance = new Vector3(transform.position.x + offsetX, transform.position.y + offsetY, transform.position.z + offsetZ);
    }

    private void FixedUpdate()
    {
        PlayerControlsUpdate();
        CameraFollow();
        PowerUps();
        Jumping();
        Attacking();
        Death();
    }

    private void Attacking()
    {
        if (attacking > 0 && doAttack == false)
        {
            doAttack = true;
        }

        if (doAttack == true)
        {
            attackTimer -= Time.deltaTime;
            playerAnimator.SetInteger("CurrentState", 3);
            if (attackTimer < 0)
            {
                doAttack = false;
                attackTimer = attackCooldown;
            }
        }
    }

    private void Death()
    {
        if (health <= 0)
        {
            Debug.Log("Death");
        }
    }

    private void Jumping()
    {

        if (currentJump == 0)
        {
            jumping = false;
        }

        if (jump == 1 && jumpButton == true)
        {
            if (currentJump > 0)
            {
                jumping = true;
            }

            jumpButton = false;
        }
        else if (jump == 0)
        {
            jumpButton = true;
            jumping = false;
        }

        if (jumping == true)
        {
            playerAnimator.SetInteger("CurrentState", 1);
            playerRB.velocity = new Vector3(playerRB.velocity.x, jumpDistance, 0);
            if (currentJump == 2)
            {
                currentJump = 1;
                jumping = false;
            }
            else if (currentJump == 1)
            {
                currentJump = 0;
                jumping = false;
            }
        }

        if (grounded == false && jump == 0)
        {
            currentJump = jumpMax;
        }
    }

    private void PowerUps()
    {
        if (action > 0)
        {
            //Debug.Log("Playing");
            playerAnimator.SetInteger("CurrentState", 4);
        }

        if (movementPU == true)
        {
            movementPUTimer += Time.deltaTime;
            if (movementPUTimer > 5)
            {
                speed = speed / 2;
                movementPU = false;
            }
        }
    }

    private void CameraFollow()
    {
        //Following Player
        xAxis = followCamera.transform.TransformDirection(cameraVector.y, 0.0f, 0.0f);

        cameraDistance = new Vector3(transform.position.x + offsetX, transform.position.y + offsetY,
            transform.position.z + offsetZ);
        followCamera.transform.position = cameraDistance;


        //Rotation
        if (cameraVector.x < -joystickGap || cameraVector.x > joystickGap)
        {
            followCamera.transform.RotateAround(transform.position,
                new Vector3(0.0f, cameraVector.x, 0.0f), rotationSpeed * Time.deltaTime);
        }

        if (-cameraVector.y < -joystickGap || -cameraVector.y > joystickGap)
        {
            if (followCamera.transform.eulerAngles.x > 5 &&
                followCamera.transform.eulerAngles.x < 45)
            {
                followCamera.transform.RotateAround(transform.position,
                    -xAxis, rotationSpeed * Time.deltaTime);
            }
            // Going Down
            else if (followCamera.transform.eulerAngles.x < 5 && -cameraVector.y > 0)
            {
                followCamera.transform.RotateAround(transform.position,
                   -xAxis, rotationSpeed * Time.deltaTime);
            }
            //Going Up
            else if (followCamera.transform.eulerAngles.x > 45 && -cameraVector.y < 0)
            {
                followCamera.transform.RotateAround(transform.position,
                   -xAxis, rotationSpeed * Time.deltaTime);
            }
        }

        offsetZ = followCamera.transform.position.z - transform.position.z;
        offsetX = followCamera.transform.position.x - transform.position.x;
        offsetY = followCamera.transform.position.y - transform.position.y;
    }

    private void PlayerControlsUpdate()
    {
        // Raycast to see if the player is touching the ground
        Ray groundRay = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        Vector3 groundRaycast = transform.position + new Vector3(0, 0.5f, 0);

        if (Physics.Raycast(groundRay, out hit))
        {
            if (hit.distance < 1)
            {
                grounded = false;
            }
            else
            {
                grounded = true;
            }
        }

        // Joystick gap ensure that the player does not accidentally touch the joystick
        if (move.x < -joystickGap || move.x > joystickGap || move.y < -joystickGap || move.y > joystickGap)
        {
            // Ensuring it takes into account the camera when moving
            cameraForward = followCamera.transform.forward;//y axis
            cameraRight = followCamera.transform.right; //x axis

            cameraForward.y = 0;
            cameraRight.y = 0;

            Vector3 forwardMovement = move.x * cameraRight;
            Vector3 rightMovement = move.y * cameraForward;

            Vector3 moveDirection = forwardMovement + rightMovement;
            playerRotation = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            if (playerRotation == 0)
            {

            }
            else
            {

                endRotation = Quaternion.Euler(new Vector3(0.0f, playerRotation, 0.0f));
                transform.rotation =
                    Quaternion.Lerp(transform.rotation, endRotation, 6 * Time.deltaTime); // Ensure this happens
                                                                                          //Debug.Log(playerRotation);

                // transform.rotation = Quaternion.Euler(new Vector3(0.0f, playerRotation, 0.0f)); // Lerp this     
            }

            if (grounded == false)
            {
                //Running
                playerAnimator.SetInteger("CurrentState", 5);
            }
            else
            {
                //Setting to random value to prevent run animation
                playerAnimator.SetInteger("CurrentState", 10);
            }


            Vector3 movement =
                new Vector3(moveDirection.x, 0.0f, moveDirection.z) *
                (speed * Time.deltaTime); // Get current rotation
            transform.Translate(movement, Space.World);
        }
        else
        {
            //Idle
            playerAnimator.SetInteger("CurrentState", 0);
        }
    }

    private void OnEnable()
    {
        controls.Player.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void OnTriggerStay(Collider other)
    {
        //Checking for Power Ups
        if (other.tag == "JumpPU")
        {
            if (action > 0)
            {
                Destroy(other.gameObject);
                jumpMax = 2;
            }
        }

        if (other.tag == "MovementPU")
        {
            if (action > 0)
            {
                Destroy(other.gameObject);
                speed = speed * 2;
                movementPU = true;
            }
        }
    }
}
