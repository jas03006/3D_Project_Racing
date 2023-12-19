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
        speed_text.text = string.Format("{0:###.#}",car.velocity.magnitude*10f) + " km/h";
    }

}
