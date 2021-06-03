using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public GameObject player;
    public float stickyness = 10;
    //public float lazyness = 10;
    private Vector3 offset;
    private Vector3 vel = Vector3.zero;
    public Vector3 target;


    // Start is called before the first frame update
    void Start()
    {
        // target = transform.position;
        // offset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position += (target - transform.position) * Time.deltaTime * stickyness;
        // transform.position += (player.transform.position + offset - transform.position) * Time.deltaTime * stickyness;
        // transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + offset, ref vel, lazyness/100);
    }
}