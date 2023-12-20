using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class MultiManager : NetworkBehaviour
{
    public static MultiManager instance = null;
    [SerializeField] private Text count_down_text;
    public Car local_car { get; private set; } = null;
    public List<Car> car_list { get; private set; } = null;

    public int current_player_cnt { get; private set; } = 0;
    public bool is_start;

    public float timer { get; private set; } = 0f;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else {
            Destroy(this);
            return;
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        car_list = new List<Car>();
    }
    private void Start()
    {
        
        count_down_text = GameObject.FindGameObjectWithTag("Count_Down_Text").GetComponent<Text>();
        is_start = false;
        StartCoroutine( race_start_co());
    }
    private void Update()
    {
        if (isServer && is_start)
        {
            update_timer();
        }
    }
    [Server]
    private void update_timer() {
        timer += Time.deltaTime;
        foreach (Car car_ in car_list) {
            car_.update_drive_time();
        }
    }
    public void resist_local_car(Car car_) {
        local_car = car_;
    }
    private IEnumerator race_start_co() {
        float elapsed_time = 3f;
        int count = 3;
        while (elapsed_time >= 0f) {            
            if (elapsed_time <= count) {
                count_down_text.text = $"{count}"; 
                count--;
            }
            yield return null;
            elapsed_time -= Time.deltaTime;
        }
        count_down_text.text = "Start!";
        is_start = true;
        yield return new WaitForSeconds(1f);
        count_down_text.text = "";
    }
    public int generate_player_index(Car car_) {
        if (isServer) {
            car_list.Add(car_);
        }
        
        current_player_cnt++;
        return current_player_cnt-1;
    }
}
