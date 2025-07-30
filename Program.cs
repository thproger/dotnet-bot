using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;


namespace programs
{
    class Program
    {
        static TelegramBotClient botClient = new TelegramBotClient("7578671037:AAGD_OiGGYA9rhp19muqBaHIV0pzbX9pV88");

        static Dictionary<long, int> userQuestionIndex = new();
        static Dictionary<long, List<string>> userAnswers = new();
        static async Task Main()
        {
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = [] // Отримувати всі типи оновлень
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMe();
            Console.WriteLine($"Бот запущено: @{me.Username}");
            Console.ReadLine();
        }

        static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            var questions = Question.Start("./test.json");
            if (update.Message is not { Text: { } messageText })
                return;

            var chatId = update.Message.Chat.Id;

            if (!userQuestionIndex.ContainsKey(chatId))
            {
                userQuestionIndex[chatId] = 0;
                userAnswers[chatId] = new List<string>();
                await bot.SendMessage(chatId, "Привіт! Починаємо тест:", cancellationToken: cancellationToken);
                await bot.SendMessage(chatId, 1 + ". "
            + questions![0].question! + "\n"
            + "a) " + questions![0].options![0] + "\n"
            + "b) " + questions![0].options![1] + "\n"
            + "c) " + questions![0].options![2] + "\n"
            + "d) " + questions![0].options![3] + "\n", cancellationToken: cancellationToken);
            }
            else
            {
                int index = userQuestionIndex[chatId];
                userAnswers[chatId].Add(messageText);

                index++;

                if (index < questions.Count)
                {
                    userQuestionIndex[chatId] = index;
                    await bot.SendMessage(chatId, 1 + index + ". "
            + questions![index].question! + "\n"
            + "a) " + questions![index].options![0] + "\n"
            + "b) " + questions![index].options![1] + "\n"
            + "c) " + questions![index].options![2] + "\n"
            + "d) " + questions![index].options![3] + "\n", cancellationToken: cancellationToken);
                }
                else
                {
                    await bot.SendMessage(chatId, "Дякую! Ось твої відповіді:", cancellationToken: cancellationToken);
                    int counter = 0;
                    for (int i = 0; i < questions.Count; i++)
                    {
                        string reply;
                        Console.WriteLine(questions[i].answer!.ToLower());
                        if (questions[i].answer! == userAnswers[chatId][i].ToLower())
                        {
                            reply = $"{i}. {userAnswers[chatId][i]} +";
                            counter++;
                        }
                        else
                        {
                            reply = $"{i}. {userAnswers[chatId][i]} -";
                        }

                        await bot.SendMessage(chatId, reply, cancellationToken: cancellationToken);
                    }
                        await bot.SendMessage(chatId, $"Ваша оцінка: {counter}/{questions.Count}", cancellationToken: cancellationToken);
                    // Очистити стан
                    userQuestionIndex.Remove(chatId);
                    userAnswers.Remove(chatId);
                }
            }
        }

        static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}