using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Folded
{
	public class ObstacleEditor : MonoBehaviour
	{
		[SerializeField, Tooltip("Obstacle Tilemap")]
		private Tilemap tilemap;

		private void SetColliders()
		{
			tilemap.CompressBounds();
		}
	}
}

