using AttackLog.Model;
using AttackLog.Save;

namespace AttackLog.State;

public class AttackLogRunState
{
    public RunLog? RunLog { get; set; }
    public ModSave? ModSave { get; set; }

    public void Reset()
    {
        RunLog = null;
        ModSave = null;
    }
}
