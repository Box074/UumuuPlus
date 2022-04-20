
namespace UumuuPlusMod;

class UumuuFsm : CSFsm<UumuuFsm>
{
    [FsmVar("Quirrel Land")]
    public FsmGameObject quirrelLand = new();
    [FsmVar("Quirrel Timer")]
    public FsmFloat quirrelTimer = new();
    public GameObject jfSpawner;
    public HealthManager hm;
    public GameObject zapPrefab;
    public int shieldCount;
    public Rigidbody2D rig;
    public AudioSource ac;
    public tk2dSpriteAnimator anim;
    public GameObject roar;
    public GameObject shield;
    public bool inGG = UumuuPlus.CheckGGUumuu();
    public bool dontSpawnJF = true;
    private void Roar()
    {
        rig.velocity = Vector2.zero;
        anim.Play("Attack");
        ac.PlayOneShot(UumuuPlus.uAttack);
        roar = UnityEngine.Object.Instantiate(UumuuPlus.roarPrefab,
            FsmComponent.transform.position, Quaternion.identity, FsmComponent.transform);
    }
    [FsmState]
    private IEnumerator Idle()
    {
        DefineEvent("SHIELD BREAK", nameof(BreakShield));
        DefineEvent("SLUG ATTACK", nameof(SpawnSlugAtk));
        CopyEvent("ATTACK");
        if (inGG) CopyEvent("EXPLODE");
        var chaseHero = GetOriginalAction<ChaseObjectV2>(0);
        yield return StartActionContent;
        InvokeAction(chaseHero);
        if (shieldCount <= 0 || (shield?.transform?.childCount ?? 0) == 0)
        {
            yield return "SHIELD BREAK";
        }
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.35f, 1.25f));
        if (UnityEngine.Random.Range(0, 100) < 50)
        {
            yield return "SLUG ATTACK";
        }
        else
        {
            yield return "ATTACK";
        }
    }
    [FsmState]
    private IEnumerator QuirrelRoam()
    {
        SetName("Quirrel Roam");
        DefineEvent("FINISHED", nameof(SpawnShield));
        yield return StartActionContent;
        FSMUtility.SetBool(quirrelLand.Value.LocateMyFSM("Watch"), "Roam", true);
    }
    [FsmState]
    private IEnumerator SpawnJF()
    {
        DefineEvent("FINISHED", inGG ? nameof(SpawnShield) : "Attack Recover");
        quirrelTimer.Value = 0;
        yield return StartActionContent;
        Roar();
        FSMUtility.SendEventToGameObject(jfSpawner, "SPAWN");
        yield return new WaitForSeconds(1);
        FSMUtility.SendEventToGameObject(roar, "END");
    }
    [FsmState]
    private IEnumerator Zapping()
    {
        DefineEvent("FINISHED", "Idle");
        DefineEvent("EXPLODE", "Explode");
        var chaseHero = new ChaseObjectV2()
        {
            gameObject = new()
            {
                GameObject = new()
                {
                    Value = FsmComponent.gameObject
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
        };
        yield return StartActionContent;

        var st = Time.time;
        InvokeAction(chaseHero);
        while (Time.time - st < 10 && hm.IsInvincible)
        {
            var zap = zapPrefab.Spawn(null, HeroController.instance.transform.position
                + new Vector3(UnityEngine.Random.Range(-3, 3), UnityEngine.Random.Range(-3, 3), 0), Quaternion.identity);
            yield return new WaitForSeconds(0.25f);
        }
    }
    [FsmState]
    private IEnumerator SpawnSplit()
    {
        DefineGlobalEvent("EXPLODE SPLIT");
        yield return StartActionContent;
        for (int i = 0; i < 9; i++)
        {
            var split = UumuuPlus.spit.Spawn(null, FsmComponent.transform.position, Quaternion.identity);
            var rig = split.GetComponent<Rigidbody2D>();
            rig.velocity = new Vector2(UnityEngine.Random.Range(-15, 15), UnityEngine.Random.Range(15, 20));
        }
    }
    [FsmState]
    private IEnumerator BreakSpawnShield()
    {
        DefineEvent("FINISHED", "Explode");
        yield return StartActionContent;

        if (shield != null) UnityEngine.Object.DestroyImmediate(shield);
        shield = null;
        shieldCount = 0;
        dontSpawnJF = true;
    }
    [FsmState]
    private IEnumerator BreakShield()
    {
        DefineGlobalEvent("UUMUU SHIELD BREAK");
        DefineEvent("SPAWN JF", nameof(SpawnJF));
        DefineEvent("SPAWN SHIELD", nameof(SpawnShield));
        if (!inGG) DefineEvent("CALL QUIRREL", "Set Timer");
        yield return StartActionContent;
        rig.velocity = Vector2.zero;
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.25f, 1.5f));
        if (inGG)
        {
            if (dontSpawnJF)
            {
                yield return "SPAWN SHIELD";
            }
            else
            {
                yield return "SPAWN JF";
            }
        }
        else
        {
            yield return "CALL QUIRREL";
        }

    }
    [FsmState]
    private IEnumerator SpawnShield()
    {
        DefineEvent("EXPLODE", nameof(BreakSpawnShield));
        DefineEvent("FINISHED", "Attack Recover");
        yield return StartActionContent;
        Roar();
        dontSpawnJF = false;
        shieldCount = 4;
        if (shield != null) UnityEngine.Object.DestroyImmediate(shield);
        shield = null;

        shield = new GameObject("Shield");
        shield.AddComponent<ShieldRotate>();
        shield.transform.parent = FsmComponent.transform;
        shield.transform.localPosition = Vector2.zero;

        GameObject[] shields = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            var s = UnityEngine.Object.Instantiate(UumuuPlus.jfBaby, shield.transform.position +
                new Vector3((i == 0 || i == 2) ? -3 : 3, (i == 0 || i == 3) ? -3 : 3, 0), Quaternion.identity
                    , shield.transform);
            shields[i] = s;
            s.SetActive(true);
            s.AddComponent<ShieldAttach>();
            var hm = s.GetComponent<HealthManager>();
            hm.hp = 30;
            hm.OnDeath += () =>
            {
                shieldCount--;
                if(shieldCount <= 0)
                {
                    FsmComponent.SendEvent("UUMUU SHIELD BREAK");
                }
            };
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
            yield return null;
        }
        foreach (var v in shields)
        {
            v.GetComponent<tk2dSprite>().color = new Color(1, 1, 1, 1);
            v.GetComponent<BoxCollider2D>().enabled = true;
        }
        yield return new WaitForSeconds(1);
        FSMUtility.SendEventToGameObject(roar, "END");
    }
    [FsmState]
    private IEnumerator SpawnSlugAtk()
    {
        DefineEvent("FINISHED", "Attack Recover");
        DefineGlobalEvent("DO SLUG ATK");
        yield return StartActionContent;
        Roar();
        int b = UnityEngine.Random.Range(0, 90);
        for (int i = 0; i < 12; i++)
        {
            var g = UumuuPlus.slugAtk.Spawn(null, FsmComponent.transform.position, Quaternion.identity);
            g.transform.localEulerAngles = new(0, 0, i * 30 + b);
        }
        yield return new WaitForSeconds(1);
        FSMUtility.SendEventToGameObject(roar, "END");
    }
}
