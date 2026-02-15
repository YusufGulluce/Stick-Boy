using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class KeyofDoor : MonoBehaviour, Folded.Core.IInteractable, Folded.IFoldEffected
{
    [SerializeField]
    private bool onFront = true;

    public static int keyCount = 0;
    public static string playerTag = "Player";

    private bool interactable = true;

    private Collider col;

    private void Start()
    {
        col = GetComponent<Collider>();
    }

    private void OnValidate()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sortingLayerName = onFront ? "map layer" : "Default";
        sr.sortingOrder = onFront ? -15 : 0;
    }
    public void Interact()
    {
        if(interactable || (!onFront && !FoldController.OnDesk(transform.position)))
        {
            if(Folded.Core.PlayerHand.main &&
                Folded.Core.PlayerHand.main.state == Folded.Core.PlayerHand.HandState.None)
                Folded.Core.PlayerHand.main.StartReaching(this);
        }
    }

    public void Collect()
    {
        ++keyCount;
        Destroy(gameObject, .1f);
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

    public void ImmidiateInteract()
    {
        Interact();
    }
}

