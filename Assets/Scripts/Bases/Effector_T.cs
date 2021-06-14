using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Effector_T
{
    public string tag;
    public float factor, dist;
    public bool linear;

    public Effector_T(string tagName, float POIfactor, float maxdist, bool linear = true)
    {
        tag = tagName; this.linear = linear;
        dist = maxdist; factor = POIfactor;
    }

    private Vector3 attract(Vector3 src, Vector3 dst)
    {
        Vector3 d = dst - src;
        d.y = 0;
        if (d.sqrMagnitude >= dist * dist) return Vector3.zero;
        else return linear ? (1 - d.magnitude / dist) * d.normalized : d.normalized;
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

    public List<Vector3> getEffs(Component src)
    {
        return getEffs(src.gameObject);
    }

    public float getRaycast(GameObject src, Vector3 dir)
    {
        if (Physics.Raycast(src.transform.position, dir, out RaycastHit hitInfo, dist, LayerMask.GetMask(tag)))
            if (hitInfo.collider.CompareTag(tag))
                return linear ? factor * (1 - hitInfo.distance / dist) : factor;
        return 0;
    }

    public float getRaycast(Component src, Vector3 dir)
    {
        return getRaycast(src.gameObject, dir);
    }
}
