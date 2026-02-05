using System;
using System.Collections;
using UnityEngine;

namespace Folded.GUI
{
	[RequireComponent(typeof(Camera))]
	public class DeskCam : MonoBehaviour
	{
        [Header("Angle Contraints")]

        [SerializeField, Tooltip("Maximum Vertical Angle Camera Can See")]
        private float maxVerticalAngle;

        [Space]

        [SerializeField, Tooltip("Maximum Horizantal Angle Camera Can See")]
        private float maxHorizontalAngle;

        [Header("Looking Contraints")]
        [SerializeField, Tooltip("Sensivity setting of cam to look around.")]
        private float sensivity;


        private Camera cam;

        private void Start()
        {
            cam = GetComponent<Camera>();
        }

        private void Update()
        {
            Vector3 rot = Input.mousePosition;
            rot.x /= Screen.width * .5f;
            rot.x -= 1f;

            rot.y /= Screen.height * -.5f;
            rot.y += 1f;

            rot.z = 0f;

            rot.x *= maxVerticalAngle * (sensivity / (Mathf.Abs(rot.x * sensivity) + 1));
            rot.y *= maxHorizontalAngle * (sensivity / (Mathf.Abs(rot.y * sensivity) + 1));
            (rot.y, rot.x) = (rot.x, rot.y);
            cam.transform.eulerAngles = rot;
        }

        public void Enable()
        {
            enabled = true;
            StopAllCoroutines();
        }

        public void AdjustTo(Vector3 direction, float time)
        {
            enabled = false;

            direction.Normalize();

            StartCoroutine(Adjusting(direction, time));
        }

        IEnumerator Adjusting(Vector3 direction, float time)
        {
            for(float t = 0f; t < time; t += Time.deltaTime)
            {
                cam.transform.forward = Vector3.Lerp(cam.transform.forward, direction, .1f);
                yield return new WaitForEndOfFrame();
            }
            cam.transform.forward = direction;
        }
    }
}

