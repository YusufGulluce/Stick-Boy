using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Folded.Editor
{
	public class PageEditor : MonoBehaviour
	{
        #region Shared Variables
        public static PageEditor main;
        [HideInInspector]
        public Vector2 pageCenter;
        [HideInInspector]
        public Vector2 size;
        #endregion Shared Variables

        #region General Parameters



        [Header("General Parameters")]
		[SerializeField, Tooltip("Arrange type of playground.")]
		private PageArrangeType arrangeType;
        [SerializeField, Tooltip("Show type of playground.")]
        private PageShowType showType;
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

        [SerializeField, ShowIf("arrangeType", PageArrangeType.Horizantal, PageArrangeType.Vertical),Tooltip("Left Page Components and Objects")]
        private PageStruct leftPage;
        [SerializeField, ShowIf("arrangeType", PageArrangeType.Horizantal, PageArrangeType.Vertical), Tooltip("Right Page Components and Objects")]
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
        private void Awake()
        {
            main = this;

            ApplyChanges();
        }

        private void OnDestroy()
        {
            if (main == this)
                main = null;
        }

        [ContextMenu("Apply")]
        private void ApplyChanges()
        {
            RearrangeMiddlePage();
            RearrangePage(rightPage, 1);
            RearrangePage(leftPage, -1);


            size = leftPage.backPage.collisionMask.lossyScale;

            if (showType == PageShowType.Both)
                switch (arrangeType)
                {
                    case PageArrangeType.Horizantal:
                        size.x *= 2f;
                        break;
                    case PageArrangeType.Vertical:
                        size.y *= 2f;
                        break;
                }
            switch (showType)
            {

                case PageShowType.Both:
                    leftPage.main.gameObject.SetActive(true);
                    rightPage.main.gameObject.SetActive(true);

                    pageCenter = transform.position;
                    break;
                case PageShowType.Left:
                    leftPage.main.gameObject.SetActive(true);
                    rightPage.main.gameObject.SetActive(false);

                    pageCenter = leftPage.main.position;
                    break;
                case PageShowType.Right:
                    leftPage.main.gameObject.SetActive(false);
                    rightPage.main.gameObject.SetActive(true);

                    pageCenter = rightPage.main.position;
                    break;
                case PageShowType.None:
                    leftPage.main.gameObject.SetActive(false);
                    rightPage.main.gameObject.SetActive(false);

                    pageCenter = Vector3.zero;
                    break;
            }
        }
        #endregion Behaviour Functions
        #region Arrangment Functions
        private void RearrangePage(PageStruct page, int dir)    //dir = -1 => left|down page, dir = 1 => right|up page
        {
            page.controller.foldDirection = arrangeType switch
            {
                PageArrangeType.Vertical => new(0, -dir),
                _=> new(-dir, 0)
            };
            RearrangeFrontPage(page.frontPage, dir);
            RearrangeBackPage(page.backPage, dir);
            RearrangePageMasks(page);
        }
        private void RearrangeFrontPage(FrontPageStruct page, int dir)
        {

            Vector3 gridCellSize = Vector3.Scale(page.backgroundGrid.cellSize, page.backgroundGrid.transform.localScale);
            Vector3 pageSize = new(gridCellSize.x * this.pageSize.x, gridCellSize.y * this.pageSize.y, 1f);

            page.frontCol.size = Vector3.Scale(pageSize, new(1f,1f,10f));
            if (middlePageType == MiddlePageType.None) middlePageLength = 0f;

            Vector3 gapVector = arrangeType switch
            {
                PageArrangeType.Horizantal => new Vector3( pageSize.x * .5f, 0f, 0f) * dir,
                PageArrangeType.Vertical => new Vector3(0f, pageSize.y * .5f, 0f) * dir,
                PageArrangeType.OnePage => Vector3.zero,
                _ => Vector3.zero
            };
            if (dir > 0)
                rightPage.main.localPosition = gapVector;
            else
                leftPage.main.localPosition = gapVector;

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

            page.collisionMask.localScale = pageSize;
            page.grappables.transform.localScale = pageSize;

            for(int i = 0; i < 3; ++i)
            {
                page.grappables[i].transform.localPosition = arrangeType switch
                {
                    PageArrangeType.Vertical => new((i - 1) * .5f, dir * -.5f, 0f),
                    _ => new(dir * -.5f, (i - 1) * .5f, 0f)
                } ;
                page.grappables[i].foldDirection = arrangeType switch
                {
                    PageArrangeType.Vertical => new(1 - i, -dir),
                    _=> new(-dir, 1 - i)
                };
            }

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
                Vector3 pageSize = arrangeType switch
                {
                    PageArrangeType.Vertical => new(gridCellSize.x * this.pageSize.y, gridCellSize.y * this.pageSize.x, 1f),
                    _ => new(gridCellSize.x * this.pageSize.x, gridCellSize.y * this.pageSize.y, 1f)
                };

                ringsCollider.transform.localPosition = Vector3.zero;
                ringsCollider.transform.localScale = new(4f, 4f, 1f);
                ringsCollider.size = arrangeType > PageArrangeType.Vertical ? Vector3.zero : new Vector3(middlePageLength / ringsCollider.transform.localScale.x, pageSize.y / ringsCollider.transform.localScale.y, 10f);
                
                if (arrangeType == PageArrangeType.Vertical)
                    ringsCollider.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                else
                    ringsCollider.transform.rotation = Quaternion.identity;

                ringsCollider.GetComponent<SpriteRenderer>().size = ringsCollider.size;

                foreach (FoldController controller in FoldController.pages)
                    controller.foldableDeadzone = middlePageLength * .5f;
            }
        }
        private void RearrangePageMasks(PageStruct page)
        {
            page.pageMask.localPosition = page.backPage.page.localPosition;
            page.pageMask.localScale = page.backPage.collisionMask.localScale;

            page.pageMask.localScale = Vector3.Scale(arrangeType switch
            { PageArrangeType.Vertical => new(4f, 1f, 1f), _ => new(1f, 4f, 1f) }, page.backPage.collisionMask.localScale);

            page.secondaryPageMask.localPosition = arrangeType switch
            {
                PageArrangeType.Vertical => new(0, 1),
                _ => new(1, 0)
            };
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
        [Serializable]
        private enum PageShowType
        {
            Both,
            Left,
            Right,
            None
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
            public FoldController controller;
            [SerializeField]
            public FrontPageStruct frontPage;
            [SerializeField]
            public BackPageStruct backPage;
            [SerializeField]
            public Transform pageMask;
            [SerializeField]
            public Transform secondaryPageMask;
            
        }

        [Serializable]
        private struct FrontPageStruct
        {
            [SerializeField, Tooltip("Front Page Transform")]
            public Transform page;

            [Space]

            [SerializeField, Tooltip("Main Front Page Collider")]
            public BoxCollider frontCol;

            [Space]

            [SerializeField, Tooltip("Grid of Background Paper.")]
            public Grid backgroundGrid;
            [SerializeField, Tooltip("Tilemap of Background Paper.")]
            public Tilemap backgroundMap;

            [Space]

            [SerializeField, Tooltip("Paper Tilemap thats behind the front page.")]
            public Tilemap groundMap;
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

            [SerializeField, Tooltip("Collision mask that will ignore collisions in front page.")]
            public Transform collisionMask;

            [Space]

            [SerializeField, Tooltip("Grappable Areas that will start folding operation.")]
            public GrappablesStruct grappables;

            [Space]

            [SerializeField, Tooltip("Sprite mask that will hide player and other objects in certain stations.")]
            public Transform spriteMask;
        }

        [Serializable]
        private struct GrappablesStruct
        {
            public Transform transform;


            public FoldArea this[int index]
            {
                readonly get { if (index == 0) return bot; else if (index == 1) return mid; else return top; }
                set { if (index == 0) bot = value; else if (index == 1) mid = value; else top = value; }
            }

            [Space]

            public FoldArea top;
            public FoldArea mid;
            public FoldArea bot;
        }


        #endregion Structions

        //private void OnValidate()
        //{
        //    ApplyChanges();
        //}
    }
}

