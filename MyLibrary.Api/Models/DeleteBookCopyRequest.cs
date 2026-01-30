public class DeleteCopiesRequest
{
    public string Mode { get; set; }   // "list" | "range"
    public string Value { get; set; }  // "400,402" | "400-405"
    public string DemirbasPrefix { get; set; }
}
