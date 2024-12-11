using System.Text.Json;
using AiChatbot.DB;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using UtilsLib;

namespace AiChatbot.Controller;

public static class AIChatEndPoints
{
    public static void MapChatBotEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/openai/gpt", OpenAI_GPT);
    }

    public static async Task<IResult> OpenAI_GPT(ApplicationDBContext dbcontext, ClientInfo request)
    {
        Console.WriteLine($"env data - {OpenAIAuthentication.LoadFromEnv()}");
        var apiresponse = new ApiResponse<string>();
        using (
            var api = new OpenAIClient(
                new OpenAIAuthentication(
                    AiChatbotService.Utils.Config.openAI.OPENAI_API_KEY,
                    AiChatbotService.Utils.Config.openAI.OPENAI_ORGANIZATION_ID
                )
            )
        )
        {
            var messages = new List<Message>();
            var messagelogs = new List<ChatLogs>();
            AIChatHistory? chathistory = null;

            // process existing messages if there's any
            chathistory = dbcontext
                .AIChatHistory.Where(x => x.itemkey == request.parentMessageId)
                .SingleOrDefault();

            // if not exist, we just create a new instance
            if (chathistory == null)
            {
                messages.Add(new Message(Role.User, request.message));
            }
            else
            {
                // deserialize the messages
                var usermessages = JsonSerializer.Deserialize<List<Message>>(chathistory.messages);

                if (usermessages != null)
                {
                    messages.AddRange(usermessages);
                    messages.Add(new Message(Role.User, request.message));
                }
                else
                {
                    messages.Add(new Message(Role.User, request.message));
                }

                var logs = JsonSerializer.Deserialize<List<ChatLogs>>(chathistory.logs);
                if (logs != null)
                {
                    messagelogs.AddRange(logs);
                }
            }

            var defaultModel = Model.GPT3_5_Turbo;
            if (AiChatbotService.Utils.Config.openAI.DEFAULT_MODEL == "gpt3.5") {
                defaultModel = Model.GPT3_5_Turbo;
            } else if (AiChatbotService.Utils.Config.openAI.DEFAULT_MODEL == "gpt4omini") {
                defaultModel = Model.GPT4oMini;
            }
            

            var chatRequest = new ChatRequest(messages, defaultModel);
            var response = await api.ChatEndpoint.GetCompletionAsync(chatRequest);
            var choice = response.FirstChoice;

            {
                messages.Add(new Message(Role.Assistant, "Please remove the chat styling like bold ex. **, just sent the message in pure text"));
                messages.Add(new Message(Role.Assistant, choice.Message));
            }

            {
                // add user logs
                messagelogs.Add(
                    new ChatLogs()
                    {
                        username = request.userInfo.userName,
                        createdateutc = DateTime.UtcNow,
                        message = request.message
                    }
                );

                // add bot logs
                messagelogs.Add(
                    new ChatLogs()
                    {
                        username = "GPT-3 bot",
                        createdateutc = DateTime.UtcNow,
                        message = choice.Message
                    }
                );
            }

            if (chathistory == null)
            {
                var parentMessageId = !string.IsNullOrEmpty(request.parentMessageId)? request.parentMessageId : request.messageId ;
                var newmessage = new AIChatHistory()
                {
                    recid = Guid.NewGuid(),
                    itemkey = parentMessageId,
                    username = request.userInfo.userId,
                    channel = request.channel,
                    createdateutc = DateTime.UtcNow,
                    lastupdateutc = DateTime.UtcNow,
                    messages = JsonSerializer.Serialize(messages),
                    logs = JsonSerializer.Serialize(messagelogs)
                };

                dbcontext.AIChatHistory.Add(newmessage);
            }
            else
            {
                chathistory.lastupdateutc = DateTime.UtcNow;
                chathistory.messages = JsonSerializer.Serialize(messages);
                chathistory.logs = JsonSerializer.Serialize(messagelogs);
            }

            dbcontext.SaveChanges();

            apiresponse.responseType = UtilsLib.ApiResponseType.reply;
            apiresponse.result = choice.Message;
            apiresponse.success = true;
        }

        return Results.Ok(apiresponse);
    }
}
