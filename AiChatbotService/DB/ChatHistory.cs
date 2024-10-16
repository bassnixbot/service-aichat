using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AiChatbot.DB;

public class ChatHistory
{
    [Key]
    public Guid recid { get; set; }
    public string itemkey { get; set; }
    public string username { get; set; }
    public string channel { get; set; }
    public DateTime createdateutc { get; set; }
    public DateTime lastupdateutc { get; set; }
    [Column(TypeName = "jsonb")]
    public string messages { get; set; }
    [Column(TypeName = "jsonb")]
    public string logs {get; set;}
}

public class ChatLogs {
    public string username {get; set;}
    public DateTime createdateutc {get; set;}
    public string message {get; set;}
    
}
