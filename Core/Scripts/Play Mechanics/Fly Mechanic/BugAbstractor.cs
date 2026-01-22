using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Folded.Core
{
	public abstract class BugAbstractor : MonoBehaviour, Folded.IFoldEffected
	{
        /*
		 * STATIC VARIABLES
		 */

        /// <summary>All bugs in the active scenes which are enabled are restored here.</summary>
        private static List<BugAbstractor> allBugs = new();

        [SerializeField, Tooltip("Smash tiles of bugs will be applied to this map.")]
        private Tilemap tileMap;
        /*
         * SPECIAL VARIABLES
         */
        /// <summary>Coordinate Change Per Second</summary>
        public float speed;
      
        [SerializeField, Tooltip("Tiles that this bug is using when it is smashed.")]
        private RuleTile tileSet;

        /*
         * COMPONENTS
         */
        private SpriteRenderer sr;
        private Rigidbody rb;




        /*
         * TIME FUNCTIONS
         */
        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody>();

            OverStart();
        }
        
        private void OnEnable()
        {
            allBugs.Add(this);  //Add this bug to all bug list when it is enabled.
        }

        private void OnDisable()
        {
            allBugs.Remove(this);   //Remove this bug from all bug list when it is disabled.
        }

        /*
         * BUG FUNCTIONS
         */
        public void Smash()
        {
            Vector3Int cellPos = tileMap.WorldToCell(transform.position);
            tileMap.SetTile(cellPos, tileSet);
            ////TODO - Sound and (maybe) visual effects that occurs when bug is smashed.
            Destroy(gameObject);
            Debug.Log("smashed");
        }

        /*
         * OVERRIDE FUNCTIONS
         */
        public abstract void OverStart();

        public void FoldedOnFull()
        {
            Smash();
        }

        public void FoldedOnHalf()
        {
        }
    }
}

