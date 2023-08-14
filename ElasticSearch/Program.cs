
var node = new Uri("https://localhost:9200"); // Elasticsearch节点的URL

var settings = new ConnectionSettings(node)
    // .BasicAuthentication("elastic", "li*M8VNdSsCRvU0IHoE3") //8.9.0
    .BasicAuthentication("elastic", "eGIutE2ZGircY53s30tf") //8.8.2

    .DefaultIndex("your-default-index"); // 设置默认索引


var elasticClient = new ElasticClient(settings);

var people = new List<Person>
{
    new Person { Id = 9, FirstName = "a", LastName = "k", Age = 30 },
    new Person { Id = 7, FirstName = "b", LastName = "5", Age = 25 },
    new Person { Id = 4, FirstName = "c", LastName = "7", Age = 40 }
};

var s = elasticClient.Search<Person>(s => s
    .From(0)
    .Size(10)
    .Query(q => q
        .MatchAll()
    )
);


foreach (var person in people)
{
    var indexResponse = elasticClient.Index(person, idx => idx.Index("your-default-index"));
}


s = elasticClient.Search<Person>(s => s
    .From(0)
    .Size(10)
    .Query(q => q
        .MatchAll()
    )
);

foreach (var hit in s.Hits)
{
    Console.WriteLine(hit.Source.FirstName);
}

//discovery.type=single-node
//xpack.security.enabled=false