using System.Collections.Generic;
using UnityEngine;

public enum EffectorType { BOOL, LINEAR, QUADRATIC };

[System.Serializable]
public struct Effector_T
{
    public string tag;
    public float factor, dist;
    EffectorType type;

    public Effector_T(string tagName, float POIfactor, float maxdist, EffectorType type = EffectorType.LINEAR)
    {
        tag = tagName; this.type = type;
        dist = maxdist; factor = POIfactor;
    }

    private float getTyped(float f)
    {
        f = 1 - f;
        switch (type)
        {
            case EffectorType.BOOL: f = f > 1 ? 0 : 1; break;
            case EffectorType.LINEAR: break;
            case EffectorType.QUADRATIC: f = Mathf.Pow(f, 2); break;
            default: return 0;
        }
        return Mathf.Clamp(f, 0, 1);
    }

    private Vector3 attract(Vector3 src, Vector3 dst)
    {
        Vector3 d = dst - src;
        d.y = 0;
        return getTyped(d.magnitude / dist) * d.normalized;
    }

    public Vector3 getEff(GameObject src, GameObject dst)
    {
        return factor * attract(src.transform.position, dst.transform.position);
    }

    public Vector3 getEff(Component src, Component dst)
    {
        return getEff(src.gameObject, dst.gameObject);
    }

    public List<Vector3> getEffs(GameObject src)
    {
        GameObject[] arr = GameObject.FindGameObjectsWithTag(tag);
        List<Vector3> effs = new List<Vector3>(arr.Length);
        foreach (GameObject dst in arr) effs.Add(getEff(src, dst));
        return effs;
    }

    public List<Vector3> getEffs(Component src) => getEffs(src.gameObject);

    public float getRaycast(GameObject src, Vector3 dir)
    {
        if (Physics.Raycast(src.transform.position, dir, out RaycastHit hitInfo, dist, LayerMask.GetMask(tag)))
            if (hitInfo.collider.CompareTag(tag))
                return factor * getTyped(hitInfo.distance / dist);
        return 0;
    }

    public float getRaycast(Component src, Vector3 dir)
    {
        return getRaycast(src.gameObject, dir);
    }
}
