using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEditor.VersionControl;
using UnityEngine;

public class DalekController : MonoBehaviour
{
    
    public Transform[] points; // array of transforms - different points on the patrol path
    private int destPoint = 0;
    private DalekController _dalek;
    [SerializeField] private Transform Player;
    private Vector3 _dalekVelocity;
    public float speed = 1f;
    public float distance;
    private Vector3 targetPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        _dalek = GetComponent<DalekController>();
        GotoNextPoint();
        
    }
    void GotoNextPoint()
    {
        if (points.Length == 0)
            return;
        // returns if no points have been set up

        targetPosition = points[destPoint].position; // sets the titan's next point to go to
        destPoint = (destPoint + 1) % points.Length; // sets the next point in the array as the destination
        // cycles back to the start of the array after the last point
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPosition = _dalek.transform.position;
        //first, check to see if we're close enough to the target
        //targetPosition = Player.transform.position; // if uncommented, this makes the dalek just fly to the player always
        distance = Vector3.Distance(currentPosition, targetPosition);
        if (distance > 0.5f)
        {
            Vector3 directionOfTravel = targetPosition - currentPosition;
            //now normalize the direction, since we only want the direction information
            directionOfTravel.Normalize();
            //scale the movement on each axis by the directionOfTravel vector components
            _dalek.transform.Translate(
                (directionOfTravel.x * speed * Time.deltaTime),
                (directionOfTravel.y * speed * Time.deltaTime),
                (directionOfTravel.z * speed * Time.deltaTime),
                Space.World);
        }
        else
        {
            GotoNextPoint();
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Chase();
        }
        
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Chase();
            Fire();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScanFor(other);
        }
    }

    private void ScanFor(Collider other)
    {
        throw new System.NotImplementedException();
        // Search algorithm
    }

    private void Fire()
    {
        // Fire projectile in player's direction
        throw new System.NotImplementedException();
    }

    void Chase()
    {
        // greedy first/A* path to player's location
        targetPosition = Player.transform.position;
    }
}
