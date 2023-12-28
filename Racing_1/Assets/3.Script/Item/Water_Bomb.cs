using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Water_Bomb : NetworkBehaviour
{

    [SerializeField] GameObject bomb_go;
    [SerializeField] GameObject fly_go;

    [Server]
    public void target(Vector3 pos) {
        StartCoroutine(target_co(pos));
    }

    public IEnumerator target_co(Vector3 pos)
    {
        Vector3 dir;
        while (true)
        {
            dir = pos - transform.position;
            if (dir.magnitude <= 1f)
            {
                boom(4.3f);
                boom_RPC(4.3f);
                break;
            }
            transform.position += Vector3.Lerp(Vector3.zero, dir, 0.5f) * Time.deltaTime * 10f;
            yield return null;
        }
    }

    public void chase_frotier(Transform target) {
        StartCoroutine(chase_co(target));
    }

    [Server]
    public IEnumerator chase_co(Transform target) {
        Vector3 dir;
        float mag;
        while (true) {
            dir = target.position + target.transform.forward * 2f - transform.position ;
            mag = dir.magnitude;
            if (mag <= 2f)
            {
                boom(4.3f);
                boom_RPC(4.3f);
                break;
            }
            transform.position += dir.normalized * Mathf.Max(mag,50f) * Time.deltaTime;
            yield return null;
        }
    }
    public void boom(float t) {
        fly_go.SetActive(false);
        bomb_go.SetActive(true);
        StartCoroutine(boom_co(t));
    }
    [ClientRpc]
    public void boom_RPC(float t)
    {
        boom(t);
    }

    public IEnumerator boom_co(float t) {
        float elapsed_time = 0f;
        float temp_x;
        if (t < 2.3f) {
            t = 2.3f;
        }
        while(elapsed_time < 0.7f){
            elapsed_time += Time.deltaTime;
            temp_x = 2.5f*elapsed_time/0.7f + 0.05f;
            transform.localScale += Vector3.one * Time.deltaTime * 5f / temp_x;//(Mathf.Log((t/20f+1f)/ (1f+elapsed_time/20f), 2f));
            yield return null;
        }
        yield return new WaitForSeconds(t-2.3f);
        while (elapsed_time < 1.6f)
        {
            elapsed_time += Time.deltaTime;
            temp_x = 1f * elapsed_time / t + 0.05f;
            if (transform.localScale.x <= 0f) {
                break;
            }
            transform.localScale -= Vector3.one * Time.deltaTime * 5f / temp_x;//(Mathf.Log((t/20f+1f)/ (1f+elapsed_time/20f), 2f));
            yield return null;
        }
        Destroy(this.gameObject);
    }
}
