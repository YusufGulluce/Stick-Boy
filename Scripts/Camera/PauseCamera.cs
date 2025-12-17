using UnityEngine;
using System.Collections;

public class PauseCamera : MonoBehaviour
{
    private Vector3 aim;
    private float size;
    public void Set(Vector3 pos)
    {
        pos.z = transform.position.z;
        aim = pos;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, aim, .1f);
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, 25f, .1f);
        if ((transform.position - aim).magnitude < .1f)
            enabled = false;
    }
}
