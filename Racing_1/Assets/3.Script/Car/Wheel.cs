using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public bool is_slip = false;
    public bool is_on_oil = false;
    private Coroutine current_slip_co = null;
    private float timer = 0f;
    [SerializeField]private Transform ray_pos;
    // Start is called before the first frame update

    private void FixedUpdate()
    {
        
        check_oil();
    }

    private IEnumerator slip_co() {
        is_slip = true;
        timer = 0f;
        while (timer < 2f)
        {
            yield return null;
            if (!is_on_oil) {
                timer += Time.deltaTime;
            }            
        }
        is_slip = false;
        current_slip_co = null;
    }

    private void check_oil() {
        //Debug.DrawLine(ray_pos.position, ray_pos.position - ray_pos.up*0.3f, Color.red);
        is_on_oil = false;
        if (Physics.BoxCast(ray_pos.position,Vector3.right*0.3f, -ray_pos.up,Quaternion.identity, 0.2f, LayerMask.GetMask("Oil")) )
        {
            //Debug.Log("oil!!!!");
            is_on_oil = true;
            if (current_slip_co == null)            {
                
                current_slip_co = StartCoroutine(slip_co());
            }
            
        }
    }

  /*  private void OnTriggerStay(Collider other)
    {
        
        if (!is_on_oil)
        {
            Debug.Log("check!!!!");
            if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Oil")))
            {
                Debug.Log("oil!!!!");
                is_on_oil = true;
                if (current_slip_co != null) {
                    StopCoroutine(current_slip_co);
                }
                
                current_slip_co = StartCoroutine(slip_co());
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!is_on_oil)
        {
            if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Oil")))
            {
                is_on_oil = true;
                if (current_slip_co != null)
                {
                    StopCoroutine(current_slip_co);
                }
                current_slip_co = StartCoroutine(slip_co());
            }
        }

    }*/
}
