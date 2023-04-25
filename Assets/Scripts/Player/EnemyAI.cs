using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    
    Transform player;
    [SerializeField]
    float enemySpeed, nearbydistance, Gobackspeed;
    Vector3 StartPosition; // Current position of the enemy is stored here

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        nearbydistance = Vector3.Distance(transform.position, player.position);
        if(nearbydistance <= 8f) 
        {
            Chase();
        }
        if(nearbydistance > 8f) 
        {
            GoBackToPatrol();
        }
    }

    void Chase() 
    {
        transform.LookAt(player);
        transform.Translate(0, 0, enemySpeed * Time.deltaTime);
    }

    void GoBackToPatrol() 
    {
        transform.LookAt(StartPosition);
        transform.position = Vector3.Lerp(transform.position, StartPosition, Gobackspeed);
        
    }
}
