using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track_Manager : MonoBehaviour
{
    public static Track_Manager instance = null;
    [SerializeField] private List<Transform> track_list;
    [SerializeField] private GameObject[] respawn_arr;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else {
            Destroy(this.gameObject);
            return;
        }
    }
    private void Start()
    {
        if (respawn_arr != null && respawn_arr.Length>0) {
            return;
        }
        for (int i = 0; i < transform.childCount; i++) {
            Transform tr = transform.GetChild(i);
            if (tr.CompareTag("Track")) {
                track_list.Add(tr);
            }
        }
        respawn_arr = GameObject.FindGameObjectsWithTag("Respawn_Point");        
    }

    public Transform get_nearest_track(Vector3 pos) {
        float dis_sqr = float.MaxValue;
        float temp;
        Transform result_tr = null;
        for (int i =0; i < track_list.Count; i++) {
            temp = (track_list[i].position - pos).sqrMagnitude;
            if (temp < dis_sqr) {
                dis_sqr = temp;
                result_tr = track_list[i];
            }   
        }
        return result_tr;
    }
    public KeyValuePair<Vector3,Vector3> get_nearest_respawn_point(Vector3 pos)
    {
        float dis_sqr = float.MaxValue;
        float temp;
        Transform result_tr = null;
        KeyValuePair<Vector3, Vector3> pos_for = new KeyValuePair<Vector3, Vector3>();
        for (int i = 0; i < respawn_arr.Length; i++)
        {
            temp = (respawn_arr[i].transform.position - pos).sqrMagnitude;
            if (temp < dis_sqr)
            {
                dis_sqr = temp;
                result_tr = respawn_arr[i].transform;
            }
        }
        if (result_tr !=null) {
            pos_for = new KeyValuePair<Vector3, Vector3>(result_tr.position, result_tr.transform.GetChild(1).position- result_tr.transform.GetChild(0).position);
        }
        return pos_for;
    }

    public float get_distance_from_goal(Car car_) {
        float dis_sqr = float.MaxValue;
        float temp;
        Transform result_tr = null;
        int result_ind = -1;
        for (int i = 0; i < respawn_arr.Length; i++)
        {
            temp = (respawn_arr[i].transform.position - car_.transform.position).sqrMagnitude;
            if (temp < dis_sqr)
            {
                dis_sqr = temp;
                result_tr = respawn_arr[i].transform;
                result_ind = i;
            }
        }               
        
        if (result_ind == respawn_arr.Length-1 && car_.lap_check_bool_arr[0] == true && car_.lap_check_bool_arr[1] == false)
        {
            result_ind = 0;
        }

        Vector3 dir = respawn_arr[(result_ind + 1) % respawn_arr.Length].transform.position - result_tr.position;
        float result = (respawn_arr.Length - result_ind) * 100f + Mathf.Sign(Vector3.Dot(result_tr.position - car_.transform.position, dir)) * (result_tr.position - car_.transform.position).magnitude;
        
        if (car_.lap_check_bool_arr[0] == false) {
            result += 20000f;            
        }

        return result;// + car_.get_check_point_dist_ind()*10000f;
    }
}
