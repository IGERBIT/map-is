using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using client.Utils;
using MapShared.Dto;
using Newtonsoft.Json;

namespace client.Services;

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
    
    public async Task<Result<HttpResponseMessage, Exception>> PostJson([StringSyntax("Uri")] string? requestUri, object value)
    {
        try
        {
            var content = new StringContent(JsonConvert.SerializeObject(value, Formatting.None), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(requestUri, content);
            return response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            return e;
        }
    }
    
    public Task<Stream> GetStream([StringSyntax("Uri")] string? requestUri)
    {
        return _httpClient.GetStreamAsync(requestUri);
    }
    
    public async Task<Result<HttpResponseMessage, Exception>> Get([StringSyntax("Uri")] string? requestUri)
    {
        try
        {
            var response = await _httpClient.GetAsync(requestUri);
            return response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            return e;
        }
    }
    
    public static async Task<Result<T, Exception>> ReadPureJson<T>(Result<HttpResponseMessage, Exception> result)
    {
        try
        {
            if (result.IsFail) throw result.Error;

            using var value = result.Value;

            return await value.Content.ReadFromJsonAsync<T>() ?? throw new InvalidOperationException();
        }
        catch (Exception e)
        {
            return e;
        }
    }
    
    public static async Task<Result<T, Exception>> ReadPureJson<T>(Task<Result<HttpResponseMessage, Exception>> resultTask)
    {
        try
        {
            return await ReadPureJson<T>(await resultTask);
        }
        catch (Exception e)
        {
            return e;
        }
    }
    
    public static async Task<Result<T, Exception>> ReadResultJson<T>(Result<HttpResponseMessage, Exception> result)
    {
        try
        {
            var serverResponse = await ReadPureJson<Result<T, ApiError>>(result);
            if (serverResponse.IsFail) return serverResponse.Error;

            var resultValue = serverResponse.Value;
            if (resultValue.IsFail)  return new ApiException(resultValue.Error.Code, resultValue.Error.Message ?? string.Empty);
            
            return resultValue.Value;
        }
        catch (Exception e)
        {
            return e;
        }
    }
    
    public static async Task<Result<T, Exception>> ReadResultJson<T>(Task<Result<HttpResponseMessage, Exception>> resultTask)
    {
        try
        {
            return await ReadResultJson<T>(await resultTask);
        }
        catch (Exception e)
        {
            return e;
        } 
    }

    public async Task<Result<TokenDto, Exception>> SingIn(SignInDto dto)
    {
        return await ReadResultJson<TokenDto>(PostJson("auth/signin", dto));
    }
    
    public async Task<Result<string, Exception>> CreateOrg(CreateOrganizationDto dto)
    {
        return await ReadResultJson<string>(PostJson("org/create", dto));
    }
    
    public async Task<Result<string, Exception>> CreateSchema(string schemaName, Stream imageStream)
    {
        try
        {
            using var multipartFormContent = new MultipartFormDataContent();
            var streamContent = new StreamContent(imageStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            multipartFormContent.Add(streamContent, "file", "map.png");
            multipartFormContent.Add(new StringContent(schemaName), "schemeName");
            using var response = await _httpClient.PostAsync("schemas/create", multipartFormContent);
            response.EnsureSuccessStatusCode();

            return "Ok";
        }
        catch (HttpRequestException e)
        {
            return e;
        }
    }
    
    public Task<Stream> GetImage(string remotePath)
    {
        return GetStream("static/" + remotePath);
    }
    
    public async Task<Result<SchemaLiteDto[], Exception>> GetSchemes()
    {
        return await ReadPureJson<SchemaLiteDto[]>(Get("schemas/list"));
    }
    
    public async Task<Result<SchemaDto, Exception>> GetScheme(int id)
    {
        return await ReadPureJson<SchemaDto>(Get($"schemas/get/{id}"));
    }
    
    public async Task<Result<bool, Exception>> DeleteScheme(int id)
    {
        return (await Get($"schemas/delete/{id}")).IsSuccess;
    }
    
    public async Task<Result<bool, Exception>> SaveScheme(SchemaDto schemaDto)
    {
        return (await PostJson("schemas/save", schemaDto)).IsSuccess;
    }
    
    public async Task<Result<bool, Exception>> AddMember(CreateMemberDto memberDto)
    {
        return (await PostJson("org/add-member", memberDto)).IsSuccess;
    }
    
    public async Task<Result<bool, Exception>> UpdateMember(Guid id, CreateMemberDto memberDto)
    {
        return (await PostJson($"org/update-member/{id}", memberDto)).IsSuccess;
    }
    
    public async Task<Result<bool, Exception>> RemoveMember(Guid id)
    {
        return (await PostJson($"org/remove-member/{id}", new {})).IsSuccess;
    }
    
    public async Task<Result<MemberDto[], Exception>> Members()
    {
        return await ReadPureJson<MemberDto[]>(Get($"org/members"));
    }
    
    
}

