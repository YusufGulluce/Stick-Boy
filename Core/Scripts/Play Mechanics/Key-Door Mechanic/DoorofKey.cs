using UnityEngine;
using System;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(Collider))]
public class DoorofKey : MonoBehaviour, Folded.Core.IInteractable, Folded.IFoldEffected
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1);
    public static string playerTag = "Player";
    public static KeyCode openKey = KeyCode.E;

    private bool interactable = true;

    [SerializeField]
    private bool open;
    [SerializeField]
    private bool onFront = true;

    [Space]

    [SerializeField]
    private DoorMode mode;

    [Space]

    [SerializeField, Folded.Editor.ShowIf("mode", DoorMode.ToDoor)]
    private DoorofKey otherDoor;
    [SerializeField, Folded.Editor.ShowIf("mode", DoorMode.ToScene)]
    private string sceneName;

    [Space]
    [SerializeField]
    private ManuelAnimationClip animator;

    [SerializeField, Tooltip("This collider is not solid (trigger = true) and triggers when player enters it and enable them to open the door.")]
    private Collider ghostCollider;

    [ContextMenu("Show Usability")]
    private void ShowDoorOpenability()
    {
        //TODO - Show if door is usable or not.
    }

    private void OnValidate()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sortingLayerName = onFront ? "map layer" : "Default";
        sr.sortingOrder = onFront ? -15: 0;
    }

    private void Open(Player target)
    {
        //target.transform.position = new (transform.position.x, transform.position.z, target.transform.position.z);

        target.PlayPlayer(8); //Play portal animation.
        target.GetComponent<Rigidbody>().isKinematic = true;
        target.enabled = false;
        target.animationClip.OnEnd += EndOpen;
    }
    private void EndOpen(ManuelAnimationClip target)
    {
        target.Play(9); //Play reverse portal animation.
        switch (mode)
        {
            case DoorMode.ToDoor:
                StartCoroutine(EnablePlayer(target));
                if (otherDoor && otherDoor.mode == DoorMode.OutDoor)
                {
                    target.transform.position = new Vector3(otherDoor.transform.position.x, otherDoor.transform.position.y, target.transform.position.z);
                    otherDoor.ExitFrom();
                }
                
                break;
            case DoorMode.ToScene:
                Folded.GUI.FoldAnimation.FoldToScene(Vector2.zero, Vector2.one, Vector2.right, 2f, sceneName);
                break;
        }
    }
    IEnumerator EnablePlayer(ManuelAnimationClip target)
    {
        yield return _waitForSeconds1;
        target.GetComponent<Player>().enabled = true;
        target.GetComponent<Rigidbody>().isKinematic = false;
    }

    public void ExitFrom()
    {
        animator.PlaySafe(3); // Out Animation.
    }

    //IInteractables
    public void Interact()
    {
        Debug.Log("interacted dorr");
        if((interactable && mode != DoorMode.OutDoor && onFront)
            || (!onFront && mode != DoorMode.OutDoor && !FoldController.OnDesk(transform.position)))
        {
            ShowDoorOpenability();
            if (open)
                Open(Player.main);
            else if (KeyofDoor.keyCount > 0)
            {
                --KeyofDoor.keyCount;
                open = true;
                animator.PlaySafe(1);
                Folded.Core.PlayerHand.main.ClearState();
            }
        }
    }

    public void ImmidiateInteract()
    {
    }

    //IFoldEffecteds
    public void FoldedOn()
    {
        if(onFront)
        {
            if (Folded.Core.IInteractable.lastInteractable.Contains(this))
                ((Folded.Core.IInteractable)this).RemoveInteractable();
            interactable = false;
        }
    }

    public void FoldedOff()
    {
        interactable = true;
    }

    private void OnDestroy()
    {
        if (Folded.Core.IInteractable.lastInteractable.Contains(this))
            ((Folded.Core.IInteractable)this).RemoveInteractable();
    }

    [Serializable]
    private enum DoorMode
    {
        ToDoor,
        ToScene,
        OutDoor
    }
}

