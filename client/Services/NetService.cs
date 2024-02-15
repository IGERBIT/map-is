using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using client.Utils;
using MapShared.Dto;
using Newtonsoft.Json;

namespace client;

public class NetService
{
    private readonly HttpClient _httpClient = new HttpClient();

    private const string BaseApi = "http://localhost:5184/";

    public NetService()
    {
        _httpClient.BaseAddress = new Uri(BaseApi);
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<Result<HttpResponseMessage, Exception>> Post(string path, object value)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(value), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(path, content);
            
            if (!response.IsSuccessStatusCode)
            {
                return new HttpException(response.ReasonPhrase, response.StatusCode);
            }
            
            return response;
        }
        catch (HttpRequestException e)
        {
            return e;
        }
    }
    
    public async Task<Result<HttpResponseMessage, Exception>> Get(string path)
    {
        try
        {
            var response = await _httpClient.GetAsync(path);

            if (!response.IsSuccessStatusCode)
            {
                return new HttpException(response.ReasonPhrase, response.StatusCode);
            }
            
            return response;
        }
        catch (HttpRequestException e)
        {
            return e;
        }
    }

    public static async Task<Result<T, Exception>> JsonTo<T>(Result<HttpResponseMessage, Exception> result)
    {
        if (result.IsFail) return result.Error!;

        using var value = result.Value!;

        try
        {
            var serverResponse = await value.Content.ReadFromJsonAsync<Result<T, ApiError>>();

            if (serverResponse.IsFail)
            {
                var error = serverResponse.Error;
                return new ApiException(error.Code, error.Message);
            }

            return serverResponse.Value!;
        }
        catch (Exception e)
        {
            return e;
        }
    }
    
    public static async Task<Result<T, Exception>> JsonTo<T>(Task<Result<HttpResponseMessage, Exception>> resultTask)
    {
        try
        {
            return await JsonTo<T>(await resultTask);
        }
        catch (Exception e)
        {
            return e;
        } 
    }

    public async Task<Result<string, Exception>> SingIn(SignInDto dto)
    {
        return await JsonTo<string>(Post("auth/signin", dto));
    }
    
    public async Task<Result<string, Exception>> CreateOrg(CreateOrganizationDto dto)
    {
        return await JsonTo<string>(Post("auth/signin", dto));
    }
    
    public async Task<Result<string, Exception>> WhoIAm()
    {
        try
        {
            var response = await Get("auth/whoiam");
            if (response.IsFail) return response.Error;

            return await response.Value!.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            return e;
        }
    }
    
    
}

