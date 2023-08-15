# $Order$

## $创建索引$

`curl -k -XPUT -u elastic:eGIutE2ZGircY53s30tf https://localhost:9200/index`

## $读取索引$

`curl -k -XGET -u elastic:eGIutE2ZGircY53s30tf  https://localhost:9200/index/_search`

## $创建映射$

```shell
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
## $读取索引$
```shell
curl   -k -u elastic:eGIutE2ZGircY53s30tf  -XGET https://localhost:9200/index/_mapping -H 'Content-Type:application/json' 
```
## $索引文档$

```shell
curl -k -u elastic:eGIutE2ZGircY53s30tf  -XPOST https://localhost:9200/index/_create/1 -H 'Content-Type:application/json' -d'
{"content":"美国留给伊拉克的是个烂摊子吗"}
'
```

```shell
curl -k -u elastic:eGIutE2ZGircY53s30tf -XPOST https://localhost:9200/index/_create/2 -H 'Content-Type:application/json' -d'
{"content":"公安部：各地校车将享最高路权"}
'
```

```shell
curl -k -u elastic:eGIutE2ZGircY53s30tf -XPOST https://localhost:9200/index/_create/3 -H 'Content-Type:application/json' -d'
{"content":"中韩渔警冲突调查：韩警平均每天扣 1 艘中国渔船"}
'

```

```shell
curl -k -u elastic:eGIutE2ZGircY53s30tf -XPOST https://localhost:9200/index/_create/4 -H 'Content-Type:application/json' -d'
{"content":"中国驻洛杉矶领事馆遭亚裔男子枪击 嫌犯已自首"}
'
```
```shell
curl -k -u elastic:eGIutE2ZGircY53s30tf -XPOST https://localhost:9200/index/_create/4 -H 'Content-Type:application/json' -d'
{"content":"我 went to the store, где I bought some groceries and ein Buch."}
'
```
##  $分析$
```shell
curl -k -u elastic:eGIutE2ZGircY53s30tf -XGET https://localhost:9200/index/_analyze -H 'Content-Type:application/json' -d'
{
  "field":"content",
  "text":"中国驻洛杉矶领事馆遭亚裔男子枪击 嫌犯已自首"
}
'
```
```shell
curl -k -u elastic:eGIutE2ZGircY53s30tf -XGET https://localhost:9200/index/_analyze -H 'Content-Type:application/json' -d'
{
  "field":"content",
  "text":"我 went to the store, где I bought some groceries and ein Buch."
}
'
```
## $结果$
```json
{
  "tokens": [
    {
      "token": "中国",
      "start_offset": 0,
      "end_offset": 2,
      "type": "CN_WORD",
      "position": 0
    },
    {
      "token": "驻",
      "start_offset": 2,
      "end_offset": 3,
      "type": "CN_CHAR",
      "position": 1
    },
    {
      "token": "洛杉矶",
      "start_offset": 3,
      "end_offset": 6,
      "type": "CN_WORD",
      "position": 2
    },
    {
      "token": "领事馆",
      "start_offset": 6,
      "end_offset": 9,
      "type": "CN_WORD",
      "position": 3
    },
    {
      "token": "领事",
      "start_offset": 6,
      "end_offset": 8,
      "type": "CN_WORD",
      "position": 4
    },
    {
      "token": "馆",
      "start_offset": 8,
      "end_offset": 9,
      "type": "CN_CHAR",
      "position": 5
    },
    {
      "token": "遭",
      "start_offset": 9,
      "end_offset": 10,
      "type": "CN_CHAR",
      "position": 6
    },
    {
      "token": "亚裔",
      "start_offset": 10,
      "end_offset": 12,
      "type": "CN_WORD",
      "position": 7
    },
    {
      "token": "男子",
      "start_offset": 12,
      "end_offset": 14,
      "type": "CN_WORD",
      "position": 8
    },
    {
      "token": "子枪",
      "start_offset": 13,
      "end_offset": 15,
      "type": "CN_WORD",
      "position": 9
    },
    {
      "token": "枪击",
      "start_offset": 14,
      "end_offset": 16,
      "type": "CN_WORD",
      "position": 10
    },
    {
      "token": "嫌犯",
      "start_offset": 17,
      "end_offset": 19,
      "type": "CN_WORD",
      "position": 11
    },
    {
      "token": "已",
      "start_offset": 19,
      "end_offset": 20,
      "type": "CN_CHAR",
      "position": 12
    },
    {
      "token": "自首",
      "start_offset": 20,
      "end_offset": 22,
      "type": "CN_WORD",
      "position": 13
    }
  ]
}

```

```json
{"tokens":[{"token":"我","start_offset":0,"end_offset":1,"type":"CN_CHAR","position":0},{"token":"went","start_offset":2,"end_offset":6,"type":"ENGLISH","position":1},{"token":"store","start_offset":14,"end_offset":19,"type":"ENGLISH","position":2},{"token":"i","start_offset":25,"end_offset":26,"type":"ENGLISH","position":3},{"token":"bought","start_offset":27,"end_offset":33,"type":"ENGLISH","position":4},{"token":"some","start_offset":34,"end_offset":38,"type":"ENGLISH","position":5},{"token":"groceries","start_offset":39,"end_offset":48,"type":"ENGLISH","position":6},{"token":"ein","start_offset":53,"end_offset":56,"type":"ENGLISH","position":7},{"token":"buch.","start_offset":57,"end_offset":62,"type":"LETTER","position":8},{"token":"buch","start_offset":57,"end_offset":61,"type":"ENGLISH","position":9}]}
```



## $查询$

```shell
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
```json
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