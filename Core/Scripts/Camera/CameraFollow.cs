using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow main;

    [SerializeField]
    private Transform[] pages;
    [SerializeField]
    private Transform player;

    [SerializeField]
    private float size;
    [SerializeField]
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

    // Update is called once per frame
    private void Start()
    {
        rePositioning = false;
        main = this;
    }
    void FixedUpdate()
    {
        if(rePositioning)
        {
            Vector3 aimPos = new(0, 0, transform.position.z)
            {
                x = Mathf.Lerp(transform.position.x, Mathf.Min(Mathf.Max(player.position.x, pages[0].position.x), pages[1].position.x), smooth),
                y = Mathf.Lerp(transform.position.y, player.position.y + 3f, smooth)
            };
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, smooth);
            transform.position = aimPos;

            timer += Time.fixedDeltaTime;
            if(timer >= reTime)
            {
                timer = 0f;
                rePositioning = false;
                Camera.main.orthographicSize = size;
            }
        }
        else if(player != null)
        {
            Vector3 pos = transform.position;
            int camDirX = pos.x > player.position.x ? 1 : -1;
            int camDirY = pos.y > player.position.y + offset.y ? 1 : -1;

            if (Mathf.Abs(pos.x - player.position.x) > deadBox.x)
                pos.x = Mathf.Lerp(pos.x, Mathf.Min(Mathf.Max(player.position.x + deadBox.x * camDirX, pages[0].position.x), pages[1].position.x), smooth);
            if (Mathf.Abs(pos.y - player.position.y - offset.y) > deadBox.y)
                pos.y = Mathf.Lerp(pos.y, player.position.y + deadBox.y * camDirY + offset.y, smooth * .2f);
            transform.position = pos;
        }
    }

    public void RePosition()
    {
        rePositioning = true;
    }

    private void OnDestroy()
    {
        if(main == this)
            main = null;
    }
}
