using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Folded.Core
{
	public abstract class BugAbstractor : MonoBehaviour
	{
        /*
		 * STATIC VARIABLES
		 */

        /// <summary>All bugs in the active scenes which are enabled are restored here.</summary>
        private static List<BugAbstractor> allBugs = new();
        /// <summary>Smash tiles of bugs will be applied to this map.</summary>
        private static Tilemap tileMap;
        /*
         * SPECIAL VARIABLES
         */
        /// <summary>Coordinate Change Per Second</summary>
        private float speed;
        /// <summary>Tiles that this bug is using when it is smashed.</summary>
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
            //TODO - Sound and (maybe) visual effects that occurs when bug is smashed.
            Destroy(gameObject);
        }
    }
}

