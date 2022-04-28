[assembly: NeedHKToolVersion(default)]
namespace UumuuPlusMod;


class UumuuPlus : ModBase
{
    public static GameObject jfSpawner;
    public static GameObject jfBaby;
    public static GameObject slugAtk;
    public static AudioClip uAttack;
    public static GameObject roarPrefab;
    public static GameObject exp;
    public static GameObject spit;
    public override void Initialize()
    {
        

        ModHooks.ObjectPoolSpawnHook += (go) =>
        {
            if (go.name.StartsWith("Jellyfish GG") && go.GetComponent<OomasAttach>() is null)
                go.AddComponent<OomasAttach>();
            return go;
        };
    }
    [PreloadSharedAssets(32, "Gas Explosion Recycle L", typeof(GameObject))]
    private void PreloadExplosionRecycle(GameObject go)
    {
        exp = UnityEngine.Object.Instantiate(go);
        exp.transform.parent = null;
        exp.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(exp);
        UnityEngine.Object.Destroy(exp.LocateMyFSM("damages_enemy"));
    }
    [PreloadSharedAssets(32, "Shot Mawlek", typeof(GameObject))]
    private void PreloadSpittingZombie(GameObject go)
    {
        spit = go;
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
    [PreloadSharedAssets("GG_Uumuu", "Roar Wave Emitter", typeof(GameObject))]
    private void PreloadRoar(GameObject go)
    {
        roarPrefab = go;
    }
    [PreloadSharedAssets(161, "Shot Slug Spear", typeof(GameObject))]
    private void PreloadSlugSpear(GameObject go)
    {
        slugAtk = go;
    }
    [PreloadSharedAssets("GG_Uumuu", "mega_laser_burst", typeof(AudioClip))]
    private void PreloadAtkAC(AudioClip clip)
    {
        uAttack = clip;
    }

    public static bool CheckGGUumuu()
    {
        var curS = USceneManager.GetActiveScene();
        return curS.name.StartsWith("GG_Uumuu") || BossSceneController.IsBossScene;
    }
}
