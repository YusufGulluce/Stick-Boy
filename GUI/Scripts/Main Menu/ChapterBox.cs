using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Folded.GUI
{
	public class ChapterBox : MonoBehaviour
	{
		[SerializeField, Tooltip("Box that chapter's icon will appear on.")]
		private Image iconBox;

		[SerializeField, Tooltip("Box that chapter's name will appear on.")]
		private Text nameText;

		private Scene goToScene; // Scene of this chapter.

		/// <summary>
		/// Set function of chapter box.
		/// </summary>
		public void Set(ChapterContents contents)
		{
			iconBox.sprite = contents.icon;
			nameText.text = contents.name;
			goToScene = contents.scene;
		}

		/// <summary>
		/// Triggers when the button of box is triggered.
		/// </summary>
		public void Pressed()
		{
			//TODO - Trigger folding animation.
			SceneManager.LoadScene(goToScene.name);
		}
    }
}

