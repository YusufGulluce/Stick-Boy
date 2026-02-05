using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Folded.Editor
{
	//[ExecuteInEditMode]
	public class PageEditor : MonoBehaviour
	{
        #region General Parameters

        [Header("General Parameters")]
		[SerializeField, Tooltip("Arrange type of playground.")]
		private PageArrangeType arrangeType;

        #endregion General Parameters
        #region View Parameters

        [Header("View Parameters")]
		[SerializeField, Tooltip("Decides how paper will look")]
		private PageViewType viewType;

        [SerializeField, Tooltip("X value determines width, y determines height of the playgorund grid.")]
        private Vector2Int pageSize;

        [Space]

        [SerializeField, ShowIf("arrangeType", PageArrangeType.Horizantal, PageArrangeType.Vertical),
            Tooltip("Decides middle page that player can not fold into.")]
        private MiddlePageType middlePageType;

        [SerializeField, ShowIf("middlePageType", MiddlePageType.Ringed, MiddlePageType.Blank),Tooltip("X value determines width, y determines height of the playgorund grid.")]
        private float middlePageLength;

        #endregion View Parameters
        #region Components & Objects

        [Header("Components & Objects")]
        [SerializeField, Tooltip("Main Page Transform")]
        private Transform page;

        [SerializeField, Tooltip("Ring Object")]
        [ShowIf("middlePageType", MiddlePageType.Ringed)]
        private BoxCollider ringsCollider;

        [Space]        

        [SerializeField, ShowIf("arrangeType", PageArrangeType.Horizantal),Tooltip("Left Page Components and Objects")]
        private PageStruct leftPage;
        [SerializeField, ShowIf("arrangeType", PageArrangeType.Horizantal), Tooltip("Right Page Components and Objects")]
        private PageStruct rightPage;

        [Space]
        [Header("Tile Sets")]
        [SerializeField]
        private RuleTile blankMap;
        [SerializeField]
        private RuleTile linedMap;
        [SerializeField]
        private RuleTile checkredMap;

        #endregion Components & Objects
        #region Behaviour Functions
        private void Start()
        {
			Debug.Log("started");
        }

        [ContextMenu("Apply")]
        private void ApplyChanges()
        {
            RearrangeMiddlePage();
            middlePageLength = 0f;
            RearrangePage(rightPage, 1);
            RearrangePage(leftPage, -1);
        }
        #endregion Behaviour Functions
        #region Arrangment Functions
        private void RearrangePage(PageStruct page, int dir)    //dir = -1 => left|down page, dir = 1 => right|up page
        {
            RearrangeFrontPage(page.frontPage, dir);
            RearrangeBackPage(page.backPage, dir);
        }
        private void RearrangeFrontPage(FrontPageStruct page, int dir)
        {
            Vector3 gridCellSize = Vector3.Scale(page.backgroundGrid.cellSize, page.backgroundGrid.transform.localScale);
            Vector3 pageSize = new(gridCellSize.x * this.pageSize.x, gridCellSize.y * this.pageSize.y, 1f);

            if (middlePageType == MiddlePageType.None) middlePageLength = 0f;

            Vector3 gapVector = arrangeType switch
            {
                PageArrangeType.Horizantal => new Vector3( middlePageLength * .5f + pageSize.x * .5f, 0f, 0f) * dir,
                PageArrangeType.Vertical => new Vector3(0f, middlePageLength * .5f + pageSize.y * .5f, 0f) * dir,
                PageArrangeType.OnePage => Vector3.zero,
                _ => Vector3.zero
            };
            if (dir > 0)
                rightPage.main.localPosition = gapVector;
            else
                leftPage.main.localPosition = gapVector;

            page.collisionMask.localScale = pageSize;

            //Begin to organize Tilemaps.
            page.backgroundMap.ClearAllTiles();

            RuleTile ruleTile = viewType switch
            {
                PageViewType.Blank => blankMap,
                PageViewType.Lined => linedMap,
                PageViewType.Checkered => checkredMap,
                _ => blankMap
            };

            Vector3Int offset = (Vector3Int)this.pageSize / 2;
            offset *= -1;

            float xAnchor = this.pageSize.x % 2 == 0 ? .5f : 0f;
            float yAnchor = this.pageSize.y % 2 == 0 ? .5f : 0f;
            page.backgroundMap.tileAnchor = new Vector3(xAnchor, yAnchor, 0f);

            //Ground Map Adjustments
            page.groundMap.ClearAllTiles();
            page.backgroundMap.tileAnchor = page.backgroundMap.tileAnchor;

            for (int i = 0; i < this.pageSize.x; ++i)
                for (int j = 0; j < this.pageSize.y; ++j)
                {
                    page.backgroundMap.SetTile(offset + new Vector3Int(i, j, 0), ruleTile);
                    page.groundMap.SetTile(offset + new Vector3Int(i, j, 0), ruleTile);
                }
            page.groundMap.transform.localPosition = new Vector3(UnityEngine.Random.Range(0f, .1f), UnityEngine.Random.Range(0f, .1f), 0f);
        }
        private void RearrangeBackPage(BackPageStruct page, int dir)
        {
            Vector3 gridCellSize = Vector3.Scale(page.backgroundGrid.cellSize, page.backgroundGrid.transform.localScale);
            Vector3 pageSize = new(gridCellSize.x * this.pageSize.x, gridCellSize.y * this.pageSize.y, 1f);

            if (middlePageType == MiddlePageType.None) middlePageLength = 0f;

            Vector3 gapVector = arrangeType switch
            {
                PageArrangeType.Horizantal => new Vector3(middlePageLength * .5f + pageSize.x * .5f, 0f, 0f) * dir,
                PageArrangeType.Vertical => new Vector3(0f, middlePageLength * .5f + pageSize.y * .5f, 0f) * dir,
                PageArrangeType.OnePage => Vector3.zero,
                _ => Vector3.zero
            };

            page.collisionMask.localScale = pageSize;
            page.grappableAreas.localScale = pageSize;
            page.spriteMask.localScale = pageSize;

            //Begin to organize Tilemaps.
            page.backgroundMap.ClearAllTiles();

            page.page.localPosition = arrangeType switch
            {
                PageArrangeType.Horizantal => new Vector3(pageSize.x * dir, 0f, 0f),
                PageArrangeType.Vertical => new Vector3(0f, pageSize.y * dir, 0f),
                PageArrangeType.OnePage => new Vector3(0f, 0f, 0f),
                _ => Vector3.zero
            };

            RuleTile ruleTile = viewType switch
            {
                PageViewType.Blank => blankMap,
                PageViewType.Lined => linedMap,
                PageViewType.Checkered => checkredMap,
                _ => blankMap
            };

            Vector3Int offset = (Vector3Int)this.pageSize / 2;
            offset *= -1;

            float xAnchor = this.pageSize.x % 2 == 0 ? .5f : 0f;
            float yAnchor = this.pageSize.y % 2 == 0 ? .5f : 0f;
            page.backgroundMap.tileAnchor = new Vector3(xAnchor, yAnchor, 0f);

            for (int i = 0; i < this.pageSize.x; ++i)
                for (int j = 0; j < this.pageSize.y; ++j)
                    page.backgroundMap.SetTile(offset + new Vector3Int(i, j, 0), ruleTile);
        }
        private void RearrangeMiddlePage()
        {
            if(ringsCollider)
            {
                Vector3 gridCellSize = Vector3.Scale(rightPage.frontPage.backgroundGrid.cellSize, rightPage.frontPage.backgroundGrid.transform.localScale);
                Vector3 pageSize = new(gridCellSize.x * this.pageSize.x, gridCellSize.y * this.pageSize.y, 1f);

                ringsCollider.transform.position = Vector3.zero;
                ringsCollider.transform.localScale = new(4f, 4f, 1f);
                ringsCollider.size = arrangeType switch
                {
                    PageArrangeType.Horizantal => new Vector3(middlePageLength / ringsCollider.transform.localScale.x, pageSize.y / ringsCollider.transform.localScale.y, 10f),
                    PageArrangeType.Vertical => new Vector3(pageSize.x / ringsCollider.transform.localScale.x, middlePageLength / ringsCollider.transform.localScale.y, 10f),
                    _ => Vector3.zero
                };

                ringsCollider.GetComponent<SpriteRenderer>().size = ringsCollider.size;

                foreach (FoldController controller in FoldController.pages)
                    controller.foldableDeadzone = middlePageLength * .5f;
            }
        }

        #endregion Arrangment Functions
        #region Enums
        private enum PageViewType
		{
			Blank,
			Lined,
			Checkered
		}
		private enum PageArrangeType
		{
			///<summary>Left and right pages appear.</summary>
			Horizantal,
            ///<summary>Up and down pages appear.</summary>
            Vertical,
            ///<summary>One paper that can be folded form anywhere appears.</summary>
            OnePage
        }
        private enum MiddlePageType
        {
            None,
            Blank,
            Ringed
        }
        #endregion Enums
        #region Structions
        [Serializable]
        private struct PageStruct
        {
            [SerializeField, Tooltip("Page Transform")]
            public Transform main;
            [SerializeField]
            public FrontPageStruct frontPage;
            [SerializeField]
            public BackPageStruct backPage;
            [SerializeField]
            public Transform pageMask;
            
        }

        [Serializable]
        private struct FrontPageStruct
        {
            [SerializeField, Tooltip("Front Page Transform")]
            public Transform page;

            [Space]

            [SerializeField, Tooltip("Grid of Background Paper.")]
            public Grid backgroundGrid;
            [SerializeField, Tooltip("Tilemap of Background Paper.")]
            public Tilemap backgroundMap;

            [Space]

            [SerializeField, Tooltip("Grid of Obstacles.")]
            public Grid obstacleGrid;
            [SerializeField, Tooltip("Tilemap of Obstacles.")]
            public Tilemap obstacleMap;

            [Space]

            [SerializeField, Tooltip("Paper Tilemap thats behind the front page.")]
            public Tilemap groundMap;

            [SerializeField, Tooltip("Collision mask that will ignore collisions in back page.")]
            public Transform collisionMask;
        }


        [Serializable]
        private struct BackPageStruct
        {
            [SerializeField, Tooltip("Back Page Transform")]
            public Transform page;

            [Space]

            [SerializeField, Tooltip("Grid of Background Paper.")]
            public Grid backgroundGrid;
            [SerializeField, Tooltip("Tilemap of Background Paper.")]
            public Tilemap backgroundMap;

            [Space]

            [SerializeField, Tooltip("Grid of Obstacles.")]
            public Grid obstacleGrid;
            [SerializeField, Tooltip("Tilemap of Obstacles.")]
            public Tilemap obstacleMap;

            [Space]

            [SerializeField, Tooltip("Collision mask that will ignore collisions in back page.")]
            public Transform collisionMask;

            [Space]

            [SerializeField, Tooltip("Grappable Areas that will start folding operation.")]
            public Transform grappableAreas;

            [Space]

            [SerializeField, Tooltip("Sprite mask that will hide player and other objects in certain stations.")]
            public Transform spriteMask;
            #endregion Structions
        }

    }
}

