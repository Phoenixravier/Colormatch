﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class PlayerBehaviour : ExtendedBehaviour {
    public float Threshold = 30f;
    public PlayerID _playerID;
    Team playerTeam;
    public List<int> values;
    private Rigidbody2D rb;
    public float speed;
    private Animator anim;
    public float rotateSpeed = 2;
    
	private SpriteRenderer sr;
    private Collider2D c;
    private bool canReswing = true;
    private Rigidbody2D body;
    private Transform crosshair;

	[HideInInspector]
    public bool canMove = true;

	float moveHori = 0;
	float moveVert = 0;
    float rotx = 0;
    float roty = 0;

	bool isDead;
    bool stopKilling = false;

	void Awake () {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
		c = GetComponent<BoxCollider2D>();
		playerTeam = GetComponent<Health> ().getTeam ();
        crosshair = transform.Find("Crosshair");
    }

    void killPlayer()
    {   
		Pause (0.1f,0.1f);
        c.enabled = false;
        sr.enabled = false;
        canMove = false;
    }

    public void spawnPlayer()
    {
        GameObject spawn = GameObject.FindGameObjectWithTag(playerTeam.ToString() + "Spawn");
        transform.position = spawn.transform.position;
    }

    public void respawnPlayer()
    {
        stopKilling = true;
        Wait(1, () => {
            //place player at respawn 
			GetComponent<Health> ().setDeath (false);
            GameObject spawn = GameObject.FindGameObjectWithTag(playerTeam.ToString() + "Spawn");
            transform.position = spawn.transform.position;
            //make player visible
            canMove = true;
            sr.enabled = true;
            c.enabled = true;            
			GetComponent<Health>().setUp();
            stopKilling = false;
        });

    }

	void Update(){
		isDead = GetComponent<Health>().getDeath ();
		moveHori = 0;
		moveVert = 0;
		if (canMove)
		{
			moveHori = InputManager.GetAxisRaw("Horizontal", _playerID);
			moveVert = InputManager.GetAxisRaw("Vertical", _playerID);
            rotx = InputManager.GetAxis("LookHorizontal", _playerID);
            roty = InputManager.GetAxis("LookVertical", _playerID);
		}
        if (InputManager.GetButton("Slash", _playerID) && !anim.GetBool("isSwing") && !isDead && canReswing)
        {
            Debug.Log("stab stab00");
            anim.SetTrigger("isSwing");
        }

        if (InputManager.GetButton("Slash", _playerID))
            canReswing = false;
        else
            canReswing = true;

		if (isDead && !stopKilling)
		{
			killPlayer();
			respawnPlayer();
		}

	}


	// Update is called once per frame
	void FixedUpdate () {
        
        Vector2 movement = new Vector2(moveHori, moveVert);
        rb.velocity = speed*movement;
 

        //walking animation set
        if (movement.x != 0 || movement.y != 0)
        {
            anim.SetBool("PlayerMoving", true);
        }
        else
            anim.SetBool("PlayerMoving", false);


        //rotate player
        Vector3 targetPoint = crosshair.position - transform.position;
        float targetAngle = Vector2.Angle(transform.up, targetPoint);
        Vector3 cross = Vector3.Cross(transform.up, targetPoint);

        //targetAngle = targetPoint.y < 0 ? -targetAngle : targetAngle;
        rb.AddTorque(targetAngle * cross.z * rotateSpeed);
        
        /*{
            //hopefully this works in stopping unwanted rotation from collisions but I actually can't test it properly 
            if ((Mathf.Abs(rotx) > 0.01f && Mathf.Abs(roty) > 0.01f) || (Mathf.Abs(rotx) > 0.5f && roty == 0.0f) || (Mathf.Abs(roty) > 0.5f && rotx == 0.0f))
            {
                rb.freezeRotation = false;
                float angle = Mathf.Atan2(roty, rotx) * Mathf.Rad2Deg;
                //rotate by that angle plus 90° to get player face rather than side facing 
                rb.MoveRotation(angle - 90);
            }
            else
            {
                rb.angularVelocity = 0;
                rb.freezeRotation = true;
            }

        }*/
        
    }

}
