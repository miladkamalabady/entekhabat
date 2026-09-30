namespace EntekhabatApi.Services;

public static class PersianText
{
    /// <summary>
    /// Normalizes common Arabic/Persian character variants for searching.
    /// </summary>
    public static string NormalizeForSearch(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Trim()
            .Replace('ي', 'ی')
            .Replace('ى', 'ی')
            .Replace('ك', 'ک');
    }
}
