using UnityEngine;
using System.Collections;

public class PauseCamera : MonoBehaviour
{
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime_01 = new(.01f);
    private float size;
    private Vector3 aim;
    private float speed;
    public void Set(Vector3 pos, float size, float speed)
    {
        pos.z = transform.position.z;
        aim = pos;
        this.size = size;
        this.speed = Mathf.Min(Mathf.Max(speed, 0f), 1f);

        StartCoroutine(ZoomOut());
    }


    IEnumerator ZoomOut()
    {
        while(true)
        {
            transform.position = Vector3.Lerp(transform.position, aim, speed);
            Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, size, speed);
            if ((transform.position - aim).magnitude < .1f)
                break;
            yield return _waitForSecondsRealtime_01;
        }
    }
}
