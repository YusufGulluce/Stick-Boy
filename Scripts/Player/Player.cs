using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private Collider colMask;


    [SerializeField]
    private LayerMask jumpableLayers;

    //private int keyCount;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //keyCount = 0;
    }
    private void Update()
    {
        float deltaTime = Time.deltaTime;
        if(Input.GetKey(KeyCode.A))
        {
            if(rb.velocity.x > -maxSpeed)
                rb.velocity -= deltaTime * speed * Vector3.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (rb.velocity.x < maxSpeed)
                rb.velocity -= deltaTime * speed * Vector3.left;
        }
        else
        {
            rb.velocity -= deltaTime * drag * rb.velocity.x * Vector3.right;
        }
        if (Input.GetKeyDown(KeyCode.Space) && Physics.OverlapBox(transform.position - Vector3.up * .3f, transform.localScale * .4f, transform.rotation, jumpableLayers).Length > 0)
        {
            rb.velocity += jumpForce * Vector3.up;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer == 15)
        {
            PauseMenu.main.WinScreenPop();
        }
    }
        //}
        //private void OnTriggerExit2D(Collider2D collision)
        //{
        //    if (collision.gameObject.layer == 7)
        //        gameObject.layer = 8;
        //}

        //private void OnCollisionEnter2D(Collision2D collision)
        //{
        //    if (collision.gameObject.tag == "door" && keyCount > 0)
        //    {
        //        keyCount--;
        //        Destroy(collision.gameObject);
        //    }
        //}
}
