using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class KeyofDoor : MonoBehaviour
{
    public static int keyCount = 0;
    public static string playerTag = "Player";

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered");
        if(other.CompareTag(playerTag))
        {
            keyCount++;
            Destroy(gameObject);
        }   
    }
}

