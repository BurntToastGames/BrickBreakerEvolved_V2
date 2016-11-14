using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour
{
    public GameObject PowerUpPaddle;

    public float powerUpDuration = 10f;

	public PowerUpKey powerUpName = PowerUpKey.GrowPaddle;

	public float scaleAmount = 0.4f;

    public float FallSpeed = 5f;

    public int addLineUpperBound = 3;

	public AudioSource au_pickUp;

	public float au_pickUpVolume = 0.5f;

	SpriteRenderer renderer;

    Transform trans;

	// Use this for initialization
	void Start ()
    {
        trans = GetComponent<Transform>();

		au_pickUp = (AudioSource)gameObject.AddComponent<AudioSource>();//Initialize an audio source
		au_pickUp.playOnAwake = false;

	}
	
	// Update is called once per frame
	void Update ()
    {
        float newYPos = transform.position.y - (FallSpeed * Time.deltaTime);
        trans.position = new Vector2(trans.position.x, newYPos);

        if(trans.position.y < -4.5)
        {
            Destroy(this.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject == PowerUpPaddle)
        {
            GameObject.Find("Game Manager").SendMessage("applyPowerUp", new applyPowerUpInfo { player = col.gameObject.tag.EndsWith("1") ? 1 : 2 ,
																						   powerUpKey = powerUpName,
																						  scaleAmount = scaleAmount,
                                                                                    addLineUpperBound = addLineUpperBound} );
			
			//renderer = this.gameObject.GetComponentInChildren<SpriteRenderer> ();
			//assignPickUpSFX (powerUpName);
			//PlayAudioAndDestroy (au_pickUp.clip, this.gameObject);
			Destroy(this.gameObject);
        }
    }
	/*
	IEnumerator PlayAudioAndDestroy(AudioClip clip, GameObject curObject)
	{
		print ("In play audio");
		renderer.enabled = false;
		au_pickUp.volume = au_pickUpVolume;
		au_pickUp.Play();

		yield return new WaitForSeconds (clip.length);
		Destroy(this.gameObject);
	}
	void assignPickUpSFX(PowerUpKey powerUpName)
	{
		switch (powerUpName)
		{
		case PowerUpKey.GrowPaddle:
			au_pickUp.clip = (AudioClip)Resources.Load ("Sound/missile_impact1");
			break;
		case PowerUpKey.ShrinkPaddle:
			au_pickUp.clip = (AudioClip)Resources.Load ("Sound/missile_impact1");
			break;
		case PowerUpKey.MultiBall:
			au_pickUp.clip = (AudioClip)Resources.Load ("Sound/missile_impact1");
			break;
		case PowerUpKey.AddLine:
			au_pickUp.clip = (AudioClip)Resources.Load ("Sound/missile_impact1");;
			break;
		default:
			break;
		}
	}
	*/
}

public class applyPowerUpInfo
{
    public float powerUpDuration { get; set; }

    public int player { get; set; }

    public float scaleAmount { get; set; }

    public PowerUpKey powerUpKey { get; set; }

    public int addLineUpperBound { get; set; }
}

public enum PowerUpKey
{
    GrowPaddle, ShrinkPaddle, MultiBall, AddLine
}