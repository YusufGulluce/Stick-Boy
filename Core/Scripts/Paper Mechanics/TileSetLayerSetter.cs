using System;
using UnityEditor.Tilemaps;
using UnityEngine;
namespace Folded.Core
{
	[RequireComponent(typeof(TileSet))]
	public class TileSetLayerSetter : MonoBehaviour
	{
        private void Start()
        {
            for (int i = 0; i < transform.childCount; ++i)
                transform.GetChild(i).gameObject.layer = gameObject.layer;
        }
    }
}

