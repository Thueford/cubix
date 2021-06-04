using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E_Archer: EnemyBase
{
    public float startHP;

    // Start is called before the first frame update
    override protected void Start()
    {
        base.Start();
        HP = startHP;
        pnX = Random.Range(int.MinValue, int.MaxValue);
        pnY = Random.Range(int.MinValue, int.MaxValue);
        pnD = Random.Range(int.MinValue, int.MaxValue);
    }

    public const float
        MAXDIST_E = 10,
        MAXDIST_P = 15,
        MAXDIST_P_NOISE = 5,
        MAXDIST_W = 10,

        F_WALLS = -2f,
        F_PERLIN = 0.5f,
        F_PLAYER = 2,
        F_APLAYER = -2.5f,
        F_ENEMIES = -1;

    private const int DIRS = 12;
    private int pnX, pnY, pnD;

    // Update is called once per frame
    override protected void Update()
    {
        base.Update();

        if (movable)
        {
            EnemyBase[] enemies = Player.curStage.GetComponentsInChildren<EnemyBase>();
            Vector3[] diffs = new Vector3[enemies.Length + 2];
            Vector3 dir = Vector3.zero;

            // avoid enemies
            for (int i = 0; i < enemies.Length; ++i)
                diffs[i + 2] = F_ENEMIES * -avoid(enemies[i], MAXDIST_E);

            // move towards player keeping distance
            diffs[0] =
                F_PLAYER * (Player.self.transform.position - transform.position).normalized +
                F_APLAYER * (-2 * avoid(Player.self, MAXDIST_P + MAXDIST_P_NOISE * perlinNoise(0.4f, pnX)));

            diffs[1] = F_PERLIN * new Vector3(perlinNoise(0.4f, pnX), 0, perlinNoise(0.4f, pnY)).normalized;

            // calculate directions
            for (int i = 0; i < DIRS; ++i)
            {
                Vector3 a = Quaternion.Euler(0, 360*i/DIRS, 0) * Vector3.forward;
                float factor = 0;

                foreach(Vector3 d in diffs)
                    factor += d.magnitude * Vector3.Dot(a, d.normalized);

                // avoid walls
                if (Physics.Raycast(transform.position, a, out RaycastHit hitInfo, MAXDIST_W))
                    if (hitInfo.collider.CompareTag("Wall"))
                        factor += F_WALLS * (1 - hitInfo.distance / MAXDIST_W);

                // effective dir
                dir += factor * a;
                dbgLine(a, Mathf.Abs(factor), factor > 0 ? Color.green : Color.red);
            }

            // dir = Quaternion.Euler(0, Vector3.Angle(dir, diffs[0]) + 90 * perlinNoise(0.4f, pnD), 0) * dir;

            // apply direction
            dir = dir.normalized * accelerationForce;
            dbgLine(dir, 2, Color.white);
            rb.AddForce(dir * Time.deltaTime * 1000, ForceMode.Acceleration);
            if (rb.velocity.magnitude > maxSpeed)
                rb.velocity = rb.velocity.normalized * maxSpeed;

            // look at player
            if(diffs[0].magnitude > 0) transform.forward = diffs[0];
        }
    }
}
