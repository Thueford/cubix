using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float stickyness = 10;
    //public float lazyness = 10;
    private Vector3 offset;
    private Vector3 vel = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // transform.position += (player.transform.position + offset - transform.position) * Time.deltaTime * stickyness;
        // transform.position = Vector3.SmoothDamp(transform.position, player.transform.position + offset, ref vel, lazyness/100);
    }
}
