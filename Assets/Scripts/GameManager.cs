using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
	public Player player1, player2;
    public GameObject[] LinePrefabs;
	public GameObject gameOverPanel;
    public GameObject PowerUpPrefab;
	public GameObject gamePausePanel;
	public int maxLineCount;

    public int bricksPerLine = 12;       //bricks needed to send a line.
    public float scorePerBrick = 50f;    //score needed to send a brick.
    public float brickValue = 50f;      //default score awarded per brick hit.

    public float powerUpSpawnRate = 10f; //In seconds.

    private float lineSpaceConst = 1.14f;

	private float p1AddLineYOffset = 0;
	private float p2AddLineYOffset = 12;

	private Text player1PendingText;
	private Text player2PendingText;

	private Slider player1PendingSlider;
	private Slider player2PendingSlider;

    private Text player1ScoreText;
    private Text player2ScoreText;
	private bool p1Growing = true;
	private bool p2Growing = true;
	private float scaleTextFactor = 0.005f;

    private Text player1WinsText;
    private Text player2WinsText;

    private Text gameOverText;

	private bool gamePaused = false;
	private bool gameEnd = false;

    // Use this for initialization
    void Start ()
    {
		Time.timeScale = 1f;
		player1PendingText = GameObject.Find("Player 1 Pending").GetComponent<Text>();
		player2PendingText = GameObject.Find("Player 2 Pending").GetComponent<Text>();

        player1ScoreText = GameObject.Find("Player 1 Score").GetComponent<Text>();
        player2ScoreText = GameObject.Find("Player 2 Score").GetComponent<Text>();

        player1WinsText = GameObject.Find("Player 1 Wins").GetComponent<Text>();
        player2WinsText = GameObject.Find("Player 2 Wins").GetComponent<Text>();

		player1PendingSlider = GameObject.Find("Player 1 Pending Slider").GetComponent<Slider>();
		player2PendingSlider = GameObject.Find("Player 2 Pending Slider").GetComponent<Slider>();

		gameOverText = GameObject.Find("OutcomeText").GetComponent<Text>();

		gameOverPanel.SetActive (false);
		gamePausePanel.SetActive (false);

        player1 = new Player()
        {
            playerNumber = 1,

			wins = PlayerPrefs.GetInt("Player1Wins",0),
            score = 0,
            comboCount = 0,
			name = "Player 1",

            brickCount = brickCountHelper(GameObject.FindGameObjectWithTag("Bricks1")),
            pendingBricks = 0,

            BrickGroup = GameObject.FindGameObjectWithTag("Bricks1"),
            Paddle = GameObject.FindGameObjectWithTag("Paddle1"),
            Ball = GameObject.FindGameObjectWithTag("Ball1"),

			recentlyAddedLineY = p1AddLineYOffset
        };

        player2 = new Player()
        {
            playerNumber = 2,

			wins = PlayerPrefs.GetInt("Player2Wins",0),
			score = 0,
            comboCount = 0,
			name = "Player 2",

            brickCount = brickCountHelper(GameObject.FindGameObjectWithTag("Bricks2")),
            pendingBricks = 0,

            BrickGroup = GameObject.FindGameObjectWithTag("Bricks2"),
            Paddle = GameObject.FindGameObjectWithTag("Paddle2"),
            Ball = GameObject.FindGameObjectWithTag("Ball2"),

			recentlyAddedLineY = p2AddLineYOffset
        };
        player1WinsText.text = player1.wins.ToString();
        player2WinsText.text = player2.wins.ToString();

        //Start Dropping Powerups after 5 seconds.
        InvokeRepeating("spawnPowerUp" , 5f , 10f);
    }
    int brickCountHelper(GameObject brickGroup)
    {
        int brickCount = 0;
        for (int i = 0; i < brickGroup.transform.childCount; i++)
        {
            brickCount += brickGroup.transform.GetChild(i).childCount;
        }

        return brickCount;
    }
	public void ContinueGame(bool button_input)
	{
		gamePaused = false;
		Time.timeScale = 1;
		gamePausePanel.SetActive (false);
	}
	void PauseGame()
	{
		gamePaused = true;
		Time.timeScale = 0;
		gamePausePanel.SetActive (true);
	}
    // Update is called once per frame
    void Update ()
    {
		if (Time.timeScale == 1f)
		{
			PulsateScores();
		}

		if(Input.GetKeyDown("escape") && gamePaused == false) 
		{
			PauseGame();
		}
		else if (Input.GetKeyDown ("escape") && gamePaused == true) 
		{
			ContinueGame (false);
		}
        //Victory by board clear.
        checkClearVictory(player1, player2);
    }
	void PulsateScores()
	{
		Vector3 textVecP1 = player1ScoreText.transform.localScale;
		Vector3 textVecP2 = player2ScoreText.transform.localScale;

		if (p1Growing) 
		{
			textVecP1.x = textVecP1.x + scaleTextFactor;
			textVecP1.y = textVecP1.y + scaleTextFactor;
			player1ScoreText.transform.localScale = textVecP1;
			if (textVecP1.x >= 1.2f) 
			{
				p1Growing = false;
			}
		}
		if (p2Growing) 
		{
			textVecP2.x = textVecP2.x + scaleTextFactor;
			textVecP2.y = textVecP2.y + scaleTextFactor;
			player2ScoreText.transform.localScale = textVecP2;
			if (textVecP2.x >= 1.2f) 
			{
				p2Growing = false;
			}
		}
		if (p1Growing == false) 
		{
			textVecP1.x = textVecP1.x - scaleTextFactor;
			textVecP1.y = textVecP1.y - scaleTextFactor;
			player1ScoreText.transform.localScale = textVecP1;
			if (textVecP1.x <= 1f) 
			{
				p1Growing = true;
			}
		}
		if (p2Growing == false) 
		{
			textVecP2.x = textVecP2.x - scaleTextFactor;
			textVecP2.y = textVecP2.y - scaleTextFactor;
			player2ScoreText.transform.localScale = textVecP2;
			if (textVecP2.x <= 1f) 
			{
				p2Growing = true;
			}
		}
	}

    //Spawns a powerup to a given player. The disadvantaged player has a better chance of getting the spawned power-up (advantage is based on score).
    void spawnPowerUp()
    {
        //score ratio is the fraction of player1's points to all score earned. scoreRatio > 0.5 means player1 is winning. 
        float scoreRatio = (player1.score + player2.score) == 0 ? 0.5f : player1.score / (player1.score + player2.score);
        //Determines which board will recieve the Power Up
        int playerToRecieve = UnityEngine.Random.value >= scoreRatio ? 1 : 2;

        //Determine Properties for newly instantiated Power Up.
        GameObject paddleToRecieve = GameObject.FindGameObjectWithTag("Paddle" + playerToRecieve);
        int powerUpType = UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(PowerUpKey)).GetLength(0));

        GameObject instantiatedPowerUp = Instantiate(PowerUpPrefab, new Vector3(paddleToRecieve.transform.position.x, 4.5f), Quaternion.identity) as GameObject;
        PowerUp powerUp = instantiatedPowerUp.GetComponent<PowerUp>();

        powerUp.PowerUpPaddle = paddleToRecieve;
        powerUp.powerUpName = (PowerUpKey)powerUpType;

        instantiatedPowerUp.GetComponent<SpriteRenderer>().sprite = null;

        GameObject particles;

        switch (powerUp.powerUpName)
        {
            case PowerUpKey.GrowPaddle:
                particles = Instantiate(Resources.Load<GameObject>("PowerRingGreen"), instantiatedPowerUp.transform.position, Quaternion.identity) as GameObject;
                particles.transform.parent = instantiatedPowerUp.transform; 
                break;
            case PowerUpKey.ShrinkPaddle:
                particles = Instantiate(Resources.Load<GameObject>("PowerRingRed"), instantiatedPowerUp.transform.position, Quaternion.identity) as GameObject;
                particles.transform.parent = instantiatedPowerUp.transform;
                break;
            case PowerUpKey.MultiBall:
                particles = Instantiate(Resources.Load<GameObject>("PowerRingYellow"), instantiatedPowerUp.transform.position, Quaternion.identity) as GameObject;
                particles.transform.parent = instantiatedPowerUp.transform;
                break;
            case PowerUpKey.AddLine:
                particles = Instantiate(Resources.Load<GameObject>("PowerRingBlue"), instantiatedPowerUp.transform.position, Quaternion.identity) as GameObject;
                particles.transform.parent = instantiatedPowerUp.transform;
                break;
            default:
                break;
        }

    }

	//End the game, display the GameOver panel, stop time, and display outcome text
	void gameOver(Player winner, Player loser)
	{
		gameEnd = true;
		print ("GameOver");		
		gameOverPanel.SetActive(true);
        gameOverText.text = winner.name + " wins!";

		winner.wins += 1;
        player1WinsText.text = player1.wins.ToString();
        player2WinsText.text = player2.wins.ToString();

        CancelInvoke("spawnPowerUp");

        Time.timeScale = 0f;

		PlayerPrefs.SetInt("Player1Wins", player1.wins);
		PlayerPrefs.SetInt("Player2Wins", player2.wins);
	}
	
    //Check who has won the game based on number of lines in each player's screen (more conditions to be added)
	void checkClearVictory(Player player1, Player player2)
	{
		if (player1.BrickGroup.transform.childCount <= 0 && gameEnd == false) {
			gameOver(player1, player2);
		}
		if (player2.BrickGroup.transform.childCount <= 0 && gameEnd == false) {
			gameOver(player2, player1);
		}
	}
	void checkLineVictory(Player player1, Player player2)
	{
		//print ("Player1BrickLines :" + player1.BrickGroup.transform.childCount);
		//print ("Player2BrickLines :" + player2.BrickGroup.transform.childCount);

		if (player1.BrickGroup.transform.childCount >= maxLineCount) {
			gameOver(player2, player1);
		}
		if (player2.BrickGroup.transform.childCount >= maxLineCount) {
			gameOver(player1, player2);
		}
	}
    
    // *Messenger Method*
    // Sends bricks to the opponent of 'player' based on the player's current combo. Manages score too.
    void sendBricks(int player)
    {
        Player tempPlayer = player == 1 ? player1 : player2;
        Player victim = player == 2 ? player1 : player2;

        AwardScore(tempPlayer, victim);

		while (tempPlayer.pendingBricks >= bricksPerLine)
		{
            AddLine (tempPlayer, victim);
		}

		player1PendingSlider.value = player2.pendingBricks;
		player2PendingSlider.value = player1.pendingBricks;

		player1PendingText.text = "Pending : " + player2.pendingBricks;
		player2PendingText.text = "Pending : " + player1.pendingBricks;

        player1ScoreText.text = "Score : " + player1.score;
        player2ScoreText.text = "Score : " + player2.score;

        //Victory by Line #
		checkLineVictory(player1, player2);
    }

    // *Messenger Method*
    // Resets combo of a player.
    void resetCombo(int player)
    {
        Player tempPlayer = player == 1 ? player1 : player2;

        tempPlayer.comboCount = 0;
    }

    // Awards Score to a player upon breaking a brick.
    // Adds appropriate amount of bricks based on score.
    void AwardScore(Player tempPlayer ,Player tempVictim)
    {
        //Calculate and award score , increment combo count.
		float brickScore = brickValue + ((scorePerBrick) * (tempPlayer.comboCount/4));
        tempPlayer.score += brickScore;
        tempPlayer.comboCount++;

        int bricksToSend = (int)(brickScore / scorePerBrick);

        if (tempVictim.pendingBricks > 0)
        {
            int initialVictimPendingBricks = tempVictim.pendingBricks;

            tempVictim.pendingBricks = (tempVictim.pendingBricks - bricksToSend) < 0 ? 0 : tempVictim.pendingBricks - bricksToSend;
            bricksToSend = bricksToSend - initialVictimPendingBricks;
            tempPlayer.pendingBricks = bricksToSend > 0 ? tempPlayer.pendingBricks + bricksToSend : tempPlayer.pendingBricks;
        }
        else
        {
            tempPlayer.pendingBricks += (int)(brickScore / scorePerBrick);
        }

        //print((int)(brickScore / scorePerBrick) + " pending bricks added");

        tempPlayer.brickCount--;
    }

	void AddLine(Player tempPlayer , Player victim)
	{
        tempPlayer.pendingBricks -= bricksPerLine;

        Vector3 newBrickGroupPosition = new Vector3(victim.BrickGroup.transform.position.x,
		victim.BrickGroup.transform.position.y - (lineSpaceConst * victim.BrickGroup.transform.localScale.y));

		victim.BrickGroup.transform.position = newBrickGroupPosition;

		Vector3 newLinePositionWithinParent = new Vector3(0, victim.recentlyAddedLineY + lineSpaceConst);
		victim.recentlyAddedLineY += lineSpaceConst;

        //int linePrefabToSpawn = victim.playerNumber == 1 ? UnityEngine.Random.Range(0,LinePrefabs.GetLength(0)/2) : UnityEngine.Random.Range(LinePrefabs.GetLength(0)/2, LinePrefabs.GetLength(0) );
        int linePrefabToSpawn = UnityEngine.Random.Range(0, LinePrefabs.GetLength(0));

        GameObject newLine = Instantiate(LinePrefabs[linePrefabToSpawn], victim.BrickGroup.transform.position, Quaternion.identity) as GameObject;
		newLine.transform.parent = victim.BrickGroup.transform;
		newLine.transform.localPosition = newLinePositionWithinParent;
		newLine.transform.localScale = Vector3.one;	
	}

    void applyPowerUp(applyPowerUpInfo info)
    {
		Player player;

		if (info.powerUpKey == PowerUpKey.ShrinkPaddle) //Players to act upon for ShrinkPaddle Powerup.
		{
			player = info.player == 1 ? player2 : player1;
		} 
		else//Otherwise, keep them in sync
		{
			player = info.player == 1 ? player1 : player2;
		}

        Power.Apply(info, player);
    }


    //MESSENGER METHOD
    //a ball has been destroyed. Update a Player's Ball Game Object. FOR Multi-ball powerup.
    void ballDestroyed(int playerNumber)
    {
        //Player whose ball has been destroyed.
        Player tempPlayer = playerNumber == 1 ? player1 : player2;

        if (tempPlayer.Ball == null || !tempPlayer.Ball.activeInHierarchy)
        {
            print("renewed in ballDestroyed");
            tempPlayer.Ball = GameObject.FindGameObjectWithTag("Ball" + playerNumber);
        }
    }
}

public class Player
{
    public int playerNumber { get; set; }

    public int wins { get; set; }

    public float score { get; set; }
    public int comboCount { get; set; }

    public int brickCount { get; set; }                     

    public int pendingBricks { get; set; }  // Bricks player will send to victim.

    public GameObject BrickGroup { get; set; }
    public GameObject Paddle { get; set; }
    public GameObject Ball { get; set; }

    internal float recentlyAddedLineY;

	public string name { get; set; }

    public Player()
	{
	}

}

public class Power : MonoBehaviour
{
	public static void Apply(applyPowerUpInfo power, Player player)
    {
		switch (power.powerUpKey)
        {
            case PowerUpKey.GrowPaddle:
				GrowPaddle(player, power);
                break;
			case PowerUpKey.ShrinkPaddle:
				ShrinkPaddle(player, power);
				break;
            case PowerUpKey.MultiBall:
				MultiBall(player); if (UnityEngine.Random.value <= 0.5) MultiBall(player);
                break;
            case PowerUpKey.AddLine:
                AddLine(player , power);
                break;
        }
    }

    static void AddLine(Player player, applyPowerUpInfo power)
    {
        GameManager GMref = (GameManager)GameObject.Find("Game Manager").GetComponent<GameManager>();

        int linesToAdd = UnityEngine.Random.Range(1, power.addLineUpperBound);
        int bricksToAdd = (linesToAdd * GMref.bricksPerLine) - ((int)(GMref.brickValue/GMref.scorePerBrick));

        player.pendingBricks += bricksToAdd;

        GameObject.Find("Game Manager").SendMessage("sendBricks", player.playerNumber);
    }

	public static void ResetPaddleToOne(Player player)
	{
		Vector3 newSize = player.Paddle.transform.localScale;
		newSize.x = 1f;

		if (player.Paddle.transform.childCount > 0) 
		{
			if (player.Ball == null)
			{
				player.Ball = GameObject.FindGameObjectWithTag("Ball" + player.playerNumber);
			}

			player.Ball.transform.parent = null;
			player.Paddle.transform.localScale = newSize;
			player.Ball.transform.parent = player.Paddle.transform;
		} 
		else 
		{
			player.Paddle.transform.localScale = newSize;
		}
	}
	static void AdjustPaddleSize(Player player, float scaleAmount)
	{
		Vector3 newSize = player.Paddle.transform.localScale;
		newSize.x = newSize.x + scaleAmount;

		if (player.Paddle.transform.childCount > 0) 
		{
			if (player.Ball == null)
			{
				player.Ball = GameObject.FindGameObjectWithTag("Ball" + player.playerNumber);
			}

			player.Ball.transform.parent = null;
			player.Paddle.transform.localScale = newSize;
			player.Ball.transform.parent = player.Paddle.transform;
		} 
		else 
		{
			player.Paddle.transform.localScale = newSize;
		}
	}
	static void GrowPaddle(Player player, applyPowerUpInfo power)//Grow paddle by scaleConstant when the player collects the GrowPaddle powerup
	{
		if (player.Paddle.transform.localScale.x == 1f) //Increase size of paddle ONLY if paddle is size 1
		{
			AdjustPaddleSize (player, power.scaleAmount);
		}
		if (player.Paddle.transform.localScale.x < 1f)//reset size of paddle if paddle was shrunken
		{
			ResetPaddleToOne (player);	 
		}
	}
	static void ShrinkPaddle(Player player, applyPowerUpInfo power)//Decrease opponents paddle by scaleConstant when player collects ShrinkPaddle powerup
	{
		if (player.Paddle.transform.localScale.x == 1f) //Decrease size of paddle ONLY if paddle is size 1
		{
			AdjustPaddleSize (player, -power.scaleAmount);
		}
		if (player.Paddle.transform.localScale.x > 1f)//reset size of paddle if paddle was grown
		{
			ResetPaddleToOne (player);
		} 
	}

    static void MultiBall(Player player)
    {
        if (player.Ball == null)
        {
            print("renewed in MultiBall");
            player.Ball = GameObject.FindGameObjectWithTag("Ball" + player.playerNumber);
        }

        GameObject newBall = GameObject.Instantiate(player.Ball, player.Ball.transform.position, Quaternion.identity) as GameObject;
        Ball castedNewBall = newBall.GetComponent<Ball>();

        castedNewBall.Player = player.playerNumber;
        castedNewBall.ballInPlay = true;

        castedNewBall.tag = "Ball" + player.playerNumber;

        castedNewBall.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        castedNewBall.GetComponent<Rigidbody2D>().isKinematic = false;

        castedNewBall.GetComponent<Rigidbody2D>().AddForce(new Vector3(Input.GetAxis("Horizontal" + player.playerNumber) * castedNewBall.StartingSpeed, castedNewBall.StartingSpeed));
    }


}