using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DriveTimer : MonoBehaviour
{
    [SerializeField] private Text m_text;
    [SerializeField] private Text s_text;
    [SerializeField] private Text lap_text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (MultiManager.instance.local_car == null) {
            return;
        }
        float t = MultiManager.instance.local_car.drive_time;
        int m =(int) (t / 60f);
        m_text.text = string.Format("{0:00}", (int)m);
        s_text.text = string.Format("{0:00.000}", (t - m*60));
        lap_text.text = $"{MultiManager.instance.local_car.lap_cnt}";
       
    }
}
