
namespace UumuuPlusMod;

class OomasAttach : MonoBehaviour
{
    public float lastShot;
    private void Update() 
    {
        if(Time.time - lastShot < 1.5f) return;
        var hm = GetComponent<HealthManager>();
        if(hm is null || hm.isDead)
        {
            Destroy(this);
            return;
        }
        lastShot = Time.time;
        int b = UnityEngine.Random.Range(0, 90);
        for(int i = 0 ; i < 360 ; i += 45)
        {
            var g = UumuuPlus.slugAtk.Spawn(null, transform.position, Quaternion.identity);
            g.transform.localEulerAngles = new(0, 0, i + b);
        }
    }
}
