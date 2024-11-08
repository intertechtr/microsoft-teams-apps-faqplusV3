using System.Collections.Generic;

public record Request
{
    public bool Stream { get; set; }
    public List<Message> Messages { get; set; } = new();
}
