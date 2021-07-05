using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public GameObject HPCubePrefab;
    private int hp = 0;
    public int rotSpeed = 90;
    [Range(0,5)]
    public float dist = 1.3f;
    private Stack<GameObject> cubes;

    void Awake()
    {
        cubes = new Stack<GameObject>();
    }

    void LateUpdate()
    {
        // revert player rotation
        transform.rotation = Quaternion.Euler(0, -transform.rotation.y, 0);
        transform.Rotate(new Vector3(0, rotSpeed * Time.time, 0));
        // updateHP();
    }

    public void SetHP(int hp)
    {
        if (hp > this.hp) addHP(hp - this.hp);
        else if (hp < this.hp) subHP(this.hp - hp);
    }

    public void addHP(int hp)
    {
        while(hp-- > 0) {
            GameObject c = Instantiate(HPCubePrefab, transform.position + dist * Vector3.back, Quaternion.identity, transform);
            cubes.Push(c);
        }
        updateHP();
    }

    public void subHP(int hp)
    {
        while (hp-- > 0) Destroy(cubes.Pop());
        updateHP();
    }

    private void updateHP()
    {
        float dang = 360f / cubes.Count;
        int i = -cubes.Count / 2;
        foreach (GameObject c in cubes)
        {
            float ang = dang * i;
            c.transform.position = transform.position;
            c.transform.Rotate(new Vector3(0, ang - c.transform.localEulerAngles.y, 0));
            c.transform.Translate(dist * Vector3.back);

            i++;
        }
    }
}
