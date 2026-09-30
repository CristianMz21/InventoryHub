namespace InventoryHub.Contracts;

public static class Limits
{
    public const int NameMaxLength = 100;
    public const int SearchMaxLength = 100;
    public const int CategoryIdMin = 1;
    public const int QuantityMin = 0;
    public const int PageMin = 1;
    public const int PageSizeMin = 1;
    public const int PageSizeMax = 100;
    public const int PageSizeDefault = 20;
}
