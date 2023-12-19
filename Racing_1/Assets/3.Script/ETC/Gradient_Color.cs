using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Gradient_Color : MonoBehaviour
{
    public Gradient gradient;

    [Range(0, 1)]
    public float value;

    public Image img;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        img.color = gradient.Evaluate(value);
    }
}
