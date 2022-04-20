
namespace UumuuPlusMod;

[AttachHealthManager]
class UumuuAttach : MonoBehaviour
{
    public GameObject zapPrefab;
    public Rigidbody2D rig;
    public GameObject roar;
    public tk2dSpriteAnimator anim;
    public HealthManager hm;
    public AudioSource ac;
    public bool inGG = UumuuPlus.CheckGGUumuu();
    public PlayMakerFSM ctrl;
    public bool inited = false;
    public int shieldCount = 0;
    private bool firstShield = true;
    private void Awake()
    {
        if (!gameObject.name.StartsWith("Mega Jellyfish")) Destroy(this);
    }
    private void Start()
    {
        ac = GetComponent<AudioSource>();
        anim = GetComponent<tk2dSpriteAnimator>();
        hm = GetComponent<HealthManager>();
        hm.hp = 900;
        rig = GetComponent<Rigidbody2D>();

        ctrl = gameObject.LocateMyFSM("Mega Jellyfish");
        
        zapPrefab = ctrl.Fsm.GetFSMStateActionOnFSM<SpawnObjectFromGlobalPool>().gameObject.Value;
        
        var fsmCtrl = UumuuFsm.Apply(ctrl);
        fsmCtrl.ac = ac;
        fsmCtrl.anim = anim;
        fsmCtrl.hm = hm;
        fsmCtrl.rig = rig;
        fsmCtrl.zapPrefab = zapPrefab;

        using (var patch = ctrl.Fsm.CreatePatch())
        {
            if (inGG)
            {
                fsmCtrl.jfSpawner = GameObject.Find("Jellyfish Spawner");
                patch
                    .AddStateAndEdit("Temp")
                    .AppendTransition("EXPLODE", "Explode")
                    .EditState("Choice")
                    .ChangeTransition("SPAWN", "Idle")
                    .EditState("Explode")
                    .AppendAction(FSMHelper.CreateMethodAction((a) =>
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            var split = UumuuPlus.spit.Spawn(null, transform.position, Quaternion.identity);
                            var rig = split.GetComponent<Rigidbody2D>();
                            rig.velocity = new Vector2(UnityEngine.Random.Range(-15, 15), UnityEngine.Random.Range(15, 20));
                        }
                    }));
            }
            else
            {
                fsmCtrl.jfSpawner = Instantiate(UumuuPlus.jfSpawner);
                fsmCtrl.jfSpawner.SetActive(true);
                patch
                    .AddState("Temp")
                    .EditState("Quirrel?")
                    .DelayBindTransition("QUIRREL", "SpawnJF")
                    .EditState("Wounded")
                    .AppendAction(FSMHelper.CreateMethodAction(
                        (a) =>
                        {
                            for (int i = 0; i < 9; i++)
                            {
                                var split = UumuuPlus.spit.Spawn(null, transform.position, Quaternion.identity);
                                var rig = split.GetComponent<Rigidbody2D>();
                                rig.velocity = new Vector2(UnityEngine.Random.Range(-15, 15), UnityEngine.Random.Range(15, 20));
                            }
                        }
                    ))
                    ;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Nail Attack") && hm.IsInvincible)
        {
            Instantiate(UumuuPlus.exp, HeroController.instance.transform.position
                , Quaternion.identity).SetActive(true);
        }
    }
    

}
class ShieldRotate : MonoBehaviour
{
    private void FixedUpdate()
    {
        transform.Rotate(new Vector3(0, 0, 1), Space.Self);
    }
}
class ShieldAttach : MonoBehaviour
{
    public static float offsetPos = 4.5f;
    private void Update()
    {
        var pos = transform.localPosition;
        if (pos.x != offsetPos && pos.x != -offsetPos) pos.x = pos.x < 0 ? -offsetPos : offsetPos;
        if (pos.y != offsetPos && pos.y != -offsetPos) pos.y = pos.y < 0 ? -offsetPos : offsetPos;
        transform.localPosition = pos;
    }
}