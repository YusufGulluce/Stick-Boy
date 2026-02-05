using System;
using UnityEngine;
namespace Folded.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Shadow : MonoBehaviour, IPauseEffected
    {
		[SerializeField]
		private SpriteRenderer originalRenderer;
        private SpriteRenderer shadowRenderer;

        private void OnEnable()
        {
            IPauseEffected.all.Add(this);
        }
        private void OnDisable()
        {
            IPauseEffected.all.Remove(this);
        }

        private void Start()
        {
            shadowRenderer = GetComponent<SpriteRenderer>();
            shadowRenderer.flipX = originalRenderer.flipX;
            shadowRenderer.enabled = false;

        }

        public void Pause()
        {
            shadowRenderer.enabled = true;
            shadowRenderer.sprite = originalRenderer.sprite;
            shadowRenderer.flipX = originalRenderer.flipX;
        }

        public void Resume()
        {
            shadowRenderer.enabled = false;
        }
    }
}

