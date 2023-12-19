using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
    [SerializeField] private float drift_rigid_friction = 0.01f;
    [SerializeField] private float original_rigid_friction = 0.05f;
 
    [SerializeField] private float steering = 0f;
    [SerializeField] private float steering_rate = 70f;

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

    [SerializeField] private ParticleSystem booster_particle;
    [SerializeField] private Rigidbody car;
    [SerializeField] private TrailRenderer[] trail_renderer_arr;

    private void Start()
    {
        wheels[0] = LFW;
        wheels[1] = RFW;
        wheels[2] = LBW;
        wheels[3] = RBW;

    }
    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer) return;
        booster();
        if (Input.GetKeyDown(KeyCode.R)) {
            respawn();
        }
    }
    void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        accel();
        brake();
        drift();
        steer();
        draw_skid();
    }
    private void booster() {
        if (Input.GetKeyDown(KeyCode.C) && Booster_Slider.instance.use_booster())
        {
            if (boost_co != null) {
                StopCoroutine(boost_co);
                booster_particle.Stop();
            }
            boost_co = StartCoroutine(booster_co());
        }
    }

    private IEnumerator booster_co() {
        is_boosting = true;
        set_rigid_friction(drift_rigid_friction);
        car.AddForce(car.transform.forward*3000f, ForceMode.Impulse);
        booster_coeff = 5f;
        booster_particle.Play();
        yield return new WaitForSeconds(5f);
        booster_coeff = 1f;
        booster_particle.Stop();
        is_boosting = false;
    }
    private void accel() {
        motor_torque = Input.GetAxis("Vertical") * motor_torque_force * booster_coeff;
        LFW.motorTorque = motor_torque;
        RFW.motorTorque = motor_torque;
    }

    private void brake() {
        brake_torque = Input.GetAxis("Brake") * brake_torque_force;
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
            set_forward_friction(LFW, drift_back_wheel_friction_coeff);
            set_forward_friction(RFW, drift_back_wheel_friction_coeff);
            set_side_friction(LBW, drift_back_wheel_friction_coeff);
            set_side_friction(RBW, drift_back_wheel_friction_coeff);
        }
        else
        {
            //skid_mark_off();
            if (!is_boosting) {
                set_rigid_friction(original_rigid_friction);
            }
            set_forward_friction(LFW, original_back_wheel_friction_coeff);
            set_forward_friction(RFW, original_back_wheel_friction_coeff);
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
        if (side_velocity_sqrmag > 1f)
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
        for (int i = 0; i < trail_renderer_arr.Length; i++)
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

    private void respawn() {
        KeyValuePair<Vector3, Vector3> pos_for = Track_Manager.instance.get_nearest_respawn_point(transform.position);
        transform.position = pos_for.Key + Vector3.up;
        transform.forward = pos_for.Value;
        init();
    }

    private void init() {
        skid_mark_off();

        car.velocity = Vector3.zero;
        car.angularVelocity = Vector3.zero;
        
        is_boosting = false;
        if (boost_co != null)
        {
            StopCoroutine(boost_co);
            booster_particle.Stop();
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
}
