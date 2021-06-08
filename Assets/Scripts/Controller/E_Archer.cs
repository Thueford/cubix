using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Archer: EnemyBase
{
    public float startHP;

    public const float
        MAXDIST_P = 15,
        MAXDIST_P_NOISE = 10,

        F_PERLIN = 0.5f,
        F_APLAYER = -3f;

    private int pnX, pnY;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        HP = startHP;

        pnX = Random.Range(int.MinValue, int.MaxValue);
        pnY = Random.Range(int.MinValue, int.MaxValue);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();

        if (movable)
        {
            EnemyBase[] enemies = Player.curStage.GetComponentsInChildren<EnemyBase>();
            Vector3[] effectors = new Vector3[enemies.Length + 2];

            // avoid enemies
            for (int i = 0; i < enemies.Length; ++i)
                effectors[i + 2] = F_ENEMIES * -avoid(enemies[i], MAXDIST_E);

            // move towards player keeping distance
            effectors[0] =
                F_PLAYER * (Player.self.transform.position - transform.position).normalized +
                F_APLAYER * (-2 * avoid(Player.self, MAXDIST_P + MAXDIST_P_NOISE * sinNoise(0.8f, pnX)));

            effectors[1] = F_PERLIN * new Vector3(perlinNoise(0.4f, pnX), 0, perlinNoise(0.4f, pnY)).normalized;

            // steer
            Vector3 dir = contextSteer(effectors, F_WALLS, MAXDIST_W).normalized * accelerationForce;

            // apply direction
            rb.AddForce(dir * Time.deltaTime * 1000, ForceMode.Acceleration);
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;

            // look at player
            //if(effectors[0].magnitude > 0) transform.forward = effectors[0];
            transform.forward = -transform.position + Player.self.transform.position;
        }
    }
}
