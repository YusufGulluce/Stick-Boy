using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

namespace Folded.GUI
{
	public static class General
	{
        public static Vector3 CalculateBestRect(float width, float height, float ratio, int count)
        {
            Vector3 h = Vector3.zero;
            for (int i = 1; i <= count; ++i)
            {
                int x = (count / i) + (count % i);  //max horizontal column count.
                int y = i;                          //vertical column count

                float temp = width / (x * ratio);

                if (height / y < temp)
                {
                    temp = height / y;
                }
                if (temp > h.z)
                {
                    h = new(x, y, temp);
                }
            }

            return h;
        }

        public static List<GameObject> FitPrefabToRect(RectTransform parentRect, float aspectRatio, int itemCount, GameObject prefab)
        {
            Vector3 info = CalculateBestRect(parentRect.rect.width, parentRect.rect.height, aspectRatio, itemCount);
            float x = info.z / parentRect.rect.width;
            float y = info.z / parentRect.rect.height;
            float minY = 1f - ((int)info.y * y);
            float minX = 1f - ((int)info.x * x);
            minY *= .5f;
            minX *= .5f;

            List<GameObject> returnObjects = new();

            int _x = (int)info.x;
            for (int j = (int)info.y - 1; j >= 0; --j)
            {
                if (j == 0 && itemCount % _x != 0)
                {
                    _x = itemCount % _x;
                    minX = 1f - (_x * x);
                    minX *= .5f;
                }
                for (int i = 0; i < _x; ++i)
                {
                    RectTransform _rt = GameObject.Instantiate(prefab, parentRect).GetComponent<RectTransform>();

                    _rt.anchorMin = new(minX + i * x, minY + j * y);
                    _rt.anchorMax = new(_rt.anchorMin.x + x, _rt.anchorMin.y + y);
                    _rt.ForceUpdateRectTransforms();
                    returnObjects.Add(_rt.gameObject);
                }
            }

            return returnObjects;
        }


    }

    public static class FoldAnimation
    {
        private static Mesh mesh;
        private static Vector2 dir;
        private static float speed;
        private static int smoothEdges;

        //Scene swap variables
        private static Scene firstScene;
        private static Scene nextScene;
        private static Transform paper;
        private static Camera firstCam;
        private static Vector3 firstPos;

        public static void FoldToScene(Vector2 anchorMin, Vector2 anchorMax, Vector2 dir, float speed, string sceneName)
        {
            Debug.Log("firstPos: " + firstPos);

            var objects = UnityEngine.Object.FindObjectsByType(typeof(GameObject), FindObjectsSortMode.None);
            foreach(GameObject obj in objects.Cast<GameObject>())
                if (obj.GetComponent<Camera>() == null && obj.GetComponent<RectTransform>() == null)
                    GameObject.Destroy(obj);


            paper = FoldUI(anchorMin, anchorMax, dir, speed, new List<Action> { EndFirstScene });

            foreach (GameObject obj in objects.Cast<GameObject>())
                if (obj.GetComponent<RectTransform>() != null)
                    GameObject.Destroy(obj);

            firstScene = SceneManager.GetActiveScene();
            firstCam = Camera.main;
            firstPos = firstCam.transform.position;

            nextScene = SceneManager.LoadScene(sceneName, new LoadSceneParameters(LoadSceneMode.Additive));
            SceneManager.sceneLoaded += SceneLoaded;

            //Debug.Break();

        }

        private static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(scene == nextScene)
            {
                //Debug.Break();
                SceneManager.SetActiveScene(scene);

                foreach (GameObject obj in scene.GetRootGameObjects())
                {
                    //Debug.Log(obj.name + " / " + obj.transform.position);
                    if (obj.CompareTag("MainCamera"))
                    {
                        Camera cam = obj.GetComponent<Camera>();
                        firstCam.GetComponent<AudioListener>().enabled = false;
                        firstCam.gameObject.AddComponent<SceneAdapter>().Set(cam, paper);
                    }
                }

            }

        }

        [RequireComponent(typeof(Camera))]
        private class SceneAdapter : MonoBehaviour
        {
            private Camera follow;
            private Camera cam;
            private Transform paper;

            public void Set(Camera follow, Transform paper)
            {
                cam = GetComponent<Camera>();
                this.follow = follow;
                this.paper = paper;
            }
            private void Update()
            {
                transform.position = follow.transform.position;
                paper.localScale *= follow.orthographicSize / cam.orthographicSize;

                paper.localScale = (Vector3)(Vector2)paper.localScale + Vector3.forward;
                paper.transform.localPosition = new Vector3(-firstPos.x * paper.localScale.x,
                                                    -firstPos.y * paper.localScale.y, 1.4f);
                cam.orthographicSize = follow.orthographicSize;
            }
        }


        private static void EndFirstScene()
        {
            SceneManager.UnloadScene(firstScene);
        }

        public static Transform FoldUI(Vector2 anchorMin, Vector2 anchorMax, Vector2 dir, float speed, List<Action> endActs)
        {
            dir.Normalize();

            RenderTexture rt = new((int)(Screen.width * (anchorMax.x - anchorMin.x)), (int)(Screen.height * (anchorMax.y - anchorMin.y)), 0);                         // Creates Render Texture to screen shot.
            MeshFilter mf = new GameObject("Folding Object").AddComponent<MeshFilter>();    // Creates object and a mesh filter to store mesh.                     // Creates Render Texture to screen shot.
            MeshFilter bgMf = new GameObject("BG Folding Object").AddComponent<MeshFilter>();    // Creates object and a mesh filter to store mesh.
            mf.gameObject.AddComponent<MeshRenderer>().material.mainTexture = rt;           // Creates mesh renderer and apply them the texture.
            bgMf.gameObject.AddComponent<MeshRenderer>().material = LoadAssets.paperBackgroundMat;

            mf.transform.SetParent(Camera.main.transform);

            bgMf.transform.SetParent(mf.transform);
            bgMf.transform.localPosition = Vector3.zero;
            mf.GetComponent<MeshRenderer>().material.shader = Shader.Find("UI/Default");

            /*
             * Takes screenshot of the screen.
             */
            Camera secondCam = new GameObject().AddComponent<Camera>();
            secondCam.enabled = false;
            secondCam.targetTexture = rt;

            Camera cam = Camera.main;

            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = null;

            List<Vector3> verts = new();
            List<int> tris = new();
            List<Vector2> uvs = new();

            float camViewWidth = cam.orthographicSize * 2f * cam.aspect;
            float camViewHeight = cam.orthographicSize * 2f;

            Vector2 camViewOffset = new Vector2(camViewWidth * anchorMin.x, camViewHeight * anchorMin.y);
            camViewOffset += (Vector2)cam.transform.position;
            camViewOffset.x -= camViewWidth * .5f;
            camViewOffset.y -= camViewHeight * .5f;

            camViewWidth *= anchorMax.x - anchorMin.x;
            camViewHeight *= anchorMax.y - anchorMin.y;

            verts.AddRange(new List<Vector3> {camViewOffset, camViewOffset + Vector2.right * camViewWidth,
                camViewOffset + Vector2.up * camViewHeight, camViewOffset + new Vector2(camViewWidth, camViewHeight) });
            tris.AddRange(new List<int> { 2, 1, 0, 2, 3, 1});
            uvs.AddRange(new List<Vector2> { anchorMin, new Vector2(anchorMax.x, anchorMin.y), new Vector2(anchorMin.x, anchorMax.y), anchorMax});

            mesh = new();

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);

            mf.sharedMesh = mesh;
            bgMf.sharedMesh = new();

            mf.gameObject.AddComponent<FoldingMesh>().Set(mf, bgMf, dir, speed, 15, 1, secondCam.gameObject, endActs);

            return mf.transform;
        }


    }



    public class FoldingMesh : MonoBehaviour
    {
        private MeshFilter mf;
        private MeshFilter bgMf;
        private Vector2 dir;
        private float speed;
        private int curveCount;
        private float depth;
        private float _depth;

        private Vector3 corner0;
        private Vector3 corner1;
        private Vector3 _corner0;
        private Vector3 _corner1;
        private float length;

        private Vector2 uvR;
        private Vector2 uvL;
        private Vector2 _uvR;
        private Vector2 _uvL;
        private Vector2 uvDir;

        [SerializeField]
        private float updateCooldown = 1f;

        //[SerializeField, Range(-1f, 1f)]
        private float curve = 1f;

        private List<Action> endActs = new();
        private GameObject secondCam;

        public void Set(MeshFilter mf, MeshFilter bgMf, Vector2 dir, float speed, int curveCount, float depth, GameObject destroyCam, List<Action> endActs)
        {
            dir.Normalize();

            this.mf = mf;
            this.bgMf = bgMf;
            this.dir = dir;
            this.speed = speed;
            this.curveCount = curveCount;
            this.depth = depth;
            this.endActs = endActs;
            this.secondCam = destroyCam;

            uvDir = dir;
            float uvLength;

            if(dir == Vector2.down)
            {
                corner0 = mf.sharedMesh.vertices[2];
                corner1 = mf.sharedMesh.vertices[3];

                uvR = mf.sharedMesh.uv[2];
                uvL = mf.sharedMesh.uv[3];

                length = (corner0 - mf.sharedMesh.vertices[0]).y;
                uvLength = (uvR - mf.sharedMesh.uv[0]).y;
            }
            else if(dir == Vector2.up)
            {
                corner0 = mf.sharedMesh.vertices[1];
                corner1 = mf.sharedMesh.vertices[0];

                uvR = mf.sharedMesh.uv[1];
                uvL = mf.sharedMesh.uv[0];

                length = -(corner0 - mf.sharedMesh.vertices[3]).y;
                uvLength = -(uvR - mf.sharedMesh.uv[3]).y;
            }
            else if(dir == Vector2.right)
            {
                corner0 = mf.sharedMesh.vertices[0];
                corner1 = mf.sharedMesh.vertices[2];

                uvR = mf.sharedMesh.uv[0];
                uvL = mf.sharedMesh.uv[2];

                length = -(corner0 - mf.sharedMesh.vertices[1]).x;
                uvLength = -(uvR - mf.sharedMesh.uv[1]).x;

                //Debug.Log("Length: " + length + "\n UV Length: " + uvLength);

            }
            else
            {
                this.dir = Vector2.left;
                uvDir = this.dir;
                corner0 = mf.sharedMesh.vertices[3];
                corner1 = mf.sharedMesh.vertices[1];

                uvR = mf.sharedMesh.uv[3];
                uvL = mf.sharedMesh.uv[1];

                length = (corner0 - mf.sharedMesh.vertices[2]).x;
                uvLength = (uvR - mf.sharedMesh.uv[2]).x;
            }

            _corner0 = corner0;
            _corner1 = corner1;
            _uvL = uvL;
            _uvR = uvR;
            _depth = depth;

            this.dir *= length;
            this.uvDir *= uvLength;

            //Debug.Log(uvDir);

            curve = 1f;
            //StartCoroutine(UpdatingCurve());


        }

        private void FixedUpdate()
        {

            UpdateCurve(curve);

            curve -= Time.fixedDeltaTime * speed * Mathf.Log(2.1f - curve);

            if (curve < -1f)
            {
                Destroy(gameObject);
                Destroy(secondCam);

                if(endActs != null)
                    foreach(Action action in endActs)
                        action?.Invoke();
            }

        }


        IEnumerator UpdatingCurve()
        {
            while(enabled)
            {
                yield return new WaitForSeconds(updateCooldown);
                UpdateCurve(curve);
            }
        }

        public void UpdateCurve(float curve)
        {
            //Create variables to apply in mesh.
            List<Vector3> verts = new();    //Mesh corner points.
            List<int> tris = new();         //Indicates how corners connect.
            List<Vector2> uvs = new();      //Indicates how material texture is applied to each corner.


            List<Vector3> verts2 = new();
            List<int> tris2 = new();
            List<Vector2> uvs2 = new();

            //Use original variables for each curving.
            corner0 = _corner0;
            corner1 = _corner1;
            uvL = _uvL;
            uvR = _uvR;
            depth = _depth;


            
            if (depth <= 0f)
                depth = .001f;

            //Curve ending.
            if (curve < 0f)
            {
                float a = Mathf.PI * depth * .5f / dir.magnitude;
                if (curve + a < 0f) curve = -a;
                depth *= (a + curve) / a;
                curve = 0f;
            }


            //Before Curving apply flat sheet.
            verts.AddRange(new List<Vector3> {
                corner0,
                corner1,
                corner0 + (Vector3)dir * curve,
                corner1 + (Vector3)dir * curve
            });

            uvs.AddRange(new List<Vector2>
            {
                uvR,
                uvL,
                uvR + uvDir * curve,
                uvL + uvDir * curve
            });

            // During Curve
            corner0 += (Vector3)dir * curve;
            corner1 += (Vector3)dir * curve;

            uvR += uvDir * curve;
            uvL += uvDir * curve;

            // turn depth to radius and indicate where curve starts.
            float curveStart = curve * dir.magnitude;
            float radius = depth * .5f;
            depth *= Mathf.PI * .5f;

            // Set mesh on curve.
            for(float cur = curveStart; cur < curveStart + depth && cur < dir.magnitude; cur += depth / curveCount)
            {
                float l = cur - curveStart;      
                float distance = radius * Mathf.Sin(l/radius);

                float _l = 2f * radius * radius * (1f - Mathf.Cos(l/radius));
                float height = Mathf.Sqrt(_l - distance * distance);

                verts2.AddRange(new List<Vector3> { corner0 + (Vector3)dir.normalized * distance + Vector3.back * height,
                                                    corner1 + (Vector3)dir.normalized * distance + Vector3.back * height });

                uvs2.AddRange(new List<Vector2> { uvR + uvDir * l / dir.magnitude,
                                                uvL + uvDir * l / dir.magnitude });
            }
            // After curve set flat sheet.
            if (curveStart + depth < dir.magnitude)
            {
                verts2.AddRange(new List<Vector3> {
                    corner0 + 2f * radius * Vector3.back,
                    corner1 + 2f * radius * Vector3.back,
                    corner0 + 2f * radius * Vector3.back - (Vector3)dir.normalized * (dir.magnitude - curveStart - depth),
                    corner1 + 2f * radius * Vector3.back - (Vector3)dir.normalized * (dir.magnitude - curveStart - depth)
                });
                uvs2.AddRange(new List<Vector2> {
                    uvR + uvDir * depth / dir.magnitude,
                    uvL + uvDir * depth / dir.magnitude,
                    _uvR + uvDir,
                    _uvL + uvDir
                });
            }

            verts.AddRange(verts2);
            uvs.AddRange(uvs2);
            //Set triangles.
            for (int i = 0; i < verts.Count - 2; i += 2)
            {
                tris.AddRange(new List<int>
                {
                    i, i + 2, i + 1,
                    i + 1, i + 2, i + 3
                });
            }
            for (int i = 0; i < verts2.Count - 2; i += 2)
            {
                tris2.AddRange(new List<int>
                {
                    i, i + 2, i + 1,
                    i + 1, i + 2, i + 3
                });
            }

            //for (int i = 0; i < verts.Count; ++i)
            //    verts[i] += transform.position;
            //for (int i = 0; i < verts2.Count; ++i)
            //    verts2[i] += transform.position;

            mf.sharedMesh.Clear();

            mf.sharedMesh.SetVertices(verts);
            mf.sharedMesh.SetTriangles(tris, 0);
            mf.sharedMesh.SetUVs(0, uvs);

            mf.sharedMesh = mf.sharedMesh;

            //Rendering Back Of Page
            bgMf.sharedMesh.Clear();

            bgMf.sharedMesh.SetVertices(verts2);
            bgMf.sharedMesh.SetTriangles(tris2, 0);
            bgMf.sharedMesh.SetUVs(0, uvs2);

            bgMf.sharedMesh = bgMf.sharedMesh;

            //if (bgMf.sharedMesh == mf.sharedMesh)
            //    Debug.Log("a");
        }
    }



    /*
     * PROPERTY FIELDS
     */

}

