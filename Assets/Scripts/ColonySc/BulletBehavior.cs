using UnityEngine;

public class BulletBehavior : MonoBehaviour
{
    public Transform TargetT = null;
    private bool wasSet = false;
    private TagsEnum tagE;

    [Header("stats")]
    public BulletsE type = BulletsE.None;
    public float mySpeed = 100f;
    public int dmg = 20;
    private float rotateSpeed = 7f;

    public void Set(Transform _target, BulletsE _type)
    {
        TargetT = _target;
        type = _type;

        if (type == BulletsE.Laser) { HitTarget(); Destroy(gameObject); return; }

        wasSet = true;
        transform.parent = BulletManager.instance.transform;

        tagE = WorldMenager.instance.TagToTagEnum(TargetT.tag);

        rotateSpeed = rotateSpeed * mySpeed / 50f;
    }

    void Update()
    {
        if (wasSet == false) { return; }

        if (TargetT == null)
        {
            TargetT = BulletManager.instance.GetTransNearTarget(transform.position, tagE);
            if (TargetT == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        Vector2 dir = TargetT.position - transform.position;
        float disThisFrame = mySpeed * Time.deltaTime;
        float disToTarget = dir.magnitude;
        if (disToTarget <= disThisFrame)
        {
            transform.Translate(disToTarget, 0, 0, Space.World);
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * disThisFrame, Space.World);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotateSpeed * Time.deltaTime);
    }
    private void HitTarget()
    {
        if (TargetT == null) { Destroy(gameObject); return; }

        BulletManager.instance.HitSmt(TargetT, dmg);

        Destroy(gameObject);
    }
}
