using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Item_Box : NetworkBehaviour
{
    [SerializeField] GameObject box;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, Time.deltaTime * 70f, 0f);        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isServer) {
            Debug.Log("Item Box Trigger!");
            Car car_ = other.gameObject.GetComponentInParent<Car>();
            if (car_ != null && box.activeSelf)
            {
                car_.get_item_TRPC((item_index)Random.Range(0, 3));
                StartCoroutine(hide(20f));
                hide_RPC(20f);
            }
        }        
    }
    [ClientRpc]
    private void hide_RPC(float t) {
        StartCoroutine(hide(t));
    }

    private IEnumerator hide(float t) {
        box.SetActive(false);
        yield return new WaitForSeconds(t);
        box.SetActive(true);
    }
}
