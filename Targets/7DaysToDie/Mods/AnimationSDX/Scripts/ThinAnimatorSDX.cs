using UnityEngine;


// ThinAnimatorSDX, Mods
class ThinAnimatorSDX : AvatarZombie01Controller
{
    protected override void Awake()
    {
        base.Awake();
        AddTransformRefs(this.modelTransform);
    }
    private void AddTransformRefs(Transform t)
    {
        if (t.GetComponent<Collider>() != null && t.GetComponent<RootTransformRefEntity>() == null)
        {
            RootTransformRefEntity root = t.gameObject.AddComponent<RootTransformRefEntity>();
            root.RootTransform = this.transform;
            // Debug.Log("Added root ref on " + t.name + " tag " + t.tag); 
        }
        foreach (Transform tran in t)
        {
            AddTransformRefs(tran);
        }
    }

}

