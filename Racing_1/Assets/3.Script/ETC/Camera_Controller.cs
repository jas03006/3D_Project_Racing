using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class Camera_Controller : MonoBehaviour
{
    [SerializeField] private Transform car_camera_point = null;
    [SerializeField] private Transform car_camera_aim_point = null;

    [SerializeField] private Transform start_camera_point = null;
    [SerializeField] private Transform start_camera_point2 = null;
    [SerializeField] private Transform start_camera_aim_point = null;
    [SerializeField] private bool is_side_cam = false;
    public bool is_ready = false;
    private void Start()
    {
        
        GetComponent<Camera>().enabled = true;
    }
    private void Update()
    {
        if (!is_ready) {
            return;
        }

        if (car_camera_aim_point == null)
        {
            find_local_car();
            return;
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
                    car_camera_aim_point = cars[i].get_camera_back_aim_point();                    
                }
                else
                {
                    car_camera_point = cars[i].get_camera_point();
                    car_camera_aim_point = cars[i].get_camera_aim_point();
                }

                transform.SetParent(car_camera_point);
                transform.localPosition = Vector3.zero;
                transform.LookAt(car_camera_aim_point.position);
                return;
            }
        }
    }
    public void start_cam_move() {
        StartCoroutine(start_cam_move_co());
    }
    public IEnumerator start_cam_move_co()
    {
        transform.position = start_camera_point.position;
        
        float elapsed_time = 0f;
        while(elapsed_time < 1f){
            elapsed_time += Time.deltaTime;
            transform.RotateAround(start_camera_aim_point.position, Vector3.up, Time.deltaTime * 20f);
            transform.LookAt(start_camera_aim_point.position);
            yield return null;
        }
        transform.position = start_camera_point2.position;
        while (elapsed_time < 2f)
        {
            elapsed_time += Time.deltaTime;
            transform.RotateAround(start_camera_aim_point.position, Vector3.up, Time.deltaTime * 20f);
            transform.LookAt(start_camera_aim_point.position);
            yield return null;
        }
        is_ready = true;
    }
}
