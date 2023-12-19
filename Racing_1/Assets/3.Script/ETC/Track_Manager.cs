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
}
