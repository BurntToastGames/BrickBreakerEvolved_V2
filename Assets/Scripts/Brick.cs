using UnityEngine;
using System.Collections;
using System;

public class Brick : MonoBehaviour
{
    GameObject DPBlue;
    GameObject DPRed;

    void Start()
    {
        DPBlue = Resources.Load<GameObject>("PickupExplodeBlue");
        DPRed = Resources.Load<GameObject>("PickupExplodeRed");
    }

    //this.transform.parent.parent <- if brick is in line, returns the brickGroup
	void OnCollisionEnter2D(Collision2D col)
    {
        SpawnDP(col);

        //Lets GM know when a brick has been broken.
        GMSendBricks();

        //Deletes a line prefab if all its bricks have been broken.
        if(LineCleanUp())
        {
            ScootLinesUp(this.transform.position.y);
        }

        Destroy(this.gameObject);
    }

    void SpawnDP(Collision2D collision)
    {
        int player = gameObject.transform.parent.parent.gameObject.CompareTag("Bricks1") ? 1 : 2;

        Vector3 contactPoint = collision.contacts[0].point;

        if(player == 1)
            Destroy(Instantiate(DPBlue, contactPoint, Quaternion.identity), 4f);
        else
            Destroy(Instantiate(DPRed, contactPoint, Quaternion.identity), 4f);

    }

    void ScootLinesUp(float brickYPos)
    {
        Transform parentBrickGroup = this.transform.parent.parent;
        Vector2 newPos;
        for (int i = 0; i < parentBrickGroup.childCount; i++)
        { 
            if(parentBrickGroup.transform.GetChild(i).transform.position.y < this.transform.parent.position.y)
            {
                newPos = parentBrickGroup.transform.GetChild(i).transform.localPosition;

                newPos.Set(newPos.x, newPos.y + 1.14f);

                parentBrickGroup.transform.GetChild(i).transform.localPosition = newPos;
            }
        }
        Destroy(this.transform.parent.gameObject);
        this.transform.parent = null;
    }

    void GMSendBricks()
    {
        GameObject brickBlob = this.transform.parent.transform.parent.gameObject;

        if (brickBlob.tag == "Bricks1")
        {
            GameObject.FindGameObjectWithTag("Game Manager").SendMessage("sendBricks", 1);
        }
        else
        {
            GameObject.FindGameObjectWithTag("Game Manager").SendMessage("sendBricks", 2);
        }
    }

    bool LineCleanUp()
    {
        if (this.transform.parent.childCount == 1)
        {
            return true;
        }
        return false;
    }

}
