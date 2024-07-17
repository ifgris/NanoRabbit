
/// <summary>
/// Db Config
/// </summary>
public class DbConfig
{
    public RedisConfig RedisConfig { get; set; }
}

public class RedisConfig
{
    public string DbConnStr { get; set; }
}
