using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

class DiscordClient
{
    private HttpClient _httpClient;
    public bool isClientAutorize { get; private set; }
    public HashSet<string> chatIds { get; private set; }
    public DiscordClient()
    {
        isClientAutorize = false;
        chatIds = new();
        _httpClient = new();
        _httpClient.BaseAddress = new Uri("https://discord.com");
    }


    public bool AddId(string id) => chatIds.Add(id);
    public async Task<string> Login(string login, string password)
    {
        try
        {
            var authData = new JsonObject
            {
                ["login"] = login,
                ["password"] = password,
                ["undelete"] = false,
                ["login_source"] = null,
                ["gift_code_sku_id"] = null
            };
            var response = await _httpClient.PostAsync("api/v9/auth/login", JsonContent.Create(authData));
            response.EnsureSuccessStatusCode();
            var responseJson = JsonSerializer.Deserialize<JsonObject>(await response.Content.ReadAsStringAsync());
            return responseJson["token"].ToString();
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
        }
        return string.Empty;
    }
    public void ClientAutorization(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
        isClientAutorize = true;
    }

    public async Task SendMessage(string message, string chatId)
    {
        try
        {
            string nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            ulong nonceL = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() << 22;
            var json = new JsonObject
            {
                ["mobile_network_type"] = "unknown",
                ["content"] = message,
                ["nonce"] = nonce,
                ["tts"] = false,
                ["flags"] = 0
            };

            var r = await _httpClient.PostAsync($"api/v9/channels/{chatId}/messages", JsonContent.Create(json));
            r.EnsureSuccessStatusCode();

        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
        }
    }
    public async Task SendImageToChannel(string channelId, string imagePath)
    {
        try
        {
            var attachmentResponse = await _httpClient.PostAsJsonAsync(
                $"https://discord.com/api/v9/channels/{channelId}/attachments",
                new
                {
                    files = new[]
                    {
                        new
                        {
                            filename = Path.GetFileName(imagePath),
                            file_size = new FileInfo(imagePath).Length,
                            id = "1",
                            is_clip = false
                        }
                    }
                }
            );

            var attachmentJson = await attachmentResponse.Content.ReadFromJsonAsync<JsonDocument>();
            var uploadUrl = attachmentJson.RootElement.GetProperty("attachments")[0].GetProperty("upload_url").GetString();
            var uploadedFilename = attachmentJson.RootElement.GetProperty("attachments")[0].GetProperty("upload_filename").GetString();
            var attachmentId = attachmentJson.RootElement.GetProperty("attachments")[0].GetProperty("id").GetInt32();

            using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(imagePath));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            var uploadResponse = await _httpClient.PutAsync(uploadUrl, fileContent);
            uploadResponse.EnsureSuccessStatusCode();

            var messageResponse = await _httpClient.PostAsJsonAsync(
                $"https://discord.com/api/v9/channels/{channelId}/messages",
                new
                {
                    mobile_network_type = "unknown",
                    content = "",
                    nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
                    tts = false,
                    flags = 0,
                    attachments = new[]
                    {
                        new
                        {
                            id = attachmentId.ToString(),
                            filename = Path.GetFileName(imagePath),
                            uploaded_filename = uploadedFilename
                        }
                    }
                }
            );

            messageResponse.EnsureSuccessStatusCode();
            Console.WriteLine("Image sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending image: {ex.Message}");
        }
    }
}
