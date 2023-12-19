using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Camera_Controller : MonoBehaviour
{
    [SerializeField] private Transform car_camera_point = null;
    [SerializeField] private Transform car_camera_aim_point = null;
    [SerializeField] private bool is_side_cam = false;
    private void Start()
    {
        
        GetComponent<Camera>().enabled = true;
    }
    private void Update()
    {

        if (car_camera_aim_point == null)
        {
            find_local_car();
            return;
        }
        else {
            transform.LookAt(car_camera_aim_point.position, Vector3.up);
        }
    }


    private void find_local_car()
    {
        Car[] cars = GameObject.FindObjectsOfType<Car>();
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].isLocalPlayer)
            {
                if (is_side_cam)
                {
                    car_camera_point = cars[i].get_camera_side_point();
                }
                else {
                    car_camera_point = cars[i].get_camera_point();                    
                }
                car_camera_aim_point = cars[i].get_camera_aim_point();
                transform.SetParent(car_camera_point);
                transform.localPosition = Vector3.zero;
                transform.LookAt(car_camera_aim_point.position);
                return;
            }
        }
    }
}
