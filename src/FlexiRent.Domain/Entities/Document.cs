namespace FlexiRent.Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public Guid FolderId { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long Size { get; set; }
}
