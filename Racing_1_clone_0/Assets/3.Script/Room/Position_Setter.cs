using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Position_Setter : MonoBehaviour
{
    //[SerializeField] private NewNetworkRoomManager nrm;
    [SerializeField] public GameObject[] pos_arr;
    [SerializeField] public RectTransform[] name_tag_arr;
    [SerializeField] public GameObject[] car_arr;
    private void Start()
    {
        //nrm = FindObjectOfType<NewNetworkRoomManager>();
        if (pos_arr ==null || pos_arr.Length == 0) {
            pos_arr = GameObject.FindGameObjectsWithTag("Start_Point");
        }        
        car_arr = new GameObject[pos_arr.Length];
        /*if (nrm != null) {
            for (int i =0; i < Mathf.Min( nrm.roomSlots.Count,pos_arr.Length); i++) {
                nrm.roomSlots[i].transform.position = pos_arr[i].transform.position;
            }
        }*/
    }

    public void locate_car(GameObject car) {
        for (int i = 0; i < car_arr.Length; i++) {
            if (car_arr[i] == null) {
                car_arr[i] = car;
                car.transform.position = pos_arr[i].transform.position;
                car.GetComponentInChildren<CarNetworkRoomPlayer>().name_tag_index = i;                
                return;
            }
        }
    }

    public void remove_car(GameObject car)
    {
        for (int i = 0; i < car_arr.Length; i++)
        {
            if (car_arr[i] == car)
            {
                car_arr[i] = null;
                return;
            }
        }
    }
    public void remove_car(int index_)
    {
        car_arr[index_] = null;
        name_tag_arr[index_].position = Vector3.right * -2000f;
    }
}
