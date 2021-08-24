namespace AniListClient.Responses
{
    internal interface IResponse<T>
    {
        T ConvertToModel();
    }
}
