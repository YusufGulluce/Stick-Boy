using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace Folded.GUI
{
    public class ChapterSelection : MonoBehaviour
    {
        public static ChapterSelection main;    //Main object that enables other classes to access.

        // Chapter Variables
        private ChapterPage currentPage;

        private void Awake()
        {
            if (main != null ||  main != this)
                main = this;
            else
                Destroy(gameObject);
        }
        private void OnDestroy()
        {
            main = null;
        }

        private List<GameObject> chapterBoxes;

        [Header("View")]
        [SerializeField]
        [Tooltip("Rect of the page that all chapter will be seen on. (Page)")]
        private RectTransform pageRect;

        [SerializeField, Tooltip("Standard prefab of chapter contents.")]
        private GameObject chapterPrefab;

        [SerializeField, Tooltip("Standard aspect ratio of prefab of chapter icons.")]
        private float iconAspectRatio;

        //[Tooltip("Use this after making changes to apply them.")]

        [SerializeField, Tooltip("Index of page to load.")]
        [ContextMenuItem("Load Page", "RearrangePageView")]
        private int pageIndex;

        [ContextMenu("Rearrange Page View")]
        public void RearrangePageView()
        {
            currentPage = chapterPages[pageIndex];

            if(chapterBoxes != null)
                foreach (GameObject obj in chapterBoxes)
                    DestroyImmediate(obj);
            chapterBoxes?.Clear();

            chapterBoxes = General.FitPrefabToRect(pageRect, iconAspectRatio, currentPage.chapters.Length, chapterPrefab);

            for(int i = 0; i < currentPage.chapters.Length; ++i)
            {
                ChapterContents currChapter = currentPage.chapters[i];
                GameObject currBox = chapterBoxes[i];

                currBox.GetComponent<ChapterBox>().Set(currChapter);

            }
        }

        [ContextMenu("Take Screenshot")]
        public void TakeScreenShot()
        {
            //SceneManager
            //Debug.Log("Scene name: " + SceneManager.GetActiveScene().name);
            FoldAnimation.FoldToScene(new Vector2(.0f, .0f), new Vector2(1f, 1f), Vector2.down, 2f, "MainMenu");
        }




        [Header("Chapters")]
        [SerializeField, Tooltip("Every page will be here.")]
        private ChapterPage[] chapterPages;

        public void SelectPage(ChapterPage chapterPage)
        {
            currentPage = chapterPage;
            RearrangePageView();
        }
    }

    [Serializable, Tooltip("A class that contains datas of chapters and view of that page.")]
	public class ChapterPage
    {
        [ContextMenu("Select Page")]
        public void SelectPage()
        {
            ChapterSelection.main.SelectPage(this);
        }

        [Header("Page View")]

        [SerializeField, Tooltip("Enables special usage of box prefab.")]
        private bool useSpecialPrefab;

        [SerializeField, Tooltip("Custimazed prefab of current page's chapters. (Use standard prefab assigned by ChapterSelection if == null)")]
        private GameObject chapterPrefab;

        [SerializeField, Tooltip("Custimazed aspect ratio of prefab of current page's chapters. (It will use standard ratio if 'iconAspectRatio <= 0')")]
        private float iconAspectRatio;

        [Header("Chapters")]

		[Tooltip("Array of chapters that will be on this page.")]
		public ChapterContents[] chapters;




    }

	[Tooltip("Struction that holds data of a chapter.")]
	[Serializable]
	public struct ChapterContents
    {
        public string name;
        public Sprite icon;
        public Scene scene;
	}
}

