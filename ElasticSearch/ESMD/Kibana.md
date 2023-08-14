# 配置 Kibana

https://www.elastic.co/guide/en/kibana/current/docker.html#run-kibana-on-docker-for-dev

## 在 Docker 上运行 Kibana 进行开发编辑

`启动一个用于开发或测试的 Elasticsearch 容器：`

`为 Elasticsearch 和 Kibana 创建一个新的 Docker 网络：`

`docker network create elastic`

## 拉取 Elasticsearch Docker 镜像：

`docker pull docker.elastic.co/elasticsearch/elasticsearch:8.9.0`

## 可选：验证 Elasticsearch Docker 镜像签名：

`wget https://artifacts.elastic.co/cosign.pub`

`cosign verify --key cosign.pub docker.elastic.co/kibana/kibana:8.9.0`

`有关此步骤的详细信息，请参阅 Elasticsearch 文档中的验证 Elasticsearch Docker 镜像签名。`

## 在 Docker 中启动 Elasticsearch：

`docker run --name es-node01 --net elastic -p 9200:9200 -p 9300:9300 -t docker.elastic.co/elasticsearch/elasticsearch:8.9.0`

复制生成的密码和注册令牌，并将其保存在安全的环境中 位置。这些值仅在您第一次启动 Elasticsearch 时显示。 您将使用这些功能将 Kibana 注册到 Elasticsearch 集群并登录。
在新的终端会话中，启动 Kibana 并将其连接到 Elasticsearch 容器：

`docker pull docker.elastic.co/kibana/kibana:8.9.0`

`docker run --name kib-01 --net elastic -p 5601:5601 docker.elastic.co/kibana/kibana:8.9.0`

`wget https://artifacts.elastic.co/cosign.pub`

`cosign verify --key cosign.pub docker.elastic.co/kibana/kibana:8.9.0`

_`有关此步骤的详细信息，请参阅 Elasticsearch 文档中的验证 Elasticsearch Docker 镜像签名。`_

## $在 Docker 中启动 Kibana：$

`docker run --name kib-01 --net elastic -p 5601:5601 docker.elastic.co/kibana/kibana:8.9.0`

启动 Kibana 时，将输出到终端的唯一链接。

要访问 Kibana，请在终端中单击生成的链接。

在浏览器中，粘贴启动时复制的注册令牌 Elasticsearch，然后单击按钮以将您的 Kibana 实例与 Elasticsearch 连接。
使用生成的密码以用户身份登录 Kibana
