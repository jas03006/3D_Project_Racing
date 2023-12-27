using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EzySlice;
public class Slicer_Shoot : NetworkBehaviour
{
    private Transform slice_ob;
    public Material cross;
    public MeshRenderer mesh_renderer;

    public float cut_force = 10f; //잘릴 때 우아하게 보이기 위해서 힘을 줌

    public LayerMask layer;
    [SerializeField] private Transform[] pos_arr;
    private void Start()
    {
        if (isServer)
        {
            StartCoroutine(death_count(30f));
        }
    }

    public void set_material(int ind_) {
        cross = Color_Manager.instance.slicer_material_arr[ind_];
        mesh_renderer.material = cross;        
    }
    [ClientRpc]
    public void set_material_RPC(int ind_)
    {
        set_material(ind_);
        Debug.Log("slice color RPC");
    }
    private IEnumerator death_count(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(this.gameObject);
    }

    [ClientRpc]

    private void stop_emitting_RPC()
    {
        Destroy(this.gameObject);
    }
    private void Update()
    {
        if (isServer) {
            transform.position += transform.forward * Time.deltaTime * 30f;
            RaycastHit hit;
            for (int i =0; i < pos_arr.Length; i++) {
                if (Physics.Raycast(pos_arr[i].position, transform.forward, out hit, 0.5f, layer))
                {
                    if (hit.point != null )
                    {
                        slice_object(hit.transform.gameObject, hit.point);
                    }
                }
            }
            
        }
        
        
    }
    [Command]
    public void slice_object_CMD(GameObject target, Vector3 hit_position) {

    }
    [ClientRpc]
    public void slice_and_set_RPC(int p_index, Vector3 hit_position, Vector3 slice_normal, uint upper_netid, uint lower_netid)
    {
        Car[] car_arr = FindObjectsOfType<Car>();
        Car car_ = null;//= MultiManager.instance.car_list[p_index];
        for (int i =0; i < car_arr.Length; i++) {
            if (car_arr[i].player_index == p_index) {
                car_ = car_arr[i];
                break;
            }
        }
        if (car_ == null) {
            return;
        }
        GameObject car_body = car_.car_body.gameObject;
        //Debug.Log($"Slice RPC! {car_body.transform.tag}");

        SlicedHull hull = car_body.Slice(hit_position, slice_normal);
        if (hull != null)
        {
            GameObject upper_hull = hull.CreateUpperHull(car_body, cross);
            upper_hull.transform.position = car_body.transform.position;

            GameObject lower_hull = hull.CreateLowerHull(car_body, cross);
            lower_hull.transform.position = car_body.transform.position;

            car_body.SetActive(false);
            car_.set_active_wheels(false);

            GameObject lower_go = null;
            GameObject upper_go = null;

            NetworkIdentity[] net_arr = FindObjectsOfType<NetworkIdentity>();

            for (int i =0; i < net_arr.Length;i++) {
                if (lower_go != null && upper_go != null) {
                    break;
                }
                if (net_arr[i].netId == upper_netid) {
                    upper_go = net_arr[i].gameObject;
                }
                else if (net_arr[i].netId == lower_netid)
                {
                    lower_go = net_arr[i].gameObject;
                }
            }


            if (lower_go == null || upper_go == null)
            {
                Destroy(upper_hull);        
                Destroy(lower_hull);
                return;
            }

            lower_go.GetComponent<MeshFilter>().mesh = lower_hull.GetComponent<MeshFilter>().mesh;
            lower_go.GetComponent<MeshRenderer>().materials = lower_hull.GetComponent<MeshRenderer>().materials;
            
            upper_go.GetComponent<MeshFilter>().mesh = upper_hull.GetComponent<MeshFilter>().mesh;
            upper_go.GetComponent<MeshRenderer>().materials = upper_hull.GetComponent<MeshRenderer>().materials;

            if (!isServer) {
                setup_slice_component(upper_go, cut_force * slice_normal);
                setup_slice_component(lower_go, -cut_force * slice_normal);
            }

            Destroy(upper_hull);
            //Destroy(upper_go, 3.0f);            
            Destroy(lower_hull);
            //Destroy(lower_go, 3.0f);
        }
    }
    [Server]
    public void slice_object(GameObject target, Vector3 hit_position)
    {
        Car car_ = target.GetComponent<Car>();

        GameObject car_body = car_.car_body.gameObject;
        if (car_body == null || !car_body.activeSelf) {
            return;
        }
       // Debug.Log($"Slice! {car_body.transform.tag}");

        Vector3 slice_normal = transform.GetChild(0).up;

        SlicedHull hull = car_body.Slice(hit_position, slice_normal);
        if (hull != null)
        {
            GameObject upper_hull = hull.CreateUpperHull(car_body, cross);
            upper_hull.transform.position = car_body.transform.position;           

            GameObject lower_hull = hull.CreateLowerHull(car_body, cross);
            lower_hull.transform.position = car_body.transform.position;

            car_body.SetActive(false);
            car_.set_active_wheels(false);
            //car_.set_active_wheels_RPC(false);

            GameObject lower_go = Instantiate(car_.net_manager.spawnPrefabs[(int)item_index.temp], car_body.transform.position, car_body.transform.rotation);
            lower_go.GetComponent<MeshFilter>().mesh = lower_hull.GetComponent<MeshFilter>().mesh;
            lower_go.GetComponent<MeshRenderer>().materials = lower_hull.GetComponent<MeshRenderer>().materials;

            GameObject upper_go = Instantiate(car_.net_manager.spawnPrefabs[(int)item_index.temp], car_body.transform.position, car_body.transform.rotation);
            upper_go.GetComponent<MeshFilter>().mesh = upper_hull.GetComponent<MeshFilter>().mesh;
            upper_go.GetComponent<MeshRenderer>().materials = upper_hull.GetComponent<MeshRenderer>().materials;
  
            NetworkServer.Spawn(upper_go);
            NetworkServer.Spawn(lower_go);

            setup_slice_component(upper_go, cut_force * slice_normal);
            setup_slice_component(lower_go, -cut_force * slice_normal);

            Destroy(upper_hull);                       
            Destroy(lower_hull);

            Destroy(upper_go, 5.0f);
            Destroy(lower_go, 5.0f);

            slice_and_set_RPC(car_.player_index, hit_position, slice_normal, upper_go.GetComponent<NetworkIdentity>().netId, lower_go.GetComponent<NetworkIdentity>().netId);
        }

    }
   

    private void setup_slice_component(GameObject g, Vector3 force)
    {
        Rigidbody rb = g.AddComponent<Rigidbody>();
        MeshCollider mc = g.AddComponent<MeshCollider>();
        mc.convex = true;        
        rb.mass = mc.bounds.size.magnitude * 200f;
        rb.AddForce(force, ForceMode.Impulse);
        //rb.AddExplosionForce(cut_force, g.transform.position, 1f);
    }



    public void slice_object_old(GameObject target, Vector3 hit_position)
    {
        Car car_ = target.GetComponent<Car>();
        GameObject car_body = car_.car_body.gameObject;
        Debug.Log($"Slice! {car_body.transform.tag}");

        // Vector3 slice_normal = Vector3.Cross(transform.position - previous_pos, transform.forward);
        Vector3 slice_normal = transform.GetChild(0).up;
        SlicedHull hull = car_body.Slice(hit_position, slice_normal);
        if (hull != null)
        {
            GameObject upper_hull = hull.CreateUpperHull(car_body, cross);
            upper_hull.transform.position = car_body.transform.position;
            GameObject lower_hull = hull.CreateLowerHull(car_body, cross);
            lower_hull.transform.position = car_body.transform.position;

            /*if (target.transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject g = target.transform.GetChild(i).gameObject;
                    if (g.transform.CompareTag("Slice_ob"))
                    {
                        continue;
                    }
                    SlicedHull hull_c = g.Slice(slice_ob.position, slice_normal);
                    if (hull_c != null)
                    {
                        GameObject upper_hull_c = hull_c.CreateUpperHull(g, cross, upper_hull);
                        GameObject lower_hull_c = hull_c.CreateLowerHull(g, cross, lower_hull);
                    }
                }
            }*/

            //Destroy(target);


            car_body.SetActive(false);
            car_.set_active_wheels(false);
            car_.set_active_wheels_CMD(false);
            //target.GetComponent<MeshRenderer>().enabled = false;
            setup_slice_component(upper_hull, cut_force * slice_normal);
            setup_slice_component(lower_hull, cut_force * -1f * slice_normal);

            //gameObject.SetActive(false);
            Destroy(upper_hull, 3.0f);
            Destroy(lower_hull, 3.0f);
        }

    }
}
