# $Order$

## $创建索引$

`curl -k -XPUT -u elastic:eGIutE2ZGircY53s30tf https://localhost:9200/index`

## $读取索引$

`curl -k -XPUT -u elastic:eGIutE2ZGircY53s30tf https://localhost:9200/index`

## $创建映射$

```
curl   -k -u elastic:eGIutE2ZGircY53s30tf  -XPOST https://localhost:9200/index/_mapping -H 'Content-Type:application/json' -d'
{
        "properties": {
            "content": {
                "type": "text",
                "analyzer": "ik_max_word",
                "search_analyzer": "ik_smart"
            }
        }

}'
```

## $索引文档$

```
curl -k -u elastic:eGIutE2ZGircY53s30tf  -XPOST https://localhost:9200/index/_create/1 -H 'Content-Type:application/json' -d'
{"content":"美国留给伊拉克的是个烂摊子吗"}
'
```

```
curl -k -u elastic:eGIutE2ZGircY53s30tf -XPOST https://localhost:9200/index/_create/2 -H 'Content-Type:application/json' -d'
{"content":"公安部：各地校车将享最高路权"}
'
```

```
curl -k -u elastic:eGIutE2ZGircY53s30tf -XPOST https://localhost:9200/index/_create/3 -H 'Content-Type:application/json' -d'
{"content":"中韩渔警冲突调查：韩警平均每天扣 1 艘中国渔船"}
'

```

```

curl -k -u elastic:eGIutE2ZGircY53s30tf -XPOST https://localhost:9200/index/_create/4 -H 'Content-Type:application/json' -d'
{"content":"中国驻洛杉矶领事馆遭亚裔男子枪击 嫌犯已自首"}
'

```

## $查询$

```

curl -k -u elastic:eGIutE2ZGircY53s30tf -XPOST https://localhost:9200/index/_search -H 'Content-Type:application/json' -d'
{
    "query" : { "match" : { "content" : "中国" }},
    "highlight" : {
        "pre_tags" : ["<tag1>", "<tag2>"],
        "post_tags" : ["</tag1>", "</tag2>"],
        "fields" : {
            "content" : {}
        }
    }
}
'

```

## $Result$
```
{
  "took": 196,
  "timed_out": false,
  "_shards": { "total": 1, "successful": 1, "skipped": 0, "failed": 0 },
  "hits": {
    "total": { "value": 2, "relation": "eq" },
    "max_score": 0.642793,
    "hits": [
      {
        "_index": "index",
        "_id": "3",
        "_score": 0.642793,
        "_source": {
          "content": "中韩渔警冲突调查：韩警平均每天扣 1 艘中国渔船"
        },
        "highlight": {
          "content": [
            "中韩渔警冲突调查：韩警平均每天扣 1 艘<tag1>中国</tag1>渔船"
          ]
        }
      },
      {
        "_index": "index",
        "_id": "4",
        "_score": 0.642793,
        "_source": { "content": "中国驻洛杉矶领事馆遭亚裔男子枪击 嫌犯已自首" },
        "highlight": {
          "content": [
            "<tag1>中国</tag1>驻洛杉矶领事馆遭亚裔男子枪击 嫌犯已自首"
          ]
        }
      }
    ]
  }
}

```