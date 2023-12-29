using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Water_Particle : NetworkBehaviour
{
    [SerializeField] Rigidbody rigid;
    private Collider[] col_arr;
    private Vector3 dis;
    private Vector3 velocity;
    private Vector3 interaction;
    private float sqr_mag;
    
    private float check_distance = 0.08f;
    private float sqr_distance_threshold;
    private float attraction_coeff = 4.0f;
    private float repulsion_coeff = 0.025f;
    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            interaction = Vector3.zero;
            sqr_distance_threshold = check_distance * check_distance;
        }
        StartCoroutine(death_count(10f));
    }

    // Update is called once per frame

    private void FixedUpdate()
    {
        if (isServer) {
            cal_interaction();
            velocity += interaction * Time.fixedDeltaTime; //분자간 인력        
            if (!cal_gounded(Time.fixedDeltaTime))
            {
                velocity -= Vector3.up * 9.8f * Time.fixedDeltaTime; //중력
            }
            /*if (velocity.sqrMagnitude > 0.001f)
            {*/
            transform.position += velocity * Time.fixedDeltaTime;
            //}

            transform.up = Vector3.up;
            velocity -= 0.3f * velocity * Time.fixedDeltaTime; // 공기저항 및 마찰력
        }       
    }
    private IEnumerator death_count(float t)
    {
        yield return new WaitForSeconds(t);
        Destroy(this.gameObject);
    }


    private void cal_interaction() {
        interaction = Vector3.zero;
        //col_arr = Physics.OverlapSphere(transform.position, 0.08f, LayerMask.GetMask("Oil"));
        col_arr = Physics.OverlapBox(transform.position,Vector3.right* 0.4f, Quaternion.identity,LayerMask.GetMask("Oil"));
        
        for (int i = 0; i < col_arr.Length; i++) {
            dis = col_arr[i].transform.position - transform.position;
            sqr_mag = dis.sqrMagnitude;
            if (sqr_mag < 0.04f) {
                if (dis.y < 0) {
                    col_arr[i].GetComponent<Water_Particle>().add_force(Vector3.Dot(dis, (9.8f * Vector3.down)) / sqr_mag * dis * Time.fixedDeltaTime );
                }
                sqr_mag = Mathf.Max(sqr_mag, 0.01f);
            }
            
            //force += (4f -  0.025f / sqr_mag)* dis.normalized / sqr_mag / 1000f; 
            interaction += (4f - 0.6f / sqr_mag) * dis / sqr_mag; // 인력, 척력

            //interaction += (4f - 0.025f / sqr_mag) * dis / sqr_mag;
        }
        interaction /= 100f;        
    }

    private bool cal_gounded(float dt) {

        if (velocity.y - 9.8f*Time.deltaTime <= 0.1f && Physics.Raycast(transform.position, Vector3.down, 0.05f + Mathf.Abs( velocity.y - 9.8f * Time.deltaTime) *Time.fixedDeltaTime, LayerMask.GetMask("Default"))) {
            velocity -= 0.95f * velocity * dt;
            velocity.y = Mathf.Max(0f, velocity.y);
            return true;
        }
        return false;
    }

    public void add_force(Vector3 dir) {
        velocity += dir;
    }
}
