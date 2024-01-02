using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Oil_Emit : NetworkBehaviour
{
    [SerializeField] private GameObject oil_particle;
    [SerializeField] private Transform emit_forward;
    private Vector3 emit_dir;
    
    // Start is called before the first frame update
    void Start()
    {
        if (isServer) {
            emit_dir = emit_forward.position - transform.position;
            StartCoroutine(emit_co());
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator emit_co() {
        float elapsed_time = 0f;
        while (elapsed_time < 2f) {
            instantiate_oil((emit_dir + (Vector3.right * Random.Range(-0.1f, 0.1f) + Vector3.forward * Random.Range(-0.1f, 0.1f))) * (5f - elapsed_time));
            for (int i =0; i < 1f * (1+(int)elapsed_time);i++) {
                
                yield return null;
                elapsed_time += Time.deltaTime;
            }
            yield return null;
            elapsed_time += Time.deltaTime;
        }
    }

    private void instantiate_oil(Vector3 force) {
        GameObject go = Instantiate(oil_particle , transform.position, Quaternion.identity);
        go.GetComponent<Water_Particle>().add_force(force/10f);
        NetworkServer.Spawn(go);
        //go.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
    }
}
