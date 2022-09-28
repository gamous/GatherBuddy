namespace GatherBuddy.Enums;

public enum NodeType : byte
{
    Unknown   = 0xFF,
    Regular   = 0,
    Unspoiled = 1,
    Ephemeral = 2,
    Legendary = 3,
};
public static class NodeTypeExtension
{
    public static string ToName(this NodeType type)
    {
        return type switch
        {
            NodeType.Regular   =>"常规",
            NodeType.Unspoiled =>"全新",
            NodeType.Ephemeral =>"限时",
            NodeType.Legendary =>"传说",
            _ => "未知֪",
        };
    }
}
