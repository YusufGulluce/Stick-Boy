using System;
using System.Collections.Generic;
using UnityEngine;

namespace Folded.Core
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class SpriteStacker : MonoBehaviour
	{
		#region Stack Parameters
		[Header("Stack Parameters")]

		[SerializeField, Min(.01f)]
		private float depth;
        [SerializeField]
        private Vector2 size;
		[SerializeField, Tooltip("Item at index 0 is at the top.")]
		private Sprite sprite;

        [Space]

        [SerializeField]
        private LayerSetType layerSetType;
        [SerializeField, Folded.Editor.ShowIf("layerSetType", LayerSetType.Static)]
        private int layerCount;
        #endregion

        #region Objects & Components
        [Header("Objects & Components")]
        [SerializeField]
        private MeshRenderer mr;
        [SerializeField]
        private MeshFilter mf;
        #endregion

        #region Private Variables
        private Mesh mesh;
        #endregion

        #region Private Functions
        [ContextMenu("Create")]
        private void Create()
        {
            if (!mf.sharedMesh) mf.sharedMesh = new();
            mesh = mf.sharedMesh;

            if(layerSetType == LayerSetType.Dynamic)
                layerCount = sprite.texture.width / sprite.texture.height;

            mr.sharedMaterial = new Material(Shader.Find("UI/Default"));
            mr.sharedMaterial.SetTexture("_MainTex", sprite.texture);

            SetMesh();
        }

        private void SetMesh()
        {
            mesh.Clear();

            List<Vector3> verts = new();
            List<Vector2> uvs = new();
            List<int> tris = new();

            float deltaDepth = depth / (layerCount - 1);
            float _depth = -.5f * depth;
            float width = size.x * .5f;
            float height = size.y * .5f;
            float spriteGap = 1f / (float)layerCount;

            for(int i = 0; i < layerCount; ++i)
            {
                verts.AddRange( new Vector3[]
                {
                    new Vector3(-width, -height, _depth),
                    new Vector3(width, -height, _depth),
                    new Vector3(-width, height, _depth),
                    new Vector3(width, height, _depth)
                });
                uvs.AddRange(new Vector2[]
                {
                    new Vector2(i * spriteGap, 0f),
                    new Vector2((i + 1) * spriteGap, 0f),
                    new Vector2(i * spriteGap, 1f),
                    new Vector2((i + 1) * spriteGap, 1f)
                });
                tris.AddRange(new int[]
                {
                    i * 4 + 3,
                    i * 4 + 2,
                    i * 4 + 1,

                    i * 4,
                    i * 4 + 1,
                    i * 4 + 2,
                });
                _depth += deltaDepth;
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);

            mesh.Optimize();
            mf.sharedMesh = mesh;
        }
        #endregion Private Functions

        #region Public Functions
        public void SetColor(Color color)
        {
            mr.sharedMaterial.SetColor("_Color", color);
        }
        public void SetTexture(Sprite sprite)
        {
            SetTexture(sprite.texture);
        }
        public void SetTexture(Texture texture)
        {
            mr.sharedMaterial.SetTexture("_MainTex", texture);
        }
        #endregion Public Functions

        #region enums
        [Serializable, Tooltip("Indicates how layer count will be determined.")]
        private enum LayerSetType
        {
            [Tooltip("Automatically divides sprite to sub sprites. \n\n Layer count = width / height of sprite in pixels.")]
            Dynamic,
            [Tooltip("Layer count is decided manually in the inspector field.")]
            Static
        }
        #endregion
    }
}

