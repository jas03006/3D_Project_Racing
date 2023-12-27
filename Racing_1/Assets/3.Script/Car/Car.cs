using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum item_index { 
    empty = -1,
    smoke_screen = 0,
    slicer = 1,
    oil = 2,
    temp = 3,
    item_box=4
}

public class Car : NetworkBehaviour
{
    [SerializeField] private float motor_torque = 0f;
    [SerializeField] private float motor_torque_force = 800f;

    [SerializeField] private float booster_coeff = 1f;

    [SerializeField] private float brake_torque = 0f;
    [SerializeField] private float brake_torque_force = 800f;

    [SerializeField] private float drift_torque = 0f;
    [SerializeField] private float drift_torque_force = 800f;
    [SerializeField] private float drift_back_wheel_friction_coeff = 0.3f;
    [SerializeField] private float original_back_wheel_friction_coeff;
    [SerializeField] private float drift_front_wheel_friction_coeff = 0.3f;
    [SerializeField] private float original_front_wheel_friction_coeff;
    [SerializeField] private float drift_rigid_friction = 0.01f;
    [SerializeField] private float original_rigid_friction = 0.05f;

    [SerializeField] private float steering = 0f;
    [SerializeField] private float steering_rate = 70f;

    [SyncVar]
    private bool is_boosting = false;
    private Coroutine boost_co = null;

    [SerializeField] WheelCollider LFW;
    [SerializeField] GameObject LFW_Mesh;

    [SerializeField] WheelCollider RFW;
    [SerializeField] GameObject RFW_Mesh;

    [SerializeField] WheelCollider LBW;
    [SerializeField] GameObject LBW_Mesh;

    [SerializeField] WheelCollider RBW;
    [SerializeField] GameObject RBW_Mesh;

    private WheelCollider[] wheels = new WheelCollider[4];

    [SerializeField] private ParticleSystem[] booster_particle_arr;
    [SerializeField] private ParticleSystem speed_particle;
    [SerializeField] private Rigidbody car;
    [SerializeField] private TrailRenderer[] trail_renderer_arr;
    [SerializeField] private MeshRenderer[] wheel_renderer_arr;

    [Header("Camera")]
    [SerializeField] private Transform camera_point_tr;
    [SerializeField] private Transform camera_side_point_tr;
    [SerializeField] private Transform camera_aim_point_tr;
    [SerializeField] private Transform camera_back_aim_point_tr;

    [Header("Network")]
    [SyncVar]
    public int player_index = 0;
    [SyncVar]
    public color_index material_index = 0;

    [SerializeField] public MeshRenderer car_body;

    [Header("Lap_Check")]
    [SerializeField] private LapCheckLine[] lap_check_line_arr;
    
    [SerializeField] private bool[] lap_check_bool_arr;

    [SyncVar]
    [SerializeField] private bool is_finish = false;
    [SyncVar]
    [SerializeField] public float drive_time= 0f;
    [SerializeField] public float dist_from_goal= 0f;
    [SyncVar]
    [SerializeField] public int lap_cnt = 0;

    public NewNetworkRoomManager net_manager;
    private Item_Slot item_slot_UI;
    private void Start()
    {
        //init_lap_check(); 

        wheels[0] = LFW;
        wheels[1] = RFW;
        wheels[2] = LBW;
        wheels[3] = RBW;

        ask_player_index();
        Minimap_Manager.instance.regist_car(this);
        init_car_model();

        if (isLocalPlayer) {
            item_slot_UI = FindObjectOfType<Item_Slot>();
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        init_lap_check();
        net_manager = FindObjectOfType<NewNetworkRoomManager>();
    }
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        MultiManager.instance.resist_local_car(this);
        net_manager = FindObjectOfType<NewNetworkRoomManager>();
    }

    // Update is called once per frame
    private void Update()
    {        
        draw_speed_particle();
        draw_booster_particle();

        if (!isLocalPlayer) return;
        if (is_finish || !MultiManager.instance.is_start) return;

        //Debug.Log($"ind: {player_index}, mat: {material_index}");
        
        booster();
        if (Input.GetKeyDown(KeyCode.V))
        {
            shoot_slicer_CMD();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            throw_smoke_screen_CMD();
        }
        if (Input.GetKeyDown(KeyCode.X)) {
            use_item();
            // throw_smoke_screen_CMD();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
           StartCoroutine( respawn());
        }
    }
    void FixedUpdate()
    {                
        if (is_finish || !MultiManager.instance.is_start){ return; }
        if (!isLocalPlayer) return;
        control_car();
    }

    /*[Command]
    private void control_car_CMD() {
        control_car();
    }*/
    private void control_car()
    {
        accel();
        brake();
        drift();
        steer();
        draw_skid();
        rotate_wheel();
    }

    private void draw_speed_particle() {
        
        if ( car.velocity.magnitude * 7.5f >= 100f)
        {
            //Debug.Log($"ind: {player_index}, mat: {material_index}, mag:{car.velocity.magnitude * 7.5f}");
            if (!speed_particle.isPlaying)
            {
                speed_particle.Play();
            }            
        }
        else {
            if (speed_particle.isPlaying) {
                speed_particle.Stop();
            }           
        }
    }
    [Server]
    private void init_lap_check() {        
        is_finish = false;
        LapCheckLine[] lcl = GameObject.FindObjectsOfType<LapCheckLine>();
        lap_check_bool_arr = new bool[lcl.Length+1];
        lap_check_line_arr = new LapCheckLine[lcl.Length];
        for (int i =0; i < lcl.Length; i++) {            
            LapCheckLine lcl_ = lcl[i].GetComponent<LapCheckLine>();
            lap_check_line_arr[lcl_.col_cnt] = lcl_;
            lap_check_bool_arr[i] = false;
            if (lcl_.col_cnt == 0) {
                lap_check_bool_arr[lap_check_bool_arr.Length-1] = false;
            }
        }
    }
    public int get_check_point_dist_ind() {
        for (int i = 0; i < lap_check_bool_arr.Length; i++)
        {
            if (!lap_check_bool_arr[i])
            {
                return lap_check_bool_arr.Length - i;
            }
        }
        return 0;
    }
    private void check_finish() {
        for (int i =0; i < lap_check_bool_arr.Length; i++) {
            if (!lap_check_bool_arr[i]) {
                return;
            }
        }
        for (int i = 1; i < lap_check_bool_arr.Length; i++)
        {
            lap_check_bool_arr[i] = false;
        }
        is_finish = true;
        lap_cnt++;
        init(false);
        brake(1000f);
        show_finish();
        MultiManager.instance.update_rank_final(player_index, drive_time, get_name());
    }
    [ClientRpc]
    private void check_finish_RPC()
    {
        check_finish();
    }
    [Server]
    public void update_drive_time() {
        if (is_finish || !MultiManager.instance.is_start) {
            return;
        }
        drive_time = MultiManager.instance.timer;
    }
    private void show_finish() {
        Debug.Log("Finish!!!!!!!!!!");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isLocalPlayer) {
            Car col_car = collision.gameObject.GetComponent<Car>();
            if (col_car != null && col_car.isLocalPlayer) {
                send_collision_CMD(collision.impulse);
            }            
        }
    }
    [Command(requiresAuthority =false)]
    private void send_collision_CMD(Vector3 impulse) {
        if (isLocalPlayer)
        {
            car.AddForce(impulse, ForceMode.Impulse);
        }
        else {
            send_collision_TRPC(impulse);
        }       
    }
    [TargetRpc]
    private void send_collision_TRPC(Vector3 impulse)
    {
        car.AddForce(impulse, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isClient)
        {
            if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Platform"))) {
                if (other.gameObject.CompareTag("Booster_Platform")) {
                    if (Vector3.Dot(other.transform.forward.normalized , transform.forward.normalized) < 0) {
                        car.velocity = Vector3.zero;
                    }
                    car.AddForce(other.transform.forward * 10000f, ForceMode.Impulse);
                }
                return;
            }                
        }
        if (isServer)
        {
            if (other.gameObject.layer.Equals(LayerMask.NameToLayer("CheckBox")))
            {
                LapCheckLine lcl_ = other.GetComponent<LapCheckLine>();
                if (lcl_.col_cnt == 0 && lap_check_bool_arr[0])
                {
                    for (int i = 0; i < lap_check_bool_arr.Length - 1; i++)
                    {
                        if (!lap_check_bool_arr[i])
                        {
                            return;
                        }
                    }
                    lap_check_bool_arr[lap_check_bool_arr.Length - 1] = true;
                }
                else
                {
                    if (lcl_.col_cnt == 0)
                    {
                        lap_check_bool_arr[lcl_.col_cnt] = true;
                    }
                    else if (lap_check_bool_arr[lcl_.col_cnt - 1])
                    {
                        lap_check_bool_arr[lcl_.col_cnt] = true;
                    }
                }
                check_finish();
                if (is_finish)
                {
                    check_finish_RPC();
                }
            }
        }
            
    }

    [Server]
    public void ask_player_index() {
        player_index = MultiManager.instance.generate_player_index(this);
    }
    public void init_car_model() {
        car_body.material = Color_Manager.instance.car_material_arr[(int)material_index];
    }
    public void set_car_model(color_index index_)
    {
        material_index = index_;
        car_body.material = Color_Manager.instance.car_material_arr[(int)material_index];
    }

    #region booster
    private void booster() {
        if (Input.GetKeyDown(KeyCode.C) && Booster_Slider.instance.use_booster())
        {
            if (boost_co != null) {
                StopCoroutine(boost_co);
                //booster_particle_stop();
            }
            boost_co = StartCoroutine(booster_co());
        }
    }
    private IEnumerator booster_co()
    {
        is_boosting = true;
        set_is_boost_CMD(is_boosting);

        set_rigid_friction(drift_rigid_friction);
        car.AddForce(car.transform.forward * 3000f, ForceMode.Impulse);
        booster_coeff = 5f;
        //booster_particle_play();
        yield return new WaitForSeconds(5f);
        booster_coeff = 1f;
        //booster_particle_stop();

        is_boosting = false;
        set_is_boost_CMD(is_boosting);
    }

    [Command]
    private void set_is_boost_CMD(bool value) {
        is_boosting = value;
    }
    

    private void booster_particle_play(){
        for (int i =0; i < booster_particle_arr.Length;i++) {
            if (!booster_particle_arr[i].isPlaying) {
                booster_particle_arr[i].Play();
            }
        }
    }
    


    private void booster_particle_stop(){
        for (int i = 0; i < booster_particle_arr.Length; i++)
        {
            if (booster_particle_arr[i].isPlaying)
            {
                booster_particle_arr[i].Stop();
            }
        }
    }
    private void draw_booster_particle()
    {
        if (is_boosting)
        {
            booster_particle_play();
        }
        else
        {
            booster_particle_stop();
        }
    }
    #endregion

    private void accel() {
        motor_torque = Input.GetAxis("Vertical") * motor_torque_force * booster_coeff;
        LFW.motorTorque = motor_torque;
        RFW.motorTorque = motor_torque;
    }

    private void rotate_wheel() {
        Vector3 pos = new Vector3();
        Quaternion temp_q = new Quaternion();
        for (int i =0; i < wheels.Length; i++) {   
            wheels[i].GetWorldPose(out pos, out temp_q);
            wheel_renderer_arr[i].transform.rotation = temp_q;
        }
    }

    private void brake(float torque = 0f) {
        if (torque == 0)
        {
            brake_torque = Input.GetAxis("Brake") * brake_torque_force;
        }
        else {
            brake_torque = torque;
        }
        LFW.brakeTorque = brake_torque;
        RFW.brakeTorque = brake_torque;
        LBW.brakeTorque = brake_torque;
        RBW.brakeTorque = brake_torque;
    }

    private void drift() {
        drift_torque = Input.GetAxis("Drift") * drift_torque_force / 10f;
        LBW.brakeTorque = drift_torque;
        RBW.brakeTorque = drift_torque;
        if (drift_torque != 0)
        {
            //skid_mark_on();
            //Booster_Slider.instance.get_gage(Time.fixedDeltaTime * car.velocity.magnitude);
            set_rigid_friction(drift_rigid_friction);
            set_forward_friction(LFW, drift_front_wheel_friction_coeff);
            set_forward_friction(RFW, drift_front_wheel_friction_coeff);
            set_side_friction(LBW, drift_back_wheel_friction_coeff);
            set_side_friction(RBW, drift_back_wheel_friction_coeff);
        }
        else
        {
            //skid_mark_off();
            if (!is_boosting) {
                set_rigid_friction(original_rigid_friction);
            }
            set_forward_friction(LFW, original_front_wheel_friction_coeff);
            set_forward_friction(RFW, original_front_wheel_friction_coeff);
            set_side_friction(LBW, original_back_wheel_friction_coeff);
            set_side_friction(RBW, original_back_wheel_friction_coeff);
        }
    }


    private void set_rigid_friction(float value)
    {
        car.drag = value;
    }

    private void set_forward_friction(WheelCollider wc, float value)
    {
        WheelFrictionCurve wfc = wc.forwardFriction;
        wfc.stiffness = value;
        wc.forwardFriction = wfc;
    }

    private void set_side_friction(WheelCollider wc, float value)
    {
        WheelFrictionCurve wfc = wc.sidewaysFriction;
        wfc.stiffness = value;
        wc.sidewaysFriction = wfc;
    }

    private void draw_skid() {
        
        float side_velocity_sqrmag = get_side_velocity().sqrMagnitude;
        if (side_velocity_sqrmag > 1.7f)
        {
            if (skid_mark_on(side_velocity_sqrmag)) {
                Booster_Slider.instance.get_gage(Time.fixedDeltaTime * Mathf.Log(side_velocity_sqrmag, 1.5f) );
            }            
        }
        else {
            skid_mark_off();
        }
        
    }

    private bool skid_mark_on(float value)
    {
        //Color new_color = new Color(1f, 1f, 0f, Mathf.Min(value / 20f, 1f));
        bool is_on = false;
        for (int i = 2; i < trail_renderer_arr.Length; i++)
        {
            if (wheels[i].isGrounded)
            {
                //trail_renderer_arr[i].startColor = new_color;
               // trail_renderer_arr[i].endColor = new_color;
                trail_renderer_arr[i].emitting = true;             
                is_on = true;
            }
            else {
                trail_renderer_arr[i].emitting = false;
            }            
        }
        return is_on;
    }
    private void skid_mark_off()
    {
        for (int i = 0; i < trail_renderer_arr.Length; i++)
        {
            trail_renderer_arr[i].emitting = false;
        }
    }

    private void steer() {
        steering = Input.GetAxis("Horizontal") * steering_rate;
        LFW.steerAngle = steering;
        LFW_Mesh.transform.localRotation = Quaternion.Euler(0, steering, 0);
        RFW.steerAngle = steering;
        RFW_Mesh.transform.localRotation = Quaternion.Euler(0, steering, 0);
    }



    private Vector3 get_side_velocity() {
        Vector3 result = car.velocity;
        return result - Vector3.Project(result,transform.forward);
    }

    private IEnumerator respawn() {
        enable_car_body();
        enable_car_body_CMD();
        set_active_wheels(true);
        set_active_wheels_CMD(true);

        init();
        yield return null;
        KeyValuePair<Vector3, Vector3> pos_for = Track_Manager.instance.get_nearest_respawn_point(transform.position);
        transform.position = pos_for.Key + Vector3.up;
        transform.forward = pos_for.Value;
        
    }
    public void set_active_wheels(bool value)
    {
        for (int i = 0; i < wheels.Length; i++) {
            wheels[i].enabled = value;
            wheel_renderer_arr[i].GetComponent<MeshCollider>().enabled = !value;
        }
    }
    [Command]
    public void set_active_wheels_CMD(bool value)
    {
        set_active_wheels(value);
        set_active_wheels_RPC(value);
    }
    [ClientRpc]
    public void set_active_wheels_RPC(bool value)
    {
        set_active_wheels(value);
    }

    private void enable_car_body() {
        car_body.enabled = true;
        car_body.gameObject.SetActive(true);
    }
    [Command]
    private void enable_car_body_CMD()
    {
        enable_car_body();
        enable_car_body_RPC();
    }
    [ClientRpc]
    private void enable_car_body_RPC()
    {
        enable_car_body();
    }

    private void init(bool force_vel_zero = true) {
        skid_mark_off();

        if (force_vel_zero) {
            car.velocity = Vector3.zero;
            car.angularVelocity = Vector3.zero;
        }        
        
        is_boosting = false;
        if (boost_co != null)
        {
            StopCoroutine(boost_co);
            //booster_particle_stop();
        }
        boost_co = null;
        

        set_rigid_friction(original_rigid_friction);
        set_forward_friction(LFW, original_back_wheel_friction_coeff);
        set_forward_friction(RFW, original_back_wheel_friction_coeff);
        set_side_friction(LBW, original_back_wheel_friction_coeff);
        set_side_friction(RBW, original_back_wheel_friction_coeff);
        
        LFW.motorTorque = 0f;
        RFW.motorTorque = 0f;
    }

    public Transform get_camera_point() {
        return camera_point_tr;
    }
    public Transform get_camera_aim_point()
    {
        return camera_aim_point_tr;
    }
    public Transform get_camera_back_aim_point()
    {
        return camera_back_aim_point_tr;
    }
    public Transform get_camera_side_point()
    {
        return camera_side_point_tr;
    }

    public void cal_dist_from_goal() {
        dist_from_goal = Track_Manager.instance.get_distance_from_goal(this);
    }
    public string get_name(int ind = -1) {
        if (ind < 0)
        {
            return (player_index + 1) + "P";
        }
        return (ind + 1) + "P";
    }

    [Command]
    private void throw_smoke_screen_CMD() {        
        GameObject go = Instantiate(net_manager.spawnPrefabs[(int)item_index.smoke_screen], transform.position + Vector3.up*3f - transform.forward*2f, Quaternion.identity);
        go.GetComponent<Rigidbody>().AddForce(-transform.forward*2f,ForceMode.Impulse);
        NetworkServer.Spawn(go);
        Smoke_Screen ss = go.GetComponent<Smoke_Screen>();
        ss.set_smoke_color((int)material_index);
        ss.set_smoke_color_RPC((int)material_index);
    }
    

    [Command]
    private void shoot_slicer_CMD()
    {
        GameObject go = Instantiate(net_manager.spawnPrefabs[(int)item_index.slicer], transform.position + Vector3.up * 1f + transform.forward * 3.5f,Quaternion.LookRotation( transform.forward, transform.up));
       // go.GetComponent<Rigidbody>().AddForce(-transform.forward * 2f, ForceMode.Impulse);
        NetworkServer.Spawn(go);
    }
    [Command]
    private void throw_oil_CMD()
    {
        GameObject go = Instantiate(net_manager.spawnPrefabs[(int)item_index.oil], transform.position + Vector3.up * 1f + transform.forward * 3.5f, Quaternion.LookRotation(transform.forward, transform.up));
        // go.GetComponent<Rigidbody>().AddForce(-transform.forward * 2f, ForceMode.Impulse);
        NetworkServer.Spawn(go);
    }

    [TargetRpc]
    public void get_item_TRPC(item_index ind_) {
        Debug.Log($"{player_index}: {ind_}");
        if (item_slot_UI.index == item_index.empty) {
            item_slot_UI.set_img(ind_);
        }
    }

    public void use_item() {
        item_index ind_ = item_slot_UI.use_item();
        switch (ind_)
        {
            case item_index.smoke_screen:
                throw_smoke_screen_CMD();
                break;
            case item_index.slicer:
                shoot_slicer_CMD();
                break;
            case item_index.oil:
                throw_oil_CMD();
                break;            
            default:
                break;
        }
    }
}
