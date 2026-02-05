using System;
using System.Collections;
using UnityEngine;
namespace Folded.Core
{

	public class PlayerRestrictions : MonoBehaviour
	{
        private static readonly WaitForSeconds _waitForSeconds = new(.2f);
		private static readonly KeyCode restartKey = KeyCode.R;
		private static readonly float unplayableDistance = 100f;

		[SerializeField]
		private Transform player;

		[SerializeField]
		private float resetDeadline;
		[SerializeField]
		private float immidiateResetTime;

        private void Start()
        {
			StartCoroutine(CheckOutOfBounds());
        }
        private void Update()
        {
            if(Input.GetKeyDown(restartKey))
			{
				StopAllCoroutines();
				StartCoroutine(StartRestart());
                StartCoroutine(CancelRestart());
                enabled = false;
			}
        }

        IEnumerator CheckOutOfBounds()
        {
			while(true)
            {
                yield return _waitForSeconds;
                if (player.position.magnitude > unplayableDistance)
                {
					player.GetComponent<Rigidbody>().isKinematic = true;
					player.GetComponent<Player>().enabled = false;
                    PauseMenu.main.ShouldResetText(true);
					break;
                }
            }
		}

		IEnumerator StartRestart()
		{
			PauseMenu.main.ResetingText(true, resetDeadline);

			yield return new WaitForSeconds(resetDeadline);

			StopAllCoroutines();
			PauseMenu.main.Reset();
		}

		IEnumerator CancelRestart()
		{
			while(true)
			{
				yield return new WaitForEndOfFrame();
				if (!Input.GetKey(restartKey))
					break;
			}
			StartCoroutine(ImmReset());
			yield return new WaitForSeconds(immidiateResetTime);

			StopAllCoroutines();
            StartCoroutine(CheckOutOfBounds());

			enabled = true;
            PauseMenu.main.ResetingText(false, 0f);
        }

		IEnumerator ImmReset()
		{
			while(true)
            {
                yield return new WaitForEndOfFrame();
				if (Input.GetKey(restartKey))
                {
					StopCoroutine(StartRestart());
                    PauseMenu.main.Reset();
					break;
                }
            }
		}
	}
}

