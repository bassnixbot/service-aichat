namespace AiChatbotService.Utils;

public static class Config {
    public static OpenAI openAI {get; set;} = new();
        
}

public class OpenAI {
    public string OPENAI_API_KEY {get; set;} = "";
    public string OPENAI_ORGANIZATION_ID {get; set;} = "";
    public string DEFAULT_MODEL {get; set;} = "";
}
