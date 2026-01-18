using System;
using UnityEngine;

namespace Folded.GUI
{

    public class LoadAssets : MonoBehaviour
    {
        public static Material paperBackgroundMat;

        [SerializeField]
        private Material paperBackground;

        private void Start()
        {
            paperBackgroundMat = paperBackground;
            Destroy(this);

        }

    }
}

