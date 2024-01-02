using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Minimap_Manager : MonoBehaviour
{
    public static Minimap_Manager instance = null;
    [SerializeField] private Car local_car;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject minimap_point_prefab;

    [SerializeField] private List<GameObject> minimap_point_list;
    [SerializeField] private List<Transform> car_list;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this);
        }
        else {
            Destroy(this);
        }
    }
    private void Start()
    {
        cam = GetComponent<Camera>();
        cam.enabled = true;
    }
    private void Update()
    {         
        update_map();
    }

    public void regist_car(Car car_) {
        car_list.Add(car_.transform);
        GameObject go = Instantiate(minimap_point_prefab);
        minimap_point_list.Add(go);
        go.GetComponent<MeshRenderer>().material = Color_Manager.instance.minimap_material_arr[(int)car_.material_index];
    }

    private void update_map()
    {
        for (int i =0; i<car_list.Count; i++) {
            if (car_list[i] == null) {
                car_list.RemoveAt(i);
                Destroy(minimap_point_list[i]);
                minimap_point_list.RemoveAt(i);                continue;
            }
            minimap_point_list[i].transform.position = new Vector3(car_list[i].position.x, 100f, car_list[i].position.z);  
        }
    }

    private void find_local_car()
    {
        Car[] cars = GameObject.FindObjectsOfType<Car>();
        for (int i = 0; i < cars.Length; i++)
        {
            if (cars[i].isLocalPlayer)
            {
                local_car = cars[i];
                return;
            }
        }
    }
}
