using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Smoke_Screen :NetworkBehaviour
{
    [SerializeField] ParticleSystem particle_system;
    // Start is called before the first frame update
    private void Start()
    {
        if (isServer)
        {
            StartCoroutine(death_count(20f));
        }
    }
    private IEnumerator death_count(float t) {
        yield return new WaitForSeconds(t);
        particle_system.Stop();
        stop_emitting_RPC();
        /*yield return new WaitForSeconds(4f);
        Destroy(this.gameObject);*/
    }

    [ClientRpc]

    private void stop_emitting_RPC()
    {
        particle_system.Stop();
    }

}
