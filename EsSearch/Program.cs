
using System.Globalization;
using Nest;
using Index = Nest.Index;

var settings = new ConnectionSettings(new Uri("http://test1.k8s.yx.com:39200"))
           .DefaultIndex("index")
   .BasicAuthentication("", "")
   .CertificateFingerprint("");

var NestClient = new ElasticClient(settings);

// var searchResponse = await NestClient.CountAsync<MemoryItem>(s => s
//     .Query(q => q.MatchAll())
// );

// if (searchResponse.IsValid)
// {
//     long count = searchResponse.Count;
//     Console.WriteLine($"Total count: {count}");
// }



var searchResponse = await NestClient.SearchAsync<MemoryItem>(s => s
    .Query(q => q
        .Term(t => t
            .Field(f => f.Id)
            .Value(14996975)
        )
    )
);

var s = NestClient.Search<MemoryItem>(s => s
    .Query(q => q
        .Bool(b => b
            .Filter(f => f
                .DateRange(r => r
                    .Field("DateTime")
                    .GreaterThanOrEquals("2021-01-01T00:00:00")
                    .LessThan("2021-01-31T23:59:59")
                )
            )
        )
    )
);



foreach (var hit in searchResponse.Hits)
{
    var document = hit.Source;
    Console.WriteLine($"ID: {hit.Id}, Timestamp: {document.DateTime}, Message: {document.Message}");
}

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
    public DateTime DateTime { get; set; }

    public Int64 MemoryLibId { get; set; }

    public string SrcContent { get; set; }
    public string Message { get; set; }

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