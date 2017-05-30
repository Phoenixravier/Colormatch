﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : ExtendedBehaviour {

	public Team roomTeam = Team.neutral;
	public int roomValue = 5;
    public int nbEnemies;
    public GameObject enemy;
    public LayerMask playerInteraction;

    [HideInInspector]
    public Vector2 roomSize;

	Transform firstTile;

    private GameObject positionOverlay;
    private GameObject button;
    moveCamera mC;
    moveCamera mC2;

    private GameObject overlay;
	// Use this for initialization
	void Awake () {
        mC2 = GameObject.FindGameObjectWithTag("Camera2").GetComponent<moveCamera>();
        mC = GameObject.FindGameObjectWithTag("Camera1").GetComponent<moveCamera>();
        positionOverlay = transform.Find("PositionOverlay").gameObject;
        positionOverlay.GetComponent<SpriteRenderer>().enabled = false;
        roomSize = getSize();
		firstTile = transform.GetChild (1);
        if(transform.Find("Overlay") != null)
        overlay = transform.Find("Overlay").gameObject;
        if (enemy != null)
        {
            makeEnemies(enemy, nbEnemies);
        }
    }
	
	// Update is called once per frame
	void Update () {
    }

    void OnTriggerExit2D(Collider2D c)
    {
		if (c.CompareTag("Player")) {
        	GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        	foreach (GameObject player in players)
        	{
				if (isInRoom (player))
					break;
				else {
					resetState ();
				}
        	}
		}
		else if (!c.CompareTag("goaway")) {
			c.GetComponent<Collider2D> ().enabled = false;
			c.transform.position = getCentre ();
			c.GetComponent<Collider2D> ().enabled = true;
		}
    }
		

    void OnTriggerEnter2D(Collider2D c)
    {
        Health h = c.GetComponent<Health>();
		if (c.CompareTag("Player") && h != null && h.getTeam() == Team.blue)
        {
            mC.focusOnRoom(this.gameObject);
        }
		if (c.CompareTag("Player") && h != null && h.getTeam() == Team.green)
        {
            mC2.focusOnRoom(this.gameObject);
        }
    }

    public bool isInRoom(GameObject thing)
    {
        return (Mathf.Abs(thing.transform.position.x - this.transform.position.x) < (getSize().x / 2) &&
                Mathf.Abs(thing.transform.position.y - this.transform.position.y) < (getSize().y / 2));
    }

    void makeEnemies(GameObject enemy, int nbEnemies)
    {   //make not spawn on spawntile
        Vector2 wallDims = getTileSize(this.transform.Find("Wall").gameObject);
        Vector2 enemyDims = getTileSize(enemy);
        Vector2 smallestCoordsInRoom = getMinPoint() + wallDims + enemyDims / 2;
        Vector2 biggestCoordsInRoom = getMaxPoint() - wallDims - enemyDims / 2;
        CircleCollider2D collider = enemy.GetComponent<CircleCollider2D>(); //maybe this is useless?

        for (int i = 0; i < nbEnemies; i++)
        {
            GameObject[] tiles = findChildObjectsByTag("Tile");
            GameObject tile = tiles[Random.Range(0, tiles.Length)];
            Vector3 position = tile.transform.position;
            Vector3 castingPosition = position + new Vector3(0, 0, -10);
            Vector3 targetPosition = castingPosition + new Vector3(0, 0, +20);

            
            collider.enabled = false; //maybe this is useless?
            RaycastHit2D hit = Physics2D.Raycast(castingPosition, new Vector3(0,0,1));
            collider.enabled = true; //maybe this is useless?

            //Instantiate(enemy, position, Quaternion.identity);
            if (hit.collider != null)
            {
                i -= 1;
            }
            else
            {
                GameObject monster = Instantiate(enemy, position, Quaternion.identity);
                monster.transform.parent = transform;
            }
        }
    }


    public void resetState()
        /*sets room to have 
         * no visible positionOverlay and have their button unpressed*/
    {
            //set positionOverlay to invisible
            positionOverlay = this.transform.Find("PositionOverlay").gameObject;
            positionOverlay.GetComponent<SpriteRenderer>().enabled = false;

        //set button animation to unpressed
        if (transform.Find("Button") != null)
        {
            button = transform.Find("Button").gameObject;
            Animator buttonAnim = button.GetComponent<Animator>();
            buttonAnim.SetBool("ButtonOn", false); //changes anim to unpushed
        }
    }

	public void ChangeTiles(Color color, string tag){
		/*Changes all child tiles with a specified tag to a specified color.
		 */
		foreach (Transform child in transform)
			if (child.gameObject.CompareTag(tag))
				child.gameObject.GetComponent<SpriteRenderer>().color = color;
        //overlay.GetComponent<SpriteRenderer>().color = color;
	}

	public void setRoomTeam(Team t){
		/*Worthless setter atm :)
		 */
		roomTeam = t;
	}

	public Vector2 getSize() {
		/*Gets size of room
		 */

		//special case checks :))))
		if (firstTile == null)
			firstTile = transform.GetChild (1);
		//get size of a single tile, for magical variable purposes


		//find max, min points
		Vector2 min = getMinPoint();
		Vector2 max = getMaxPoint ();


		//get height and width
		float height = Mathf.Abs(max.y - min.y);
		float width = Mathf.Abs (max.x - min.x);

		return new Vector2 (width, height); //dimensions of room
	}

	public Vector2 getMinPoint(){
		/*Gets minimum point of room.
		 */ 
		if (firstTile == null)
			firstTile = transform.GetChild (1);

		Vector2 tileSize = getTileSize(firstTile.gameObject);
		float xmin = firstTile.position.x;
		float ymin = firstTile.position.y;

		foreach (Transform tile in transform) {
			float xtile = tile.position.x;
			float ytile = tile.position.y;

			if (xtile < xmin)
				xmin = xtile;
			if (ytile < ymin)
				ymin = ytile;

		}
		return new Vector2 (xmin - tileSize.x/2, ymin - tileSize.y/2);

	}

	public Vector2 getMaxPoint(){
		/* Gets maximum point of room.
		 */

		if (firstTile == null)
			firstTile = transform.GetChild (1);

		Vector2 tileSize = getTileSize(firstTile.gameObject);
		
		float ymax = firstTile.position.y;
		float xmax = firstTile.position.x;

		foreach (Transform tile in transform)
		{
			float xtile = tile.position.x;
			float ytile = tile.position.y;


			if (ytile > ymax)
				ymax = ytile;
			if (xtile > xmax)
				xmax = xtile;      
		}
		return new Vector2 (xmax + tileSize.x/2, ymax + tileSize.y/2);
	}

	public Vector2 getCentre(){
		/*Gets the centre point :)
		 */
		Vector2 min = getMinPoint();
		Vector2 size = getSize();

		return new Vector2 (min.x + (size.x / 2), min.y + (size.y / 2));
	}

	public Transform getClosestCentreTile(){
		Vector2 centre = getCentre();


		Debug.Log (centre);
		if (firstTile == null)
			firstTile = transform.GetChild (1);

		Transform closestobj = firstTile;
		float closest = Vector2.Distance(centre, firstTile.position);

		foreach (Transform tile in transform)
		{
			if (Vector2.Distance(centre, tile.position) < closest){
				closest = Vector2.Distance(centre, tile.position);
				closestobj = tile;
			}
				
		}
		return closestobj;
	}

	public Vector2 diffBetweenCentre(Transform tile){
		return transform.position - tile.position;
	}

	public Vector2 diffBetweenCentres(){
		return (Vector2) transform.position - getCentre ();
	}

		
}