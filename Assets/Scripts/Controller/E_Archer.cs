using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Archer: EnemyBase
{
    public float startHP;

    public const float
        DIST_P_SHOOT = 20,

        MAXDIST_P = 15,
        MAXDIST_P_NOISE = 10,

        F_PERLIN = 0.5f,
        F_APLAYER = -3f;

    private int pnX, pnY;

    private float timeCounter;
    private float rateOfFire = 1f;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        HP = startHP;

        timeCounter = Random.value;
        pnX = Random.Range(int.MinValue, int.MaxValue);
        pnY = Random.Range(int.MinValue, int.MaxValue);
    }

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();
        if (!movable) return;

        // shoot
        timeCounter += Time.deltaTime;
        // dbgLine(Player.self.transform.position - transform.position, DIST_P_SHOOT, Color.gray);
        if (timeCounter > rateOfFire && Vector3.Distance(Player.self.transform.position, transform.position) < DIST_P_SHOOT)
        {
            timeCounter = 0;
            GetComponent<BulletSpawner>().shoot(transform.forward);
        }

        // Context based steering
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
        transform.forward = Player.self.transform.position - transform.position;
    }
}
