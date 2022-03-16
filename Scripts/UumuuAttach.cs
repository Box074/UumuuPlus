
namespace UumuuPlusMod;

[AttachHealthManager]
class UumuuAttach : MonoBehaviour
{
    public GameObject zapPrefab;
    public Rigidbody2D rig;
    public GameObject jfSpawner;
    public GameObject roar;
    public tk2dSpriteAnimator anim;
    public HealthManager hm;
    public AudioSource ac;
    public bool inGG = UumuuPlus.CheckGGUumuu();
    public PlayMakerFSM ctrl;
    public GameObject shield;
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

        using (var patch = ctrl.Fsm.CreatePatch())
        {
            patch
                .EditState("Zapping")
                .ForEachFsmStateActions<FsmStateAction>(_ => null)
                .RemoveTransition("FINISHED")
                .AppendAction(FSMHelper.CreateMethodAction(
                    a =>
                    {
                        StartCoroutine(MakeZap());
                    })
                )
                .AppendAction(
                    new ChaseObjectV2()
                    {
                        gameObject = new()
                        {
                            GameObject = new()
                            {
                                Value = gameObject
                            }
                        },
                        target = new()
                        {
                            Value = HeroController.instance.gameObject
                        },
                        speedMax = new()
                        {
                            Value = 15
                        },
                        accelerationForce = new()
                        {
                            Value = 20
                        },
                        offsetX = new(),
                        offsetY = new()
                    }
                )
                ;
            patch.EditState("Choice")
                .InsertAction(FSMHelper.CreateMethodAction((a) =>
                {
                    if (UnityEngine.Random.Range(0, 100) <= 50)
                    {
                        ctrl.SetState("Temp");
                        StartCoroutine(ChoiceSpecailAtk());
                    }
                }), 0);
            if (inGG)
            {
                jfSpawner = GameObject.Find("Jellyfish Spawner");
                patch
                    .AddStateAndEdit("Temp")
                    .AppendTransition("EXPLODE", "Explode")
                    .EditState("Choice")
                    .ChangeTransition("SPAWN", "Idle")
                    .EditState("Explode")
                    .AppendAction(FSMHelper.CreateMethodAction((a) =>
                    {
                        ShotSplit();
                    }));
            }
            else
            {
                jfSpawner = Instantiate(UumuuPlus.jfSpawner);
                jfSpawner.SetActive(true);
                patch
                    .AddState("Temp")
                    .AddStateAndEdit("Summon")
                    .AppendAction(FSMHelper.CreateMethodAction(
                        (a) =>
                        {
                            FSMUtility.SetFloat(ctrl, "Quirrel Timer", 0);
                            StartCoroutine(Spawn());
                        }
                    ))
                    .EditState("Quirrel?")
                    .DelayBindTransition("QUIRREL", "Summon")
                    .EditState("Wounded")
                    .AppendAction(FSMHelper.CreateMethodAction(
                        (a) =>
                        {
                            ShotSplit();
                        }
                    ))
                    ;
            }
        }
    }
    private IEnumerator Spawn(bool customSummon = false)
    {
        yield return null;
        rig.velocity = Vector2.zero;
        anim.Play("Attack");
        ac.PlayOneShot(UumuuPlus.uAttack);
        roar = Instantiate(UumuuPlus.roarPrefab, transform.position, Quaternion.identity, transform);
        if (!customSummon)
        {
            FSMUtility.SendEventToGameObject(jfSpawner, "SPAWN");
            yield return new WaitForSeconds(1);
            ctrl.Fsm.SetState("Attack Recover");
            FSMUtility.SendEventToGameObject(roar, "END");
        }
    }
    private IEnumerator SpawnSlugAtk()
    {
        yield return Spawn(true);
        int b = UnityEngine.Random.Range(0, 90);
        for (int i = 0; i < 12; i++)
        {
            var g = UumuuPlus.slugAtk.Spawn(null, transform.position, Quaternion.identity);
            g.transform.localEulerAngles = new(0, 0, i * 30 + b);
        }
        yield return new WaitForSeconds(1);
        if(hm.IsInvincible) ctrl.Fsm.SetState("Attack Recover");
        FSMUtility.SendEventToGameObject(roar, "END");
    }
    private IEnumerator MakeZap()
    {
        var st = Time.time;
        yield return null;
        while (Time.time - st < 10 && hm.IsInvincible)
        {
            var zap = zapPrefab.Spawn(null, HeroController.instance.transform.position
                + new Vector3(UnityEngine.Random.Range(-3, 3), UnityEngine.Random.Range(-3, 3), 0), Quaternion.identity);
            yield return new WaitForSeconds(0.25f);
        }
        ctrl.SetState("Idle");
    }
    private void ShotSplit()
    {
        for(int i = 0; i < 9 ; i++)
        {
            var split = UumuuPlus.spit.Spawn(null, transform.position, Quaternion.identity);
            var rig = split.GetComponent<Rigidbody2D>();
            rig.velocity = new Vector2(UnityEngine.Random.Range(-15, 15), UnityEngine.Random.Range(15, 20));
        }
    }
    private IEnumerator SpawnShield()
    {
        shieldCount = 4;
        if (shield is not null) DestroyImmediate(shield);
        shield = null;
        if (!firstShield)
        {

            if (inGG)
            {
                yield return Spawn(false);
                yield return new WaitForSeconds(3.5f);
                while (ctrl.ActiveStateName != "Idle") yield return null;
                ctrl.SetState("Temp");
            }
            else
            {
                ctrl.SetState("Set Timer");
                yield return new WaitForSeconds(1.5f);
                while (ctrl.ActiveStateName != "Idle") yield return null;
                ctrl.SetState("Temp");
            }
        }
        firstShield = false;

        yield return Spawn(true);
        shield = new GameObject("Shield");
        shield.transform.parent = transform;
        shield.transform.localPosition = Vector2.zero;

        GameObject[] shields = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            var s = Instantiate(UumuuPlus.jfBaby, shield.transform.position +
                new Vector3((i == 0 || i == 2) ? -3 : 3, (i == 0 || i == 3) ? -3 : 3, 0), Quaternion.identity
                    , shield.transform);
            shields[i] = s;
            s.SetActive(true);
            s.AddComponent<ShieldAttach>();
            var hm = s.GetComponent<HealthManager>();
            hm.hp = 100;
            hm.OnDeath += () => shieldCount--;
            var col = s.GetComponent<BoxCollider2D>();
            col.enabled = false;
            col.isTrigger = true;
            foreach (var v in s.GetComponentsInChildren<Collider2D>()) v.isTrigger = true;
            s.GetComponent<tk2dSprite>().color = new Color(0, 0, 0, 0);
        }
        var st = Time.time;
        float spd;
        while ((spd = Time.time - st) <= 1)
        {
            foreach (var v in shields) v.GetComponent<tk2dSprite>().color = new Color(1, 1, 1, Mathf.Lerp(0, 1, spd));
            yield return new WaitForFixedUpdate();
        }
        foreach (var v in shields)
        {
            v.GetComponent<tk2dSprite>().color = new Color(1, 1, 1, 1);
            v.GetComponent<BoxCollider2D>().enabled = true;
        }
        yield return new WaitForSeconds(1);
        if (hm.IsInvincible)
        {
            ctrl.Fsm.SetState("Attack Recover");
        }
        FSMUtility.SendEventToGameObject(roar, "END");
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Nail Attack") && hm.IsInvincible)
        {
            Instantiate(UumuuPlus.exp, HeroController.instance.transform.position
                , Quaternion.identity).SetActive(true);
        }
    }
    private IEnumerator ChoiceSpecailAtk()
    {
        yield return null;
        if (UnityEngine.Random.Range(0, 100) <= 50)
        {
            yield return SpawnSlugAtk();
            yield break;
        }
        ctrl.SetState("Idle");
    }
    private void FixedUpdate()
    {
        if (shield is not null) shield?.transform?.Rotate(new Vector3(0, 0, 1), Space.Self);
    }
    private void Update()
    {
        if (ctrl.ActiveStateName == "Idle")
        {
            inited = true;
            hm.IsInvincible = true;
            if (shieldCount <= 0)
            {
                shieldCount = 4;
                StartCoroutine(SpawnShield());
            }
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
}
