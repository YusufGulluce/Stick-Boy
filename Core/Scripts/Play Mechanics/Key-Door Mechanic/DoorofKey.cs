using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DoorofKey : MonoBehaviour
{
    public static string playerTag = "Player";
    public static KeyCode openKey = KeyCode.E;

    [SerializeField, Tooltip("This collider is not solid (trigger = true) and triggers when player enters it and enable them to open the door.")]
    private Collider ghostCollider;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(playerTag))
        {
            ShowDoorOpenability();

            if (KeyofDoor.keyCount-- > 0)
                Open();
        }   
    }

    [ContextMenu("Show Usability")]
    private void ShowDoorOpenability()
    {
        //TODO - Show if door is usable or not.
    }

    [ContextMenu("Open")]
    private void Open()
    {
        Debug.Log("Door is opened");
        //TODO - Open animation
    }
}

