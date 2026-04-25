using System.Text.Json;
using System.Text.Json.Serialization;

string json = "[\n  {\n    \"batchId\": 789,\n    \"debtId\": 1841,\n    \"dateAdded\": \"2021-07-27T16:01:39.41\",\n    \"debtCategoryId\": 2,\n    \"agreementNumber\": 78262155,\n    \"clientNumber\": 1068055,\n    \"clientName\": \"Client Two\"\n  },\n  {\n    \"batchId\": 866,\n    \"debtId\": 1918,\n    \"dateAdded\": \"2021-08-25T14:47:18.13\",\n    \"debtCategoryId\": 2,\n    \"agreementNumber\": 1000140792,\n    \"clientNumber\": 11213287,\n    \"clientName\": \"Client One\"\n  }\n]";
var data = JsonSerializer.Deserialize<ICollection<DebtConfirmation>>(json);
foreach (DebtConfirmation current in data)
{
    Console.WriteLine(current.DateAdded);
}

public partial class DebtConfirmation 
{
    [JsonPropertyName("batchId")]
    public long BatchId { get; set; }

    [JsonPropertyName("debtId")]
    public long DebtId { get; set; }

    [JsonPropertyName("dateAdded")]
    public DateTimeOffset DateAdded { get; set; }

    [JsonPropertyName("debtCategoryId")]
    public long DebtCategoryId { get; set; }

    [JsonPropertyName("agreementNumber")]
    public long AgreementNumber { get; set; }

    [JsonPropertyName("clientNumber")]
    public long ClientNumber { get; set; }

    [JsonPropertyName("clientName")]
    public string ClientName { get; set; }
}