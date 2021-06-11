using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Hunter : EnemyBase
{
    private const float
        MAXDIST_P_NOISE = 10,
        F_PERLIN = 0.5f;

    private int pnX, pnY;

    override public void Awake()
    {
        base.Awake();
        pnX = Random.Range(int.MinValue, int.MaxValue);
        pnY = Random.Range(int.MinValue, int.MaxValue);
    }

    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        HP = startHP;
    }


    // Update is called once per frame
    override public void Update()
    {
        base.Update();
        if (movable)
        {
            EnemyBase[] enemies = Player.curStage.GetComponentsInChildren<EnemyBase>();
            Vector3[] effectors = new Vector3[enemies.Length + 2];

            // avoid enemies
            for (int i = 0; i < enemies.Length; ++i)
                effectors[i + 2] = F_ENEMIES * -avoid(enemies[i], MAXDIST_E);

            // move towards player
            effectors[0] = F_PLAYER * (Player.self.transform.position - transform.position).normalized;
            // noise
            effectors[1] = F_PERLIN * new Vector3(perlinNoise(0.4f, pnX), 0, perlinNoise(0.4f, pnY)).normalized;

            // steer
            Vector3 dir = contextSteer(effectors, F_WALLS, MAXDIST_W).normalized * accelerationForce;

            // apply direction
            rb.AddForce(dir * Time.deltaTime * 1000, ForceMode.Acceleration);
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;

            // look in movement direction
            dir.y = 0;
            if(dir.magnitude > 0) transform.forward = dir;
        }
    }
}
