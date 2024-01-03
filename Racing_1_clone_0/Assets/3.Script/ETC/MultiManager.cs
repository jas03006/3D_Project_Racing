using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
public class MultiManager : NetworkBehaviour
{
    public static MultiManager instance = null;
    [SerializeField] private Camera_Controller start_cam;
    [SerializeField] private Text count_down_text;
    public Car local_car { get; private set; } = null;
    public List<Car> car_list { get; private set; } = null;
    

    public int current_player_cnt { get; private set; } = 0;
    [SyncVar]
    public bool is_start;
    [SyncVar]
    public bool is_finish = false;

    private int count = 3;
    
    public float timer { get; private set; } = 0f;

    [Header("Rank")]
    private List<int> final_rank_index_list;
    [SerializeField]private Rank_Slot[] rank_slt_arr;
    public Car[] car_rank_arr { get; private set; } = null;

    public RectTransform[] name_tag_arr;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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
        
        StartCoroutine(race_start_co());
    }

    private void Start()
    {
        count = 3;
        count_down_text = GameObject.FindGameObjectWithTag("Count_Down_Text").GetComponent<Text>();
        is_start = false;
       // if (!isServer && isClient) {
            start_cam_move();
        // }
        final_rank_index_list = new List<int>();
        car_rank_arr = new Car[rank_slt_arr.Length];
    }
    
    private void Update()
    {        
        
        if (is_start) {
            if (isServer )
            {
                update_timer();
                update_rank();
            }
            
        }
        if (isServer && !isClient) {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                show_car(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                show_car(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                show_car(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                show_car(3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                show_car(4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                show_car(5);
            }
        }
    }

    private void show_car(int ind) {
        if (ind >=0 && ind < car_list.Count) {
            start_cam.transform.SetParent(car_list[ind].get_camera_point());
            start_cam.transform.localPosition = Vector3.zero;
            start_cam.transform.LookAt(car_list[ind].get_camera_aim_point());
            local_car = car_list[ind];
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
        //start_cam_move();        
        yield return new WaitForSeconds(2f);

        float elapsed_time = (float) count;        
        while (elapsed_time >= 0f) {            
            if (elapsed_time <= count) {
                update_count_text($"{count}");
                count--;
            }
            yield return null;
            elapsed_time -= Time.deltaTime;
        }
        update_count_text("Start!");
        is_start = true;
        yield return new WaitForSeconds(1f);
        update_count_text("");
    }

    private void update_count_text(string msg) {
        count_down_text.text = msg;
        update_count_text_RPC(msg);
    }
    [ClientRpc]
    private void update_count_text_RPC(string msg) {
        count_down_text.text = msg;
    }
    public int generate_player_index(Car car_) {
        if (isServer) {
            car_list.Add(car_);
            rank_slot_on(current_player_cnt, car_.get_name(current_player_cnt));
            for (int i = 0; i <= current_player_cnt; i++)
            {
                rank_slot_on_RPC(i, car_list[i].get_name(i));
            }
                
        }
        
        current_player_cnt++;
        return current_player_cnt-1;
    }

    private void start_cam_move() {
        start_cam.start_cam_move();
    }
    [ClientRpc]
    private void start_cam_move_RPC()
    {
        start_cam_move();
    }
    public void rank_slot_on(int ind, string name)
    {
        rank_slt_arr[ind].gameObject.SetActive(true);
        rank_slt_arr[ind].update_name(name);
    }
    [ClientRpc]
    public void rank_slot_on_RPC(int ind, string name) {
        
            rank_slot_on(ind,name );
      
    }
    public void update_rank() {
        if (final_rank_index_list.Count == car_list.Count) {
            is_finish = true;
            return;
        }
        for (int i =0; i < car_list.Count; i++) {
            if (final_rank_index_list.Contains(i)) {
                continue;
            }
            car_list[i].cal_dist_from_goal();
            car_rank_arr[i] = null;
        }
        for (int i = 0; i < car_list.Count; i++) 
        {
            if (final_rank_index_list.Contains(i))//주행 중인 모든 차를 순회하면서
            {
                continue;
            }
            for (int j = final_rank_index_list.Count; j < car_list.Count; j++) // 확정된 rank slot이후 부터 활성화된 rank slot 까지
            {
                if (car_rank_arr[j] == null)
                {
                    car_rank_arr[j] = car_list[i];
                    break;
                }
                else if(car_rank_arr[j].dist_from_goal >= car_list[i].dist_from_goal)
                {
                    for (int k = car_list.Count-1; k > j ; k--) {
                        
                        car_rank_arr[k] = car_rank_arr[k-1];
                    }
                    car_rank_arr[j] = car_list[i];
                    break;
                }
            }
        }
        for (int i = final_rank_index_list.Count; i < car_list.Count; i++)
        {
            string str = car_rank_arr[i].get_name() + " " + (int)car_rank_arr[i].dist_from_goal;
            update_rank_text(i, str);
            update_rank_text_Rpc(i, str);
        }
    }
    public void update_rank_text(int ind, string name) {
        rank_slt_arr[ind].update_name(name);
    }
    [ClientRpc]
    public void update_rank_text_Rpc(int ind, string name)
    {
        update_rank_text( ind,  name);
    }

    public void update_rank_final(int ind,  float drive_time, string name) {
        if (final_rank_index_list.Contains(ind)) {
            return;
        }
        final_rank_index_list.Add(ind);
        rank_slt_arr[final_rank_index_list.Count - 1].update_name(name);
        int m = (int)(drive_time / 60f);
        rank_slt_arr[final_rank_index_list.Count - 1].show_record(string.Format("{0:00}", (int)m) + " : "+ string.Format("{0:00.000}", (drive_time - m * 60f)));
    }
    [Server]
    public int get_forntier_ind(int player_ind) {
        for (int i =final_rank_index_list.Count+1; i < car_rank_arr.Length; i++) {
            if (car_rank_arr[i] == null)
            {
                return -1;
            }
            if (car_rank_arr[i].player_index == player_ind) {
                return car_rank_arr[i - 1].player_index;
            }
        }
        return -1;
    }
    [Server]
    public Car get_forntier(int player_ind)
    {
        int ind = get_forntier_ind(player_ind);
        if (ind < 0) {
            return null;
        }
        return car_list[ind];
    }

}
