using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

public class FrontPageArea : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Collider2D[] backWalls;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == player)
        {
            foreach (Collider2D wall in backWalls)
                wall.enabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == player)
        {
            foreach (Collider2D wall in backWalls)
                wall.enabled = true;
        }
    }
}
