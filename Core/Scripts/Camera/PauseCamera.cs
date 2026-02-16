using UnityEngine;
using System.Collections;

public class PauseCamera : MonoBehaviour
{
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime_01 = new(.01f);
    private float startSize;
    private Vector3 startPos;
    private float size;

    private Vector3 aim;
    private float speed;

    private float timer;
    public void Set(Vector3 pos, float size, float speed)
    {
        pos.z = transform.position.z;
        aim = pos;
        this.size = size;
        this.speed = speed;
        timer = 0f;

        startPos = transform.position;
        startSize = Camera.main.orthographicSize;

        //StartCoroutine(ZoomOut());
    }


    private void Update()
    {
        float x = speed * timer * timer;
        x /= x + 1f;

        Camera.main.orthographicSize = Mathf.Lerp(startSize, size, x);
        transform.position = Vector3.Lerp(startPos, aim, x);

        if (x > .95f)
            enabled = false;
        timer += Time.unscaledDeltaTime;

    }

    IEnumerator ZoomOut()
    {
        while(true)
        {
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, speed);
            if ((transform.position - aim).magnitude < .1f)
                break;
            yield return _waitForSecondsRealtime_01;
        }
    }
}
