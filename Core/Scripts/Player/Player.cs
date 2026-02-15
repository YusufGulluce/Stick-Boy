using System;
using System.Collections;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    public static Player main;

    Rigidbody rb;
    [HideInInspector]
    public SpriteRenderer sr;

    [Header("Ground Check Parameters")]
    [SerializeField]
    private float groundRayGap;
    [SerializeField]
    private float groundRayDistance;

    //[Header("External Components")]
    //[SerializeField]
    //private Collider[] pageMaskColliders;
    //[SerializeField]
    //private Collider collisionHolder;
    //[SerializeField]
    //private Transform frontMapTransform;


    [Header("Movement Parameters")]
    [SerializeField]
    private float speed;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float passiveDrag;
    [SerializeField]
    private float stoppingDrag;
    private float drag;

    [Space]

    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float jumpDetectLength;
    [SerializeField]
    private float jumpTreshHold;

    [Space]

    [SerializeField]
    private LayerMask jumpableLayers;
    [SerializeField]
    private float fallMult = 1.5f;

    [Space]

    [SerializeField, Range(0f, 1f), Tooltip("It is using Lerp function so value must be between 0 and 1.")]
    private float rotationSpeed;
    [SerializeField]
    private float rotationDeadline;

    [Header("Animators")]
    public ManuelAnimationClip animationClip;

    [Space]
    [SerializeField, Tooltip("Hand that will interact with things.")]
    private ManuelAnimationClip handAnimator;

    //[SerializeField]
    //private Collider colMask;


    [Space]

    [SerializeField]
    private Text fpsText;

    [Header("Controls")]
    [SerializeField]
    private PlayerControllerKeys controllers;

    private int jumpCount = 0;
    //private int keyCount;
    //private float oldXSpeed = 0f;
    //private Vector3 momentum = Vector3.zero;

    private void Awake()
    {
        if (main == null)
            main = this;

        drag = passiveDrag;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Time.timeScale > 0)
        {
            CheckAnimation();
            CheckDirection();
        }
        else
            TryToUnpause();
        if (jumpCount > 0 && CheckJumpDown())
        {
            jumpCount--;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
        }
        //
        //if(fpsText != null)
        //    fpsText.text = "10x" + (int)(.01f / Time.deltaTime);

        CheckInteraction();

        
        if ((CheckLeftUp() && rb.linearVelocity.x < 0f)
            || (CheckRightUp() && rb.linearVelocity.x > 0f))
            drag = stoppingDrag;
    }

    private void FixedUpdate()
    {
        
        float deltaTime = Time.fixedDeltaTime;
        if (CheckLeft())
        {
            if (rb.linearVelocity.x > -maxSpeed)
                rb.linearVelocity += deltaTime * speed * (rb.linearVelocity.x > 0 ? 2f: 1f) * Vector3.left;
        }
        else if (CheckRight())
        {
            if (rb.linearVelocity.x < maxSpeed)
                rb.linearVelocity += deltaTime * speed * (rb.linearVelocity.x < 0 ? 2f : 1f) * Vector3.right;
        }
        else
        {
            rb.linearVelocity -= deltaTime * drag * rb.linearVelocity.x * Vector3.right;
            if(drag > passiveDrag)
            {
                drag -= deltaTime * 10f;
                if (drag < passiveDrag)
                    drag = passiveDrag;
            }
        }

        if (rb.linearVelocity.y < 0 || !CheckJump())
        {
            rb.linearVelocity += fallMult * deltaTime * Physics.gravity;
        }

        CheckGround();
        CheckRotation();
        CheckDeadJump();
    }

    private void CheckGround()
    {
        rb.useGravity = true;

        if(rb.linearVelocity.y <= 0f)
            for(int i = -1; i < 2; i += 2)
            {
                RaycastHit[] hits = Physics.RaycastAll(transform.position + groundRayGap * i * Vector3.right, Vector2.down, groundRayDistance, jumpableLayers);

                if (hits != null && hits.Length > 0)
                {
                    bool go = false;
                    //List<string> strings = new();
                    foreach (RaycastHit hit in hits)
                    {
                        if(hit.collider.isTrigger) { break; }
                        //strings.Add(hit.collider.name);
                        CollisionMask[] masks = CollisionMask.InCollsionMask(hit.collider.transform.parent);
                        if(masks.Length <= 0)
                        {
                            go = true;
                            transform.position = new Vector3(transform.position.x, hit.point.y + groundRayDistance - .01f, transform.position.z);
                            break;
                        }
                        bool _go = true;
                        foreach (CollisionMask mask in masks)
                            if (mask && mask.Contains(hit.point))
                            {
                                _go = false;
                                break;
                            }
                        if (_go)
                        {
                            go = true;
                            transform.position = new Vector3(transform.position.x, hit.point.y + groundRayDistance - .01f, transform.position.z);
                            break;
                        }
                    }
                    //if(strings.Contains("Collision Holder Left"))
                    //    foreach(string name in strings)
                    //        Debug.Log("hi, my name is: " + name);

                    if (go)
                    {
                        drag = stoppingDrag;
                        jumpCount = 1;
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
                        rb.useGravity = false;

                        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

                        return;
                    }
                }
            }

    }

    private void CheckAnimation()
    {
        if (rb.useGravity)
        {
            if (rb.linearVelocity.y > .4f)
                animationClip.PlaySafe(4); //Up jump animation.
            else if (animationClip.currIndex != 5 && animationClip.currIndex != 6)
                animationClip.PlaySafe(5);
        }
        else if (animationClip.currIndex == 6)
            animationClip.PlaySafe(7);
        else if (!CheckLeft() && !CheckRight() && animationClip.currIndex != 0 && animationClip.currIndex != 3 && animationClip.currIndex != 7)
            animationClip.PlaySafe(3);
        else if ((CheckLeft() || CheckRight()) && (animationClip.currIndex != 1 && animationClip.currIndex != 2 && animationClip.currIndex != 7))
            animationClip.PlaySafe(1);
    }

    private void CheckDirection()
    {
        if (CheckLeftDown())
            sr.flipX = true;
        else if (CheckRightDown())
            sr.flipX = false;
        else if (CheckLeftUp() && CheckRight())
            sr.flipX = false;
        else if (CheckRightUp() && CheckLeft())
            sr.flipX = true;
    }

    private void CheckInteraction()
    {
        if (CheckInteractDown())
            if (Folded.Core.IInteractable.lastInteractable != null && Folded.Core.IInteractable.lastInteractable.Count > 0)
                Folded.Core.IInteractable.lastInteractable[^1].Interact();
    }

    private void CheckRotation()
    {
        if(transform.rotation.eulerAngles.z != 0f)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        collision.GetComponent<Folded.Core.IInteractable>()?.AddInteractable();
    }

    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<Folded.Core.IInteractable>()?.RemoveInteractable();
    }


    private float deadJumpTimer = 0f;
    private void CheckDeadJump()
    {
        if (jumpCount > 0)
        {
            if (rb.useGravity)
            {
                deadJumpTimer += Time.fixedDeltaTime;
                if (deadJumpTimer >= jumpTreshHold)
                    jumpCount = 0;
            }
            else
            {
                deadJumpTimer = 0f;
            }
        }
        else
            deadJumpTimer = 0f;
    }

    public void FixMomentum()
    {
        rb.linearVelocity = Quaternion.AngleAxis(transform.rotation.eulerAngles.z, Vector3.forward) * rb.linearVelocity;
    }

    public void PlayPlayer(int animationIndex)
    {
        animationClip.PlaySafe(animationIndex);
    }

    public void PlayHand(Transform transform, int animationIndex)
    {
        handAnimator.GetComponent<SpriteRenderer>().flipX = transform.position.x < this.transform.position.x;

        handAnimator.transform.position = transform.position;
        handAnimator.Play(animationIndex);
    }

    #region Check Key Functions
    private bool CheckJumpDown()
    {
        return Input.GetKeyDown(controllers.jump) || Input.GetKeyDown(controllers.secondaryJump);
    }
    private bool CheckJump()
    {
        return Input.GetKey(controllers.jump) || Input.GetKey(controllers.secondaryJump);
    }

    private bool CheckLeft()
    {
        return Input.GetKey(controllers.left) || Input.GetKey(controllers.secondaryLeft);
    }
    private bool CheckRight()
    {
        return Input.GetKey(controllers.right) || Input.GetKey(controllers.secondaryRight);
    }
    private bool CheckLeftUp()
    {
        return Input.GetKeyUp(controllers.left) || Input.GetKeyUp(controllers.secondaryLeft);
    }
    private bool CheckRightUp()
    {
        return Input.GetKeyUp(controllers.right) || Input.GetKeyUp(controllers.secondaryRight);
    }
    private bool CheckLeftDown()
    {
        return Input.GetKeyDown(controllers.left) || Input.GetKeyDown(controllers.secondaryLeft);
    }
    private bool CheckRightDown()
    {
        return Input.GetKeyDown(controllers.right) || Input.GetKeyDown(controllers.secondaryRight);
    }

    private bool CheckInteractDown()
    {
        return Input.GetKeyDown(controllers.interact);
    }

    #endregion Check Key Functions

    #region Pause Functions
    private void TryToUnpause()
    {
        if(PauseMenu.isPaused &&( CheckJumpDown() || CheckRightDown() || CheckLeftDown()) )
        {
            PauseMenu.main.Unpause();
            CheckDirection();
        }
    }
    #endregion Pause Functions

    #region structs
    [Serializable]
    private struct PlayerControllerKeys
    {
        [Space]

        public KeyCode left;
        public KeyCode secondaryLeft;

        [Space]

        public KeyCode right;
        public KeyCode secondaryRight;

        [Space]

        public KeyCode jump;
        public KeyCode secondaryJump;

        [Space]

        public KeyCode interact;

    }
    #endregion structs
}

namespace Folded.Core
{
    public interface IInteractable
    {
        public static List<IInteractable> lastInteractable = new();

        public void AddInteractable()
        {            
            lastInteractable.Add(this);
            ImmidiateInteract();
        }
        public void RemoveInteractable()
        {
            lastInteractable.Remove(this);
        }

        public void ImmidiateInteract();
        public void Interact();

    }
}
