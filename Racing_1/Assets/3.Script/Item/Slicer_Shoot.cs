using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using EzySlice;
public class Slicer_Shoot : NetworkBehaviour
{
    private Transform slice_ob;
    public Material cross;

    public float cut_force = 10f; //잘릴 때 우아하게 보이기 위해서 힘을 줌

    public LayerMask layer;
    private void Start()
    {
        if (isServer)
        {
            StartCoroutine(death_count(5f));
        }
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
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 0.5f, layer))
        {
            slice_object(hit.transform.gameObject, hit.point);
            /*if (Vector3.Angle(transform.position - previous_pos, hit.transform.up) >= 130f)
            {
                slice_object(hit.transform.gameObject);
            }*/
        }
        
    }
 

    public void slice_object(GameObject target, Vector3 hit_position)
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

    private void setup_slice_component(GameObject g, Vector3 force)
    {
        Rigidbody rb = g.AddComponent<Rigidbody>();
        MeshCollider mc = g.AddComponent<MeshCollider>();
        mc.convex = true;
        rb.AddForce(force, ForceMode.Impulse);
        //rb.AddExplosionForce(cut_force, g.transform.position, 1f);
    }
}
