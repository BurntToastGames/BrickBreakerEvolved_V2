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

	public GameObject au_pickUp;

	public float au_pickUpVolume = 0.5f;

	SpriteRenderer renderer;

    Transform trans;

	// Use this for initialization
	void Start ()
    {
        trans = GetComponent<Transform>();

		au_pickUp = Instantiate(new GameObject());
		au_pickUp.AddComponent<AudioSource> ();
		au_pickUp.GetComponent<AudioSource> ().playOnAwake = false;

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

			if (col.gameObject.tag.EndsWith ("1")) 
			{
				au_pickUp.GetComponent<AudioSource> ().panStereo = -0.75f;
			
			} else 
			{
				au_pickUp.GetComponent<AudioSource> ().panStereo = 0.75f;
			}
			assignPickUpSFX (powerUpName);
			au_pickUp.GetComponent<AudioSource>().PlayOneShot (au_pickUp.GetComponent<AudioSource>().clip);

			Destroy(au_pickUp, au_pickUp.GetComponent<AudioSource>().clip.length);
			Destroy(this.gameObject);
        }
    }
	void assignPickUpSFX(PowerUpKey powerUpName)
	{
		switch (powerUpName)
		{
		case PowerUpKey.GrowPaddle:
			au_pickUp.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load ("Sound/PowerUpSounds/growPaddle");
			break;
		case PowerUpKey.ShrinkPaddle:
			au_pickUp.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load ("Sound/impact7");
			break;
		case PowerUpKey.MultiBall:
			au_pickUp.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load ("Sound/shoot4");
			break;
		case PowerUpKey.AddLine:
			au_pickUp.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load ("Sound/PowerUpSounds/addline");;
			break;
		default:
			break;
		}
	}

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