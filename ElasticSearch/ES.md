## Elasticsearch 安全配置信息

Elasticsearch 的安全功能已经自动配置完成:

- ✅ 启用了认证
- ✅ 集群连接被加密

### elastic 用户密码

`+EUF8y2taO0B=8KN4rGw`

可以使用 `bin/elasticsearch-reset-password -u elastic` 重置密码。

### HTTP CA 证书 SHA256 指纹

` 84af81652687827019988377b4d44f4c319f82296bbcf25064c41a4fc10dbe72`

### 复制证书

`docker cp ES:/usr/share/elasticsearch/config/certs/http_ca.crt .`

### 配置 Kibana 所需的入学令牌

```
eyJ2ZXIiOiI4LjkuMCIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiODRhZjgxNjUyNjg3ODI3MDE5OTg4Mzc3YjRkNDRmNGMzMTlmODIyOTZiYmNmMjUwNjRjNDFhNGZjMTBkYmU3MiIsImtleSI6IlZhR3Mzb2tCa3ljVlBrWnpqV2xoOktfSEpYNTJDVHdlVnlWZHBCX3VzZVEifQ==
```

注册令牌的有效期为 30 分钟。 如果您需要生成新的注册令牌，请在现有节点上运行 elasticsearch-create-enrollment-token 工具。 该工具位于 Docker 容器的 Elasticsearch bin 目录中。

例如，在现有 es01 节点上运行以下命令，为新的 Elasticsearch 节点生成注册令牌：

`docker exec -it ES /usr/share/elasticsearch/bin/elasticsearch-create-enrollment-token -s node`

### 加入集群所需的入学令牌

```
eyJ2ZXIiOiI4LjkuMCIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiODRhZjgxNjUyNjg3ODI3MDE5OTg4Mzc3YjRkNDRmNGMzMTlmODIyOTZiYmNmMjUwNjRjNDFhNGZjMTBkYmU3MiIsImtleSI6IlY2R3Mzb2tCa3ljVlBrWnpqV2xrOkg3c1lOTkdQUnptSzh5R1JXYkhocEEifQ==
```

### 如果在 Docker 中运行,可使用以下命令加入集群

```
docker run -e "ENROLLMENT_TOKEN=<token>" docker.elastic.co/elasticsearch/elasticsearch:8.9.0
```

# 配置 Kibana

## 在 Docker 上运行 Kibana 进行开发编辑

`启动一个用于开发或测试的 Elasticsearch 容器：`

`为 Elasticsearch 和 Kibana 创建一个新的 Docker 网络：`

`docker network create elastic`

`拉取 Elasticsearch Docker 镜像：`

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

## 拉取 Kibana Docker 镜像：

`docker pull docker.elastic.co/kibana/kibana:8.9.0`

## 可选：验证 Kibana Docker 镜像签名：

`wget https://artifacts.elastic.co/cosign.pub`

$cosign verify --key cosign.pub docker.elastic.co/kibana/kibana:8.9.0$

_`有关此步骤的详细信息，请参阅 Elasticsearch 文档中的验证 Elasticsearch Docker 镜像签名。`_

## $在 Docker 中启动 Kibana：$

`docker run --name kib-01 --net elastic -p 5601:5601 docker.elastic.co/kibana/kibana:8.9.0`

启动 Kibana 时，将输出到终端的唯一链接。

要访问 Kibana，请在终端中单击生成的链接。

在浏览器中，粘贴启动时复制的注册令牌 Elasticsearch，然后单击按钮以将您的 Kibana 实例与 Elasticsearch 连接。
使用生成的密码以用户身份登录 Kibana 当你启动 Elasticsearch 时。elastic
生成密码和注
