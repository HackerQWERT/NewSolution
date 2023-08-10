# 配置 ES

https://www.elastic.co/guide/en/elasticsearch/reference/8.9/docker.html#docker-cli-run-dev-mode

# 拉取 Elasticsearch Docker 镜像编辑

`docker pull docker.elastic.co/elasticsearch/elasticsearch:8.9.0`

# 可选：验证 Elasticsearch Docker 镜像签名

`wget https://artifacts.elastic.co/cosign.pub`
`cosign verify --key cosign.pub docker.elastic.co/elasticsearch/elasticsearch:8.9.0`

## 该命令以 JSON 格式打印检查结果和签名有效负载：

```
Verification for docker.elastic.co/elasticsearch/elasticsearch:{version} --
The following checks were performed on each of these signatures:

- The cosign claims were validated
- Existence of the claims in the transparency log was verified offline
- The signatures were verified against the specified public key

```

# 使用 Docker 启动单节点群集

## 为 Elasticsearch 和 Kibana 创建新的 docker 网络

`docker network create elastic`

## 为 Elasticsearch 和 Kibana 创建新的 docker 网络

`docker run --name es01 --net elastic -p 9200:9200 -it docker.elastic.co/elasticsearch/elasticsearch:8.9.0`

## 复制生成的密码和注册令牌，并将其保存在安全的环境中 位置。这些值仅在您第一次启动 Elasticsearch 时显示。 如果您需要重置用户的密码或其他 内置用户，运行弹性搜索重置密码工具。 此工具位于 Docker 容器的 Elasticsearch 目录中。 例如：elastic/bin

`docker exec -it es01 /usr/share/elasticsearch/bin/elasticsearch-reset-password`

## 将安全证书从 Docker 容器复制到 您的本地计算机。http_ca.crt

`docker cp es01:/usr/share/elasticsearch/config/certs/http_ca.crt .`
$`安装证书`$

## 打开一个新终端，并验证您是否可以通过以下方法连接到您的 Elasticsearch 集群 使用从中复制的文件进行经过身份验证的调用 您的 Docker 容器。出现提示时输入用户的密码。http_ca.crtelastic

`curl --cacert http_ca.crt -u elastic https://localhost:9200`

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

```

```

```

```
