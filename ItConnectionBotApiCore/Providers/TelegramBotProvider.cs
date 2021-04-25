using ItConnectionBotApiCore.Models.DbModels;
using Telegram.Bot.Types.ReplyMarkups;
using ItConnectionBotApiCore.Enums;
using System.Collections.Generic;
using Telegram.Bot.Args;
using Telegram.Bot;
using System.Linq;
using System.Text;
using System;

namespace ItConnectionBotApiCore.Providers
{
    public class TelegramBotProvider
    {

        private static readonly string _token = "1764830010:AAEM7NLKrPYd7adYiU78K15tSynViyoE_9A";
        private static TelegramBotClient client;
        private static EntityUserProvider userProvider;
        private static EntityItTaskProvider taskProvider;
        private static EntityInDesireOfferInfoProvider inDesireOfferInfoProvider;

        public TelegramBotProvider()
        {
            userProvider = new EntityUserProvider();
            taskProvider = new EntityItTaskProvider();
            inDesireOfferInfoProvider = new EntityInDesireOfferInfoProvider();
            client = new TelegramBotClient(_token);
            client.StartReceiving();
            client.OnMessage += OnMessageHandler;
            client.OnCallbackQuery += OnCallBackQuery;
        }

        private static async void OnCallBackQuery(object sender, CallbackQueryEventArgs e)
        {
            try
            {

                var clientId = e.CallbackQuery.Message.Chat.Id;

                switch (e.CallbackQuery.Data)
                {
                    case "ShowAllMyActiveTasks":
                        var myOffers = inDesireOfferInfoProvider.GetMyOffers(clientId.ToString());
                        if (myOffers == null || !myOffers.Any())
                        {
                            await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                            await client.SendTextMessageAsync(clientId, $"You dont have any created tasks\nGet task with command:\n/gettask task_id", replyMarkup: GetWorkersDefaultButtons());
                            break;
                        }

                        var myOffersIds = myOffers.Select(x => x.OfferId).Distinct();
                        var myOffersCreatedIdsBuilder = new StringBuilder();
                        foreach (var id in myOffersIds)
                        {
                            var createdTasks = taskProvider.GetAllMyOffers(id.ToString());
                            foreach (var task in createdTasks)
                            {
                                myOffersCreatedIdsBuilder.AppendLine($"Task Id:{task.Id}; created:{task.DateOfCreating.ToString("hh:mm dd.MM.yyyy")} - {task.TaskTitle}");
                            }
                        }
                        await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                        await client.SendTextMessageAsync(clientId, $"Task list:\n\n{myOffersCreatedIdsBuilder.ToString()}", replyMarkup: GetWorkersDefaultButtons());
                        break;
                    case "CreateNewTask":
                        var thUser = userProvider.GetUserByUserId(clientId.ToString());
                        thUser.WaitingForTitleCreatingOffer = true;
                        userProvider.UpdateUser(thUser);
                        await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                        await client.SendTextMessageAsync(clientId, $"Enter offer title name", replyMarkup: AddOfferWithTitleButtons());
                        break;
                    case "ShowAllMyCreatedTasks":
                        var myCreatedTasks = taskProvider.GetAllMyCreatedTasks(clientId.ToString());
                        if (myCreatedTasks != null && myCreatedTasks.Any())
                        {
                            var stringBuilder = new StringBuilder();
                            foreach (var task in myCreatedTasks)
                            {
                                stringBuilder.AppendLine($"Task Id:{task.Id}; created:{task.DateOfCreating.ToString("hh:mm dd.MM.yyyy")} - {task.TaskTitle}");
                            }
                            await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                            await client.SendTextMessageAsync(clientId, $"Task list:\n\n{stringBuilder.ToString()}", replyMarkup: GetSellersDefaultButtons());
                        }
                        else
                        {
                            await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                            await client.SendTextMessageAsync(clientId, $"You dont have any created tasks", replyMarkup: GetSellersDefaultButtons());
                        }
                        break;
                    case "ShowAllTask":
                        var newTasks = taskProvider.GetAllNewTasks();
                        var searchUser = userProvider.GetUserByUserId(clientId.ToString());
                        if (newTasks != null && newTasks.Any())
                        {
                            var stringBuilder = new StringBuilder();
                            if (searchUser.RoleEnum == RoleEnum.Worker)
                            {
                                stringBuilder.AppendLine("Get task with command:\n/gettask task_id");
                            }
                            foreach (var task in newTasks)
                            {
                                stringBuilder.AppendLine($"Task Id:{task.Id}; created:{task.DateOfCreating.ToString("hh:mm dd.MM.yyyy")} - {task.TaskTitle};");
                            }
                            var defaultButtons = searchUser.RoleEnum == RoleEnum.Worker ? GetWorkersDefaultButtons() : GetSellersDefaultButtons();
                            await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                            await client.SendTextMessageAsync(clientId, $"Task list:\n\n{stringBuilder.ToString()}", replyMarkup: defaultButtons);
                        }
                        else
                        {
                            var defaultButtons = searchUser.RoleEnum == RoleEnum.Worker ? GetWorkersDefaultButtons() : GetSellersDefaultButtons();
                            await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                            await client.SendTextMessageAsync(clientId, $"Task list is empty", replyMarkup: defaultButtons);
                        }
                        break;
                    case "Cancel":
                    case "Authorize":
                        var user = userProvider.GetUserByUserId(clientId.ToString());
                        if (user != null)
                        {
                            var sellersButtons = user.RoleEnum == RoleEnum.Worker ? GetWorkersDefaultButtons() : GetSellersDefaultButtons();
                            var roleString = user.RoleEnum == RoleEnum.Worker ? "Worker" : "Seller";
                            await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                            await client.SendTextMessageAsync(clientId, $"You have already registered as {roleString}", replyMarkup: sellersButtons);
                            break;
                        }
                        await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                        await client.SendTextMessageAsync(clientId, $"You are not registered yet, please, choose your role", replyMarkup: GetStartNotAuthorizedRegisterButtons());
                        break;
                    case "Seller":
                        var sellerResult = userProvider.AddUserIfNoExist(new User()
                        {
                            UserId = clientId.ToString(),
                            RoleEnum = RoleEnum.Seller
                        });
                        switch (sellerResult.Item1)
                        {
                            case ProviderResultEnum.Success:
                                await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                                await client.SendTextMessageAsync(clientId, "Success! Now You are tasks seller!", replyMarkup: GetSellersDefaultButtons());
                                break;
                            case ProviderResultEnum.AlreadyExists:
                                var sellersButtons = sellerResult.Item2.RoleEnum == RoleEnum.Worker ? GetWorkersDefaultButtons() : GetSellersDefaultButtons();
                                var roleString = sellerResult.Item2.RoleEnum == RoleEnum.Worker ? "Worker" : "Seller";
                                await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                                await client.SendTextMessageAsync(clientId, $"You have already registered as {roleString}", replyMarkup: sellersButtons);
                                break;
                            case ProviderResultEnum.Error:
                                await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                                await client.SendTextMessageAsync(clientId, "Server exception\nPlease, write to administrator or go to start dialof with command \"\\start\"");
                                break;
                        }
                        break;
                    case "Worker":
                        var workerResult = userProvider.AddUserIfNoExist(new User()
                        {
                            UserId = clientId.ToString(),
                            RoleEnum = RoleEnum.Worker
                        });
                        switch (workerResult.Item1)
                        {
                            case ProviderResultEnum.Success:
                                await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                                await client.SendTextMessageAsync(clientId, "Success! Now You are worker!\nSearch for tasks", replyMarkup: GetWorkersDefaultButtons());
                                break;
                            case ProviderResultEnum.AlreadyExists:
                                var workersButtons = workerResult.Item2.RoleEnum == RoleEnum.Worker ? GetWorkersDefaultButtons() : GetSellersDefaultButtons();
                                var roleString = workerResult.Item2.RoleEnum == RoleEnum.Worker ? "Worker" : "Seller";
                                await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                                await client.SendTextMessageAsync(clientId, $"You have already registered as {roleString}", replyMarkup: workersButtons);
                                break;
                            case ProviderResultEnum.Error:
                                await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                                await client.SendTextMessageAsync(clientId, "Server exception", replyMarkup: GetWorkersDefaultButtons());
                                break;
                        }
                        break;
                    case "Moderator":
                        await client.EditMessageReplyMarkupAsync(clientId, e.CallbackQuery.Message.MessageId);
                        await client.SendTextMessageAsync(e.CallbackQuery.Message.Chat.Id, "If you want to become administrator, please, write to our administrator!");
                        break;
                }

            }
            catch (Exception ex)
            {

            }
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {

            try
            {

                var message = e.Message;
                var clientId = message.Chat.Id;

                //TODO Dialog
                //var lastMessage = await client.ForwardMessageAsync(clientId.ToString(), clientId.ToString(), Convert.ToInt32(e.Message.MessageId) - 1);
                //var lastMessageText = lastMessage?.Text;

                var user = userProvider.GetUserByUserId(clientId.ToString());
                if (user != null)
                {
                    if (!String.IsNullOrEmpty(message?.Text) && user.WaitingForTitleCreatingOffer)
                    {
                        user.WaitingForTitleCreatingOffer = false;
                        var offer = new Offer() { CreatedUserId = clientId.ToString(), DateOfCreating = DateTime.Now, ItTaskStatus = ItTaskStatusEnum.New, TaskTitle = message.Text };
                        taskProvider.AddTask(offer);
                        await client.SendTextMessageAsync(clientId, $"Success! Your offer was added.\nWait for new workers!", replyMarkup: GetSellersDefaultButtons());
                        return;
                    }
                    else
                    {
                        if (user.WaitingForTitleCreatingOffer)
                        {
                            await client.SendTextMessageAsync(clientId, $"Title of your offer is empty. Please, enter full title...", replyMarkup: AddOfferWithTitleButtons());
                            return;
                        }
                    }
                }
                if (!String.IsNullOrEmpty(message?.Text))
                {
                    if (message.Text.StartsWith("/gettask"))
                    {
                        if (!(message.Text.Split(" ").Count() != 2))
                        {
                            var id = message.Text.Split(" ")[1];
                            var worker = userProvider.GetUserByUserId(clientId.ToString());
                            if (worker.RoleEnum == RoleEnum.Worker)
                            {
                                try
                                {
                                    var intId = Convert.ToInt32(id);
                                    var added = inDesireOfferInfoProvider.MakeDesireOffer(Convert.ToInt32(id), worker.UserId);
                                    if (added)
                                    {
                                        var offer = inDesireOfferInfoProvider.GetOfferById(intId);
                                        await client.SendTextMessageAsync(clientId, $"Success! Now this offer is yours!\n Wait for seller to communicate with you...", replyMarkup: GetWorkersDefaultButtons());
                                        await client.SendTextMessageAsync(offer.CreatedUserId, $"User @{message.From.Username} want to connect with your task:\nId:{offer.Id} ; Title: {offer.TaskTitle}");
                                        return;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(clientId, $"This offer does not exists!", replyMarkup: GetWorkersDefaultButtons());
                                        return;
                                    }
                                }
                                catch { }
                            }
                        }
                    }

                    switch (message.Text)
                    {
                        case "/registration":
                        case "/start":
                            {
                                await client.SendTextMessageAsync(message.Chat.Id, "Hello!\nLets start your IT Connection!\nWe would like to add you in our database\n\nWho are you are?", replyMarkup: GetStartRegisterButtons());
                                break;
                            }
                        default:
                            {
                                await client.SendTextMessageAsync(message.Chat.Id, "I dont know this command :(\n\nPlease, enter  /start  to speak with me...");
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static InlineKeyboardMarkup GetWorkersDefaultButtons()
        {
            var buttonsList = new List<List<InlineKeyboardButton>>() {
                new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton() { Text = "Show all tasks", CallbackData = "ShowAllTask" }
            },
            new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton() { Text = "Show all my tasks", CallbackData = "ShowAllMyActiveTasks" }
            }};
            return new InlineKeyboardMarkup(buttonsList);
        }

        private static InlineKeyboardMarkup GetSellersDefaultButtons()
        {
            var buttonsList = new List<List<InlineKeyboardButton>>() {
                new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton() { Text = "Show all tasks", CallbackData = "ShowAllTask" },
            },
            new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton() { Text = "Show all my created tasks", CallbackData = "ShowAllMyCreatedTasks" }
            },
            new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton() { Text = "Create new task", CallbackData = "CreateNewTask" }
            }};
            return new InlineKeyboardMarkup(buttonsList);
        }

        private static InlineKeyboardMarkup AddOfferWithTitleButtons()
        {
            var buttonsList = new List<List<InlineKeyboardButton>>() {
            new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton() { Text = "Cancel", CallbackData = "Cancel" }
            }};
            return new InlineKeyboardMarkup(buttonsList);
        }

        private static InlineKeyboardMarkup GetStartRegisterButtons()
        {
            var buttonsList = new List<List<InlineKeyboardButton>>() { new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton() { Text = "Seller", CallbackData = "Seller" },
                new InlineKeyboardButton() { Text = "Worker", CallbackData = "Worker" },
                new InlineKeyboardButton() { Text = "Moderator", CallbackData = "Moderator" }
            }, new List<InlineKeyboardButton>(){
                new InlineKeyboardButton() { Text = "Authorize", CallbackData = "Authorize" }
            } };
            return new InlineKeyboardMarkup(buttonsList);
        }

        private static InlineKeyboardMarkup GetStartNotAuthorizedRegisterButtons()
        {
            var buttonsList = new List<List<InlineKeyboardButton>>() { new List<InlineKeyboardButton>()
            {
                new InlineKeyboardButton() { Text = "Seller", CallbackData = "Seller" },
                new InlineKeyboardButton() { Text = "Worker", CallbackData = "Worker" },
                new InlineKeyboardButton() { Text = "Moderator", CallbackData = "Moderator" }
            }};
            return new InlineKeyboardMarkup(buttonsList);
        }

    }
}
