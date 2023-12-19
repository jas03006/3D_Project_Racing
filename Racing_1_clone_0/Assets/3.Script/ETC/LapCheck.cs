using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapCheck : MonoBehaviour
{
    [SerializeField] private Text lap_text;
    [SerializeField] private int now_lap = 0;
    [SerializeField] private LapCheckLine[] check_list;
    // Start is called before the first frame update
    void Start()
    {
        now_lap = 0;
        check_list = GetComponentsInChildren<LapCheckLine>();
    }

    public void check_lap() {
        for (int i = 0; i < check_list.Length; i++) {
            if (check_list[i]) { 
            
            }
        }
    }
}
