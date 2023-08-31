
using Nest;

var settings = new ConnectionSettings(new Uri("https://localhost:9200"))
           .DefaultIndex("index")
           .BasicAuthentication("elastic", "L*QEKkMyg+AV7CPe0Drj")
           .CertificateFingerprint("0763eb6ca7a527b19db69ddf73f5e1de2d7c42847270d9cf25f2cd6f5d8370e8");

var NestClient = new ElasticClient(settings);




var searchResponse = await NestClient.SearchAsync<MemoryItem>(s => s
    .Query(q => q
        .Term(t => t
            .Field(f => f.Id)
            .Value(15001432)
        )
    )
);


// 检查是否有匹配的结果
if (searchResponse.IsValid && searchResponse.Documents.Any())
{
    // 获取第一个匹配的文档
    var insertedDocument = searchResponse.Documents.First();
    // 输出文档的内容
    System.Console.WriteLine(insertedDocument.Id);
    System.Console.WriteLine(insertedDocument.MemoryLibId);
    System.Console.WriteLine(insertedDocument.SrcContent);
    System.Console.WriteLine(insertedDocument.SrcContentMD5);
    System.Console.WriteLine(insertedDocument.TgtContent);

}

public class MemoryItem
{
    public Int64 Id { get; set; }

    public Int64 MemoryLibId { get; set; }

    public string SrcContent { get; set; }

    public string SrcContentMD5 { get; set; }

    public string TgtContent { get; set; }

    public string TgtContentMD5 { get; set; }

    public Int64 State { get; set; }

    public Int32 Matching { get; set; }

    public Int32 SrcLanguage { get; set; }

    public Int32 TgtLanguage { get; set; }

    public Int32 Seq { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime UpdateTime { get; set; }

    public bool Status { get; set; }

    public bool EditStatus { get; set; }

    public Int64? MemoryBatchId { get; set; }

}