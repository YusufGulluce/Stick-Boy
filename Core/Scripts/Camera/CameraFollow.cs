using UnityEngine;
using System.Collections;
using static UnityEditor.PlayerSettings;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow main;

    [SerializeField]
    private Transform[] pages;
    [SerializeField]
    private Transform player;

    [SerializeField]
    private float size;
    [SerializeField, Tooltip("Time for camera to finish its adjusment.")]
    private float reTime;

    [SerializeField]
    private Vector2 offset;
    [SerializeField]
    private Vector2 deadBox;

    [SerializeField]
    [Range(0f, 1f)]
    private float smooth;

    private bool rePositioning = false;
    private float timer;
    private Vector2 maxBorder;
    private Vector2 minBorder;

    private Vector2 lastPos;


    // Update is called once per frame
    private void Start()
    {
        rePositioning = false;
        main = this;
    }
    void FixedUpdate()
    {
        minBorder = FoldController.minBorder;
        minBorder.y += Camera.main.orthographicSize;
        minBorder.x += Camera.main.orthographicSize * Camera.main.aspect;

        minBorder -= Vector2.one;

        maxBorder = FoldController.maxBorder;
        maxBorder.y -= Camera.main.orthographicSize;
        maxBorder.x -= Camera.main.orthographicSize * Camera.main.aspect;

        maxBorder += Vector2.one;

        int camDirX = transform.position.x > player.position.x ? 1 : -1;
        int camDirY = transform.position.y > player.position.y + offset.y ? 1 : -1;

        if(rePositioning)
        {
            Vector3 pos = transform.position;

            pos.x = Mathf.Lerp(transform.position.x, lastPos.x, smooth);
            pos.y = Mathf.Lerp(transform.position.y, lastPos.y, smooth);
            transform.position = pos;
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, smooth);
        }
        else if(player != null)
        {
            Vector3 pos = transform.position;

            if (Mathf.Abs(pos.x - player.position.x) > deadBox.x)
                pos.x = Mathf.Lerp(transform.position.x, Mathf.Min(Mathf.Max(player.position.x + offset.x + deadBox.x * camDirX, minBorder.x), maxBorder.x), smooth);
            if (Mathf.Abs(pos.y - player.position.y - offset.y) > deadBox.y)
                pos.y = Mathf.Lerp(transform.position.y, Mathf.Min(Mathf.Max(player.position.y + offset.y + deadBox.y * camDirY, minBorder.y), maxBorder.y), smooth);
            transform.position = pos;

            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, smooth);
        }
    }

    public void RePosition()
    {
        StartCoroutine(Repostion());
    }

    private void OnDestroy()
    {
        if(main == this)
            main = null;
    }

    private void OnDisable()
    {
        int camDirX = transform.position.x > player.position.x ? 1 : -1;
        int camDirY = transform.position.y > player.position.y + offset.y ? 1 : -1;

        lastPos.x = Mathf.Min(Mathf.Max(player.position.x + offset.x + deadBox.x * camDirX, minBorder.x), maxBorder.x);
        lastPos.y = Mathf.Min(Mathf.Max(player.position.y + offset.y + deadBox.y * camDirY, minBorder.y), maxBorder.y);
        StopAllCoroutines();
        rePositioning = false;
    }

    IEnumerator Repostion()
    {
        enabled = true;
        rePositioning = true;
        yield return new WaitForSeconds(.3f);
        rePositioning = false;
    }
}
