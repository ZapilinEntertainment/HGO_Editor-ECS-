using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform cameraBasis;
    private float xRotationSpeed = 400f, yRotationSpeed = 350f, moveSpeed = 10f;
    private float xdampening = 1f, ydampening = 1f;
    private float xdelta, ydelta;
    private const float ROTATION_ACCELERATION = 1.02f, ROTATION_DAMP_LIMIT_X = 5f, ROTATION_DAMP_LIMIT_Y = 2f;

    private void Awake()
    {
        cameraBasis = transform.parent;
        transform.LookAt(cameraBasis);
    }

    
    void Update()
    {
        float t = Time.deltaTime;
        if (Input.GetMouseButton(2))
        {
            xdelta = Input.GetAxis("Mouse X");
            ydelta = Input.GetAxis("Mouse Y");            
            if (xdelta != 0f)
            {
                cameraBasis.Rotate(Vector3.up, xdelta * xRotationSpeed * xdampening * t, Space.Self);
                if (xdampening < ROTATION_DAMP_LIMIT_X) xdampening *= ROTATION_ACCELERATION;
            }
            else xdampening = 1f;
            if (ydelta != 0f)
            {
                var dir = transform.TransformDirection(Vector3.left);
                dir = Vector3.ProjectOnPlane(dir, Vector3.up).normalized;
                transform.RotateAround(cameraBasis.position, dir, ydelta * yRotationSpeed * ydampening * t);
               if (ydampening < ROTATION_DAMP_LIMIT_Y) ydampening *= ROTATION_ACCELERATION;
            }
            else ydampening = 1f;
        }
        xdelta = Input.GetAxis("Horizontal");
        ydelta = Input.GetAxis("Vertical");
        if (xdelta != 0 || ydelta != 0)
        {
            cameraBasis.Translate(new Vector3(xdelta * t * moveSpeed, 0f, ydelta * t * moveSpeed), Space.Self);
        }
        xdelta = 0f; ydelta = 0f;
        if (Input.GetKey(KeyCode.Space)) cameraBasis.Translate(Vector3.up * moveSpeed * t, Space.World);
        else
        {
            if (Input.GetKey(KeyCode.LeftControl)) cameraBasis.Translate(Vector3.down * moveSpeed * t, Space.World);
        }
    }
}
