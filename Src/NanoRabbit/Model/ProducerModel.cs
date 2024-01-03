using System.Collections.Concurrent;

namespace NanoRabbit.Model;

/// <summary>
/// Resend message model
/// </summary>
public class ResendMsgModel
{
    public ConcurrentQueue<MsgInfoModel>? MessageList { get; set; }
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
    /// Message object
    /// </summary>
    public dynamic? Message { get; set; }
}