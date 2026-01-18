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
    [Range(0f, 1f)]
    private float smooth;

    private bool rePositioning = false;
    private float timer;

    // Update is called once per frame
    private void Start()
    {
        main = this;
    }
    void FixedUpdate()
    {
        if(rePositioning)
        {
            Vector3 aimPos = new(0,0,transform.position.z);
            aimPos.x = Mathf.Lerp(transform.position.x, Mathf.Min(Mathf.Max(player.position.x, pages[0].position.x), pages[1].position.x), smooth);
            aimPos.y = Mathf.Lerp(transform.position.y, pages[0].position.y, smooth);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 15f, smooth);
            transform.position = aimPos;

            timer += Time.fixedDeltaTime;
            if(timer >= reTime)
            {
                timer = 0f;
                rePositioning = false;
                Camera.main.orthographicSize = 15f;
            }
        }
        else
        {
            
            Vector3 pos = transform.position;
            pos.x = Mathf.Lerp(pos.x, Mathf.Min(Mathf.Max(player.position.x, pages[0].position.x), pages[1].position.x), smooth);
            transform.position = pos;
        }
    }

    public void RePosition()
    {
        rePositioning = true;
    }

    private void OnDestroy()
    {
        main = null;
    }
}
