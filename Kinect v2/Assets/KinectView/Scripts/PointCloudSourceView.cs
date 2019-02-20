using UnityEngine;
using System.Collections;
using Windows.Kinect;
using UnityEngine.UI;

public class PointCloudSourceView : MonoBehaviour
{
    private const int _Speed = 50;
    
    void Update()
    {
        float yVal = Input.GetAxis("Horizontal");
        float xVal = -Input.GetAxis("Vertical");

        transform.Rotate(
            (xVal * Time.deltaTime * _Speed), 
            (yVal * Time.deltaTime * _Speed), 
            0, 
            Space.Self);
    }
}
