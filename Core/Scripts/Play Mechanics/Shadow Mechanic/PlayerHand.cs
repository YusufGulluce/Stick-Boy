using System;
using UnityEngine;
namespace Folded.Core
{
	//[RequireComponent(typeof(ManuelAnimationClip))]
	public class PlayerHand : MonoBehaviour
	{
        ///<summary>Percent speed that is accepted as nearly max speed that accelarate time tries to achive.</summary>
        private static readonly float nearlyMaxSpeed = .8f;
        public static PlayerHand main;

        #region Parameters
        [Header("Speed")]
        [SerializeField, Tooltip("Maximum velocity this hand reaches when try to interact.")]
        private float maxSpeed;
        [SerializeField, Tooltip("Time needed to reach nearly maximum velocity.")]
        private float accelarateTime;

        [Header("Key")]
        [SerializeField, Tooltip("Offset of key to player while holding one.")]
        private Vector2 keyHoldingOffset;
        [SerializeField, Tooltip("Maximum distance for hand to grab key.")]
        private float grabDeadDistance;

        //[Header("Other")]
        //[SerializeField]
        //private float updatePeriod;
        #endregion Parameters

        #region Private Variables
        //Components & Objects
        private ManuelAnimationClip animator;
        private Player player;

        //State Variables
        [HideInInspector]
        public HandState state;
        private Action stateFunction;

        //Key Grab Variables
        private KeyofDoor key;

        //Calculation Variables
        private float timer;
        private float speedMult;


        #endregion private variables

        #region Behaviour Functions
        private void Awake()
        {
            main = this;


            animator = GetComponent<ManuelAnimationClip>();

            state = HandState.None;
            stateFunction = null;

            speedMult = nearlyMaxSpeed / (accelarateTime * (1f - nearlyMaxSpeed));
            timer = 0f;
        }
        private void Start()
        {
            player = Player.main;
        }

        private void FixedUpdate()
        {
            stateFunction?.Invoke();
        }

        #endregion Behaviour Functions

        #region State Functions
        public void StartReaching(KeyofDoor key)
        {
            if(state == HandState.None)
            {
                this.key = key;
                state = HandState.Reaching;
                stateFunction = Reaching;

                transform.position = player.transform.position;
                animator.Play(0);
            }
        }
        private void Reaching()
        {
            timer += speedMult * Time.fixedDeltaTime;
            float speed = maxSpeed * timer / (timer + 1);

            transform.position += speed * (key.transform.position - transform.position).normalized;
            transform.right = (key.transform.position - transform.position).normalized;

            if ( ((Vector2)(transform.position - key.transform.position)).magnitude <= grabDeadDistance)
            {
                stateFunction = null;
                key.Collect();
                key = null;
                timer = 0f;
                animator.Play(1, 0f, ReachCallback, 2); //Grabbing Key Animation.
            }
        }
        private void ReachCallback(ManuelAnimationClip clip)
        {
            state = HandState.HoldingKey;
            stateFunction = Holding;
        }

        private void Holding()
        {

            transform.position = Vector3.Lerp(player.transform.position + (Vector3)keyHoldingOffset, transform.position, .8f);
            transform.right = Vector3.Lerp(transform.position - player.transform.position ,transform.right, .8f);
        }

        public void ClearState()
        {
            state = HandState.None;
            stateFunction = null;
            transform.position = Vector3.one * 128f;
        }
        #endregion State Functions
        #region enums
        public enum HandState
        {
            None,
            Reaching,
            HoldingKey
        }
        #endregion enums
    }
}

