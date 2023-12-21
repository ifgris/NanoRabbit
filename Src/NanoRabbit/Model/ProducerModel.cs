namespace NanoRabbit.Model;

/// <summary>
/// Resend message model
/// </summary>
public class ResendMsgModel
{
    public List<MsgInfoModel>? MessageList { get; set; }
}

public class MsgInfoModel
{
    /// <summary>
    /// Message UUID
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Failed-sending message generate time
    /// </summary>
    public DateTime GenerateTime { get; set; }

    /// <summary>
    /// Retry count
    /// </summary>
    public int RetryCount { get; set; } = 0;
    
    /// <summary>
    /// Message object
    /// </summary>
    public dynamic Message { get; set; }
}