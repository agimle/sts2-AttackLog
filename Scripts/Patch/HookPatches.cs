using HarmonyLib;

namespace AttackLog;

public class HookPatches
{
    #region 单例模式初始化
    private static HookPatches? _instance;
    public HookPatches Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new HookPatches();
            }
            return _instance;
        }
    }
    
    private HookPatches()
    {
        
    }
    #endregion
    
}