
/// <summary>
/// 卡片状态
/// </summary>
public enum CardStatus
{
    InActive,  //未激活
    Active, //激活
    Prepare,    // 准备状态
    PrepareCompleted,
    RunIn,  // 入场状态
    RunInCompleted,
    Run,    // 运行状态
    RunCompleted,
    RunOut, // 出场状态
    RunOutCompleted,
    End //结束状态
}
