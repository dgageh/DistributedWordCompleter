namespace IngestorService.Services
{
    public class PrefixTreeClient : IPrefixTreeClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PrefixTreeClient(HttpClient httpClient, string baseUrl)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<bool> InsertWordAsync(string word)
        {
            var response = await _httpClient.PostAsync($"{_baseUrl}/api/words/insert?word={word}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveWordAsync(string word)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/words/remove?word={word}");
            return response.IsSuccessStatusCode;
        }

        //public async Task<IEnumerable<string>> SearchWordsAsync(string word)
        //{
        //    var response = await _httpClient.GetAsync($"{_baseUrl}/api/words/search?word={word}");
        //    return response.IsSuccessStatusCode
        //        ? await response.Content.ReadFromJsonAsync<IEnumerable<string>>()
        //        : Enumerable.Empty<string>();
        //}

        //public async Task<IEnumerable<string>> GetWordsWithPrefixAsync(string prefix)
        //{
        //    var response = await _httpClient.GetAsync($"{_baseUrl}/api/words/autocomplete?prefix={prefix}");
        //    return response.IsSuccessStatusCode
        //        ? await response.Content.ReadFromJsonAsync<IEnumerable<string>>()
        //        : Enumerable.Empty<string>();
        //}
    }
}
