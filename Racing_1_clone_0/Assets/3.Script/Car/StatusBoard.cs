using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusBoard : MonoBehaviour
{
    [SerializeField] private Rigidbody car;
    [SerializeField] private Text speed_text;

    private void Update()
    {
        if (car != null)
        {
            speed_text.text = string.Format("{0:###.#}", car.velocity.magnitude * 7.5f) + " km/h";
        }
        else {
            find_local_car();
        }        
    }


    private void find_local_car() {
        Car[] cars = GameObject.FindObjectsOfType<Car>();
        for (int i =0; i < cars.Length; i++)
        {
            if (cars[i].isLocalPlayer) {
                car = cars[i].gameObject.GetComponent<Rigidbody>();
                return;
            }
        }
    }
}
