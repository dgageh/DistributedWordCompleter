namespace IngestorService.Services
{
    public class PrefixTreeClient : IPrefixTreeClient
    {
        private readonly HttpClient _httpClient;

        public PrefixTreeClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<bool> InsertWordAsync(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                throw new ArgumentException("Word cannot be null or empty.", nameof(word));
            }

            var content = new StringContent($"\"{word}\"", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/api/words/insert", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveWordAsync(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                throw new ArgumentException("Word cannot be null or empty.", nameof(word));
            }

            var content = new StringContent($"\"{word}\"", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/api/words/remove", content);
            return response.IsSuccessStatusCode;
        }
    }
}
