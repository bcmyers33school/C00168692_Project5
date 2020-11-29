using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior {EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    //pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    //properties
    public float speed = 0.005f;
    public float visionDistance = 5;
    public int maxCounter = 5;
    protected int playerCloseCounter = 0;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1; 

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        pathFinder = new PathFinder();
        playerGameObject = GameObject.FindWithTag("Player");
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            //Debug.Log("Enemy stopped since the player has reached the goal or the player is dead");
            return;
        }

        switch(behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }

    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Dumb Enemy: Keeps Walking in Random direction, Will not chase player
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                
                //Changed the color to white to differentiate from other enemies
                material.color = Color.white;
                
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed);
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= speed)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Enemy chases the player when it is nearby
    private void HandleEnemyBehavior2()
    {
        Player player = GameObject.FindObjectOfType<Player>();
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                material.color = Color.blue; // Default enemy color is blue
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position += (velocity.normalized * speed);

                //if target tile reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= speed)
                {
                    //update current tile
                    currentTile = targetTile;
                }

                // If the player is within the vision of the enemy, chase it.
                if (Vector3.Distance(transform.position, 
                        player.gameObject.transform.position) < 
                    visionDistance)
                {
                    path.Clear();
                    state = EnemyState.CHASE;
                }
                else state = EnemyState.DEFAULT; 

                break;

            case EnemyState.CHASE:
                material.color = Color.red; // Changes the enemy color to red when chasing the player
                
                if (path.Count <= 0)
                {
                    path = pathFinder.FindPathAStar(currentTile, 
                    player.currentTile);
                }
                
                if (path.Count > 0) targetTile = path.Dequeue();
                state = EnemyState.MOVING;
                break;

            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Third behavior (Describe what it does)
    private void HandleEnemyBehavior3()
    {
        Player player = GameObject.FindObjectOfType<Player>();
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                material.color = Color.blue; // Default enemy color is blue
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position += (velocity.normalized * speed);

                //if target tile reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= speed)
                {
                    //update current tile
                    currentTile = targetTile;
                }

                // If the player is within the vision of the enemy, chase it.
                if (Vector3.Distance(transform.position, 
                        player.gameObject.transform.position) < 
                    visionDistance)
                {
                    path.Clear();
                    state = EnemyState.CHASE;
                }
                else state = EnemyState.DEFAULT; 

                break;

            case EnemyState.CHASE:
                material.color = Color.red; // Changes the enemy color to red when chasing the player

                List<Tile> adjacentTiles = new List<Tile>();
                foreach (Tile adjacent in player.currentTile.Adjacents)
                {
                    adjacentTiles.Add(adjacent);
                    foreach (Tile adjacent2 in adjacent.Adjacents)
                    {
                        adjacentTiles.Add(adjacent2);
                    }
                }
                System.Random rnd = new System.Random();
                int index = rnd.Next(adjacentTiles.Count);
                Tile playerAdjacentTile = adjacentTiles[index];
                if (path.Count <= 0)
                {
                    path = pathFinder.FindPathAStar(currentTile, 
                    playerAdjacentTile);
                }
                
                if (path.Count > 0) targetTile = path.Dequeue();
                state = EnemyState.MOVING;
                break;

            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }
}
