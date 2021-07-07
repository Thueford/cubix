using System;
using System.Collections;
using UnityEngine;

public class ResParts : MonoBehaviour
{
    private Particles ps;
    public int count = 2;

    private void Awake()
    {
        ps = GetComponent<Particles>();
        ps.properties.maxParts = count;
    }

    private void Update()
    {
        if (ps.properties.enabled)
        {
            ps.attractor = Player.self.pos;
            ps.attractor.w = Vector3.Distance(Player.self.pos, transform.position);
        }
    }

    public static void Spawn(Vector3 pos, int count, Color color)
    {
        ResParts rp = Instantiate(Ressource.self.resPartsPrefab, pos, Quaternion.identity);
        rp.count = count;
        rp.ps.color.color = GameState.getLightColor(color);
        rp.ps.color.color2 = (0.3f*rp.ps.color.color + 0.7f*Color.white);
    }
}