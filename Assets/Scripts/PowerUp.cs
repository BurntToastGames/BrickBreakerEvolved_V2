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

    Transform trans;

	// Use this for initialization
	void Start ()
    {
        trans = GetComponent<Transform>();
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

           Destroy(this.gameObject);
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