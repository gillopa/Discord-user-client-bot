using System.Runtime.InteropServices;

class Program
{
    static async Task Main()
    {
        var client = new DiscordClient();
        HashSet<string> files = new();
        bool autoSendIfNewFile = false;
        _ = Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                var items = Directory.GetFiles("./photos");
                foreach (var item in items)
                {
                    if (files.Add(item) && autoSendIfNewFile)
                    {
                        foreach (var chat in client.chatIds)
                        {
                            try
                            {
                                await client.SendImageToChannel(chat, item);
                            }
                            catch (Exception e)
                            {
                                System.Console.WriteLine(e);
                            }
                        }
                    }
                }
            }
        });

        while (true)
        {
            Console.Clear();
            Console.WriteLine("DISCORD CLIENT");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Add chat ID");
            Console.WriteLine("3. Send message to one chat");
            Console.WriteLine("4. Send message to all chats");
            Console.WriteLine("5. Send image to one chat");
            Console.WriteLine("6. Send image to all chats");
            Console.WriteLine("7. List all chat IDs");
            Console.WriteLine("8. Toggle autosending image when file is new");
            Console.WriteLine("9. Exit");
            Console.Write("Choose option: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    string token;
                    if (!File.Exists("token.txt"))
                    {
                        Console.Write("Enter login: ");
                        var login = Console.ReadLine();
                        Console.Write("Enter password: ");
                        var password = Console.ReadLine();
                        token = await client.Login(login, password);
                        File.WriteAllText("token.txt", token);
                    }
                    else
                        token = File.ReadAllText("token.txt");

                    if (!string.IsNullOrEmpty(token))
                    {
                        client.ClientAutorization(token);
                        Console.WriteLine("Login successful!");
                    }
                    else
                    {
                        Console.WriteLine("Login failed!");
                    }
                    break;

                case "2":
                    Console.Write("Enter chat ID: ");
                    var id = Console.ReadLine();
                    if (client.AddId(id))
                        Console.WriteLine("Chat ID added!");
                    else
                        Console.WriteLine("Chat ID already exists!");
                    break;

                case "3":
                    if (!client.isClientAutorize)
                    {
                        Console.WriteLine("Please login first!");
                        break;
                    }
                    Console.Write("Enter chat ID: ");
                    var chatId = Console.ReadLine();
                    Console.Write("Enter message: ");
                    var message = Console.ReadLine();
                    await client.SendMessage(message, chatId);
                    Console.WriteLine("Message sent!");
                    break;

                case "4":
                    if (!client.isClientAutorize)
                    {
                        Console.WriteLine("Please login first!");
                        break;
                    }
                    Console.Write("Enter message to send to all chats: ");
                    var broadcastMessage = Console.ReadLine();
                    foreach (var chat in client.chatIds)
                    {
                        try
                        {
                            await client.SendMessage(broadcastMessage, chat);
                            Console.WriteLine($"Sent to {chat}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send to {chat}: {ex.Message}");
                        }
                        await Task.Delay(1000);
                    }
                    Console.WriteLine("Broadcast completed!");
                    break;

                case "5":
                    if (!client.isClientAutorize)
                    {
                        Console.WriteLine("Please login first!");
                        break;
                    }
                    Console.Write("Enter chat ID: ");
                    var imageChatId = Console.ReadLine();
                    Console.Write("Enter image path: ");
                    var imagePath = Console.ReadLine();
                    try
                    {
                        await client.SendImageToChannel(imageChatId, imagePath);
                        Console.WriteLine("Image sent successfully!");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send image: {ex.Message}");
                    }
                    break;

                case "6":
                    if (!client.isClientAutorize)
                    {
                        Console.WriteLine("Please login first!");
                        break;
                    }
                    Console.Write("Enter image path to send to all chats: ");
                    var broadcastImagePath = Console.ReadLine();
                    foreach (var chat in client.chatIds)
                    {
                        try
                        {
                            await client.SendImageToChannel(chat, broadcastImagePath);
                            Console.WriteLine($"Image sent to {chat}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send image to {chat}: {ex.Message}");
                        }
                        await Task.Delay(1500);
                    }
                    Console.WriteLine("Image broadcast completed!");
                    break;

                case "7":
                    Console.WriteLine("Saved chat IDs:");
                    foreach (var chat in client.chatIds)
                    {
                        Console.WriteLine(chat);
                    }
                    break;

                case "8":
                    autoSendIfNewFile = !autoSendIfNewFile;
                    System.Console.WriteLine($"Auto send now is: {autoSendIfNewFile}");
                    break;

                case "9":
                    return;

                default:
                    Console.WriteLine("Invalid option!");
                    break;
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}