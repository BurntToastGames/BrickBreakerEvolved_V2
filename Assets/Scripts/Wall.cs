using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour
{
	public float nudgeForce = 100f;

	public AudioSource au_wallImpact;
	public float wallImpactVolume = 0.15f;
	AudioClip au_wallImpactClip;

	void Start()
	{
		au_wallImpact = (AudioSource)gameObject.AddComponent<AudioSource>();//Initialize an audio source on the wall Object
		au_wallImpactClip = (AudioClip)Resources.Load("Sound/impact2");//Define an audio clip

		au_wallImpact.playOnAwake = false;

		au_wallImpact.clip = au_wallImpactClip;//Set the audioSources clip to be our audioClip
		au_wallImpact.volume = wallImpactVolume;
	}
	void OnCollisionEnter2D(Collision2D col)
	{
		au_wallImpact.Play();
	}
	void OnCollisionStay2D(Collision2D col)
	{
		if(transform.position.x > col.rigidbody.position.x)
		{
			col.rigidbody.AddForce(new Vector2(-nudgeForce, 0));
		}
		else
		{
			col.rigidbody.AddForce(new Vector2(nudgeForce, 0));
		}

	}
}
