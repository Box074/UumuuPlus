[assembly: NeedHKToolVersion(default)]
namespace UumuuPlusMod;


class UumuuPlus : ModBase
{
    public static GameObject jfSpawner;
    public static GameObject jfBaby;
    public static GameObject uumuu;
    public static GameObject uumuuGG;
    public static GameObject slugAtk;
    public static AudioClip uAttack;
    public static GameObject roarPrefab;
    public static GameObject exp;
    public static GameObject spit;
    public override void Initialize()
    {
        exp = UnityEngine.Object.Instantiate(Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(
                x => x.name == "Gas Explosion Recycle L"
                ));
        exp.transform.parent = null;
        exp.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(exp);
        UnityEngine.Object.Destroy(exp.LocateMyFSM("damages_enemy"));

        ModHooks.ObjectPoolSpawnHook += (go) =>
        {
            if (go.name.StartsWith("Jellyfish GG") && go.GetComponent<OomasAttach>() is null)
                go.AddComponent<OomasAttach>();
            return go;
        };
    }
    [Preload("Crossroads_08", "Infected Parent/Spitting Zombie")]
    private void PreloadSpittingZombie(GameObject go)
    {
        spit = go.LocateMyFSM("Spit").Fsm.GetState("Spawn Bullet L")
            .GetFSMStateActionOnState<FlingObjectsFromGlobalPoolVel>().gameObject.Value;
        UnityEngine.Object.Destroy(go);
    }
    [Preload("GG_Uumuu", "Jellyfish Spawner")]
    private void PreloadJFSpawner(GameObject go)
    {
        jfSpawner = go;
    }
    [Preload("Fungus3_archive_02", "Jellyfish Baby (1)")]
    private void PreloadJFBaby(GameObject go)
    {
        jfBaby = go;
    }
    [Preload("GG_Uumuu", "Mega Jellyfish GG")]
    private void PreloadGGUumuu(GameObject go)
    {
        uumuuGG = go;
        var ctrl = go.LocateMyFSM("Mega Jellyfish");
        roarPrefab = ctrl.Fsm.GetState("Roar")
            .GetFSMStateActionOnState<CreateObject>().gameObject.Value;
        uAttack = ctrl.Fsm.GetState("Roar")
            .GetFSMStateActionOnState<AudioPlayerOneShotSingle>().audioClip.Value as AudioClip;
    }
    [Preload("GG_Ghost_Gorb", "Warrior/Ghost Warrior Slug")]
    private void PreloadSlugAtk(GameObject go)
    {
        slugAtk = go.LocateMyFSM("Attacking")
            .Fsm.GetState("Attack").GetFSMStateActionOnState<SpawnObjectFromGlobalPool>().gameObject.Value;
    }
    public static bool CheckGGUumuu()
    {
        var curS = USceneManager.GetActiveScene();
        return curS.name.StartsWith("GG_Uumuu") || BossSceneController.IsBossScene;
    }
}
