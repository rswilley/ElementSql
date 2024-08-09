internal class CachedTable
{
    public string TableName { get; set; } = string.Empty;
    public IEnumerable<CachedColumn> Columns { get; set; } = Enumerable.Empty<CachedColumn>();
    public string? KeyColumn { get; set; }
    public string ConcatenatedColumns { get; set; } = string.Empty;
    public string InsertStatement { get; set; } = string.Empty;
    public string UpdateStatement { get; set; } = string.Empty;
}
