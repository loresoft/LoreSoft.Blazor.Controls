using System.Text.Json.Serialization;

namespace Sample.Core.Models;

public class Bank
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("uid")]
    public Guid Identifier { get; set; }

    [JsonPropertyName("account_number")]
    public string AccountNumber { get; set; }

    [JsonPropertyName("iban")]
    public string IBAN { get; set; }

    [JsonPropertyName("bank_name")]
    public string BankName { get; set; }

    [JsonPropertyName("routing_number")]
    public string RoutingNumber { get; set; }

    [JsonPropertyName("swift_bic")]
    public string SwiftBIC { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
}
