using UnityEngine;

public class GlitchSceneSpinner : MonoBehaviour
{
    public Vector3 spinAxis = Vector3.up;
    public float spinSpeed = 30f;
    public bool swingDirection = false;
    
    void Update()
    {
        float speed = spinSpeed;
        if (swingDirection)
        {
            speed *= Mathf.Sin(Time.time * 2f);
        }
        transform.Rotate(spinAxis, speed * Time.deltaTime);
    }
}
