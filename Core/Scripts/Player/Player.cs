using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Player : MonoBehaviour
{
    Rigidbody rb;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float drag;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float jumpDetectLength;
    [SerializeField]
    private float jumpTreshHold;

    [SerializeField]
    private Collider colMask;

    [SerializeField]
    private LayerMask jumpableLayers;
    [SerializeField]
    private float fallMult = 1.5f;

    [SerializeField]
    private Text fpsText;

    //private int keyCount;
    private int jumpCount = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //keyCount = 0;
    }

    private void Update()
    {
        if (jumpCount > 0 && Input.GetKeyDown(KeyCode.W))
        {
            jumpCount--;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
        }
        //
        if(fpsText != null)
            fpsText.text = "" + (int)(1 / Time.deltaTime);
    }
    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            if (rb.linearVelocity.x > -maxSpeed)
                rb.linearVelocity -= deltaTime * speed * Vector3.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (rb.linearVelocity.x < maxSpeed)
                rb.linearVelocity -= deltaTime * speed * Vector3.left;
        }
        else
        {
            rb.linearVelocity -= deltaTime * drag * rb.linearVelocity.x * Vector3.right;
        }

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += fallMult * deltaTime * Physics.gravity;
        }

        bool hit = Physics.Raycast(transform.position, Vector3.down, jumpDetectLength, jumpableLayers);
        if (hit && jumpCount <= 0)
        {
            jumpCount = 1;
        }
        else if (!hit && jumpCount > 0)
        {
            StartCoroutine(AfterJump());
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 15)
        {
            PauseMenu.main.WinScreenPop();
        }
    }

    IEnumerator AfterJump()
    {
        yield return new WaitForSeconds(jumpTreshHold);
        jumpCount--;
    }
}
