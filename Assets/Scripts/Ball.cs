﻿using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
	public GameObject DeathParticles;

    public int Player;

	public float MinSpeed = 7.5f;
	public float MaxSpeed = 40f;
	public float StartingSpeed = 250f;
	private Vector2 startVelocity;

    private GameObject ball;
    private Rigidbody2D rig2D;

    internal bool ballInPlay = false;
    private Vector2 paddleSpeed;

	// Use this for initialization
	void Start ()
    {
        ball = GameObject.FindGameObjectWithTag("Ball" + Player);
        rig2D = GetComponent<Rigidbody2D>();
	}

	void LateUpdate()
	{
		//Get ball up to minimum speed.
		if (rig2D.velocity.magnitude < MinSpeed && ballInPlay)
		{
			rig2D.velocity = Vector2.ClampMagnitude(rig2D.velocity * 10, MinSpeed);
		}

        //Clamp maximum ball speed.
		if (rig2D.velocity.magnitude >= MaxSpeed) 
		{
			rig2D.velocity = Vector2.ClampMagnitude(rig2D.velocity, MaxSpeed);
		}
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!ballInPlay && Input.GetButtonDown("Fire" + Player))
        {
            if(ball != null)
                ball.transform.parent = null;

            ballInPlay = true;
            rig2D.isKinematic = false;

            //Redirects Ball based on paddle velocity.
			startVelocity.Set(Input.GetAxis("Horizontal" + Player) * StartingSpeed, StartingSpeed);
            rig2D.AddForce(startVelocity);
        }

        outOfBoundsCheck();
	}

    void outOfBoundsCheck()
    {   
        //Ball OOB + Reset
        if (transform.position.y < -4.5)
        {
            if (GameObject.FindGameObjectsWithTag("Ball" + Player).GetLength(0) == 1)
            {
                Destroy(Instantiate(DeathParticles, gameObject.transform.position, Quaternion.identity), 4);

                transform.parent = GameObject.FindGameObjectWithTag("Paddle" + Player).transform;
                ballInPlay = false;
                rig2D.isKinematic = true;
                rig2D.velocity = Vector2.zero;
                this.transform.position = new Vector2(transform.parent.position.x, transform.parent.position.y + 1.5f * transform.parent.localScale.y);

                //Reset The Player Combo
                GameObject.FindGameObjectWithTag("Game Manager").SendMessage("resetCombo", Player);

                //Update the ball object in case the old ball was deleted.
                if(ball != this.gameObject)
                {
                    ball = this.gameObject;
                }
            }
            else
            {
                Destroy(Instantiate(DeathParticles, gameObject.transform.position, Quaternion.identity), 4);
                Destroy(this.gameObject);
                GameObject.FindGameObjectWithTag("Game Manager").SendMessage("ballDestroyed", Player);
            }
        }
    }
		
}