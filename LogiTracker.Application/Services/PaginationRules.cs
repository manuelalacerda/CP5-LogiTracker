namespace LogiTracker.Application.Services;

public static class PaginationRules
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static void Validate(int page, int pageSize)
    {
        if (page < 1)
            throw new ArgumentException("O parâmetro 'page' deve ser um inteiro maior ou igual a 1.");
        if (pageSize < 1 || pageSize > MaxPageSize)
            throw new ArgumentException($"O parâmetro 'pageSize' deve estar entre 1 e {MaxPageSize}.");
    }
}