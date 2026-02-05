using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class KeyofDoor : MonoBehaviour, Folded.Core.IInteractable, Folded.IFoldEffected
{
    public static int keyCount = 0;
    public static string playerTag = "Player";

    private bool interactable = true;

    private Collider col;

    private void Start()
    {
        col = GetComponent<Collider>();
    }

    public void Interact()
    {
        if(interactable)
        {
            keyCount++;

            Player.main.PlayHand(transform, 0);

            Destroy(gameObject, .1f);
        }
    }

    private void OnDestroy()
    {
        if (Folded.Core.IInteractable.lastInteractable.Contains(this))
            ((Folded.Core.IInteractable)this).RemoveInteractable();
    }

    public void FoldedOn()
    {
        if (Folded.Core.IInteractable.lastInteractable.Contains(this))
            ((Folded.Core.IInteractable)this).RemoveInteractable();
        interactable = false;
    }

    public void FoldedOff()
    {
        interactable = true;
    }
}

