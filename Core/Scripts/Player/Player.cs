using System.Collections;
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

    [SerializeField]
    private float groundRayGap;
    [SerializeField]
    private float groundRayDistance;
    private bool onGround = false;

    [SerializeField]
    private Collider[] pageMaskColliders;
    [SerializeField]
    private Collider collisionHolder;
    [SerializeField]
    private Transform frontMapTransform;

    [Space]

    [SerializeField]
    private float speed;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
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
    public ManuelAnimationClip animationClip;

    [Space]
    [SerializeField, Tooltip("Hand that will interact with things.")]
    private ManuelAnimationClip handAnimator;

    //[SerializeField]
    //private Collider colMask;

    [Space]

    [SerializeField]
    private LayerMask jumpableLayers;
    [SerializeField]
    private float fallMult = 1.5f;

    [Space]

    [SerializeField]
    private Text fpsText;

    //private int keyCount;
    private int jumpCount = 0;
    private float oldXSpeed = 0f;

    private void Awake()
    {
        if (main == null)
            main = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if(Time.timeScale > 0)
        {
            CheckAnimation();
            CheckDirection();
        }
        if (jumpCount > 0 && Input.GetKeyDown(KeyCode.W))
        {
            jumpCount--;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
        }
        //
        if(fpsText != null)
            fpsText.text = "" + (int)(1 / Time.deltaTime);

        CheckInteraction();
    }

    private void FixedUpdate()
    {
        
        float deltaTime = Time.fixedDeltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            if (rb.linearVelocity.x > -maxSpeed)
                rb.linearVelocity -= (rb.linearVelocity.x > 0f ? 2f: 1f) * deltaTime * speed * Vector3.right ;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (rb.linearVelocity.x < maxSpeed)
                rb.linearVelocity -= (rb.linearVelocity.x < 0f ? 2f : 1f) * deltaTime * speed * Vector3.left;
        }
        else
        {
            rb.linearVelocity -= deltaTime * drag * rb.linearVelocity.x * Vector3.right;
        }

        if (rb.linearVelocity.y < 0 || !Input.GetKey(KeyCode.W))
        {
            rb.linearVelocity += fallMult * deltaTime * Physics.gravity;
        }

        CheckGround();
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
                            if (mask && mask.Contains(hit.transform.position))
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
        else if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && animationClip.currIndex != 0 && animationClip.currIndex != 3 && animationClip.currIndex != 7)
            animationClip.PlaySafe(3);
        else if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && (animationClip.currIndex != 1 && animationClip.currIndex != 2 && animationClip.currIndex != 7))
            animationClip.PlaySafe(1);
    }

    private void CheckDirection()
    {
        if (Input.GetKeyDown(KeyCode.A))
            sr.flipX = true;
        else if (Input.GetKeyDown(KeyCode.D))
            sr.flipX = false;
        else if (Input.GetKeyUp(KeyCode.A) && Input.GetKey(KeyCode.D))
            sr.flipX = false;
        else if (Input.GetKeyUp(KeyCode.D) && Input.GetKey(KeyCode.A))
            sr.flipX = true;
    }

    private void CheckInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
            if (Folded.Core.IInteractable.lastInteractable != null && Folded.Core.IInteractable.lastInteractable.Count > 0)
                Folded.Core.IInteractable.lastInteractable[^1].Interact();
    }

    private void OnTriggerEnter(Collider collision)
    {
        collision.GetComponent<Folded.Core.IInteractable>()?.AddInteractable();
    }

    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<Folded.Core.IInteractable>()?.RemoveInteractable();
    }


    IEnumerator AfterJump()
    {
        yield return new WaitForSeconds(jumpTreshHold);
        jumpCount--;
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
}

namespace Folded.Core
{
    public interface IInteractable
    {
        public static List<IInteractable> lastInteractable = new();

        public void AddInteractable()
        {
            lastInteractable.Add(this);
        }
        public void RemoveInteractable()
        {
            lastInteractable.Remove(this);
        }

        public void Interact();
    }
}
