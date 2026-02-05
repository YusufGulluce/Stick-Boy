using UnityEngine;
using System;
using System.Collections;

public class ManuelAnimationClip : MonoBehaviour
{
    [Serializable]
    public struct AnimationPiece
    {
        public string name;
        public int FPS;
        public bool loop;
        public bool visibleAfterStop;

        public int callBackIndex;
        public Action callOnEnd;
        public Sprite[] sprites;
    }
    [SerializeField]
    private float animationSpeed = 1f;
    [SerializeField]
    private AnimationPiece[] animations;

    [SerializeField]
    private bool playOnAwake;

    private float timer;
    private float frameTime;
    private int index;
    public AnimationPiece selectedAnim;

    [HideInInspector]
    public int currIndex;

    private SpriteRenderer sr;

    public event Action<ManuelAnimationClip> OnEnd;

    public int callAnimationOnEnd = -1;
    private float nextAnimationTimer = 0f;

    private void Awake()
    {

        selectedAnim = animations[0];
        frameTime = 1f / (selectedAnim.FPS * animationSpeed);
        timer = frameTime;
        index = 0;
        enabled = playOnAwake;

        sr = GetComponent<SpriteRenderer>();
    }

    /// <summary>Checks if the animation is currently playing, if it is then doesn't play.</summary>
    public void PlaySafe(int index)
    {
        if (currIndex == index)
            return;
        Play(index);
    }

    public void Play(int index)
    {
        nextAnimationTimer = 0f;
        callAnimationOnEnd = -1;
        currIndex = index;
        selectedAnim = animations[index];
        frameTime = 1f / (selectedAnim.FPS * animationSpeed);
        timer = frameTime;
        this.index = 0;

        enabled = true;
    }

    public void Play(string name)
    {
        for(int i = 0; i < animations.Length; ++i)
        {
            ref AnimationPiece ani = ref animations[i];
            if (ani.name == name)
            {
                currIndex = i;
                selectedAnim = ani;
                break;
            }
        }
        frameTime = 1f / (selectedAnim.FPS * animationSpeed);
        timer = frameTime;
        index = 0;

        enabled = true;
    }

    public void Play(int index, float time)
    {
        Play(index);
        nextAnimationTimer = time;
    }
    public void Play(int index, float time, Action<ManuelAnimationClip> callBackAction, int callBackAnimation)
    {
        Play(index);
        nextAnimationTimer = time;
        OnEnd += callBackAction;
        callAnimationOnEnd = callBackAnimation;
    }

    public void Play()
    {
        enabled = true;
    }

    public void Pause()
    {
        enabled = false;
    }

    public void Stop()
    {
        timer = frameTime;
        index = 0;
        enabled = false;


        if (callAnimationOnEnd >= 0)
        {
            Play(callAnimationOnEnd);
            callAnimationOnEnd = -1;
        }
        else
        {
            if (selectedAnim.callBackIndex >= 0)
                Play(selectedAnim.callBackIndex);
            if (!selectedAnim.visibleAfterStop)
                sr.sprite = null;
        }

        OnEnd?.Invoke(this);
        OnEnd = null;


    }

    private void Update()
    {
        timer += Time.deltaTime;
        if(nextAnimationTimer > 0f)
        {
            nextAnimationTimer -= Time.deltaTime;
            if (nextAnimationTimer <= 0f)
            {
                Stop();
                return;
            }

        }
        if(timer >= frameTime)
        {
            sr.sprite = selectedAnim.sprites[index];

            timer = 0f;
            ++index;
            index %= selectedAnim.sprites.Length;
            if (index == 0 && !selectedAnim.loop)
                Stop();
        }
    }

    public ref AnimationPiece GetAnimation(int index)
    {
        return ref animations[index];
    }

    public int GetAnimationIndex()
    {
        return currIndex;
    }
}

