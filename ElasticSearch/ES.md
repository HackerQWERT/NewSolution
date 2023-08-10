## Elasticsearch 安全配置信息

Elasticsearch 的安全功能已经自动配置完成:

- ✅ 启用了认证
- ✅ 集群连接被加密

### elastic 用户密码

`vC=yvOIqkxlEU9aYkxCO`

可以使用 `bin/elasticsearch-reset-password -u elastic` 重置密码。

### HTTP CA 证书 SHA256 指纹

`02880827049eaef23f374b56f6266f0ec45b25b8d6ee61312e7acda03c8f2963`

### 复制证书

`docker cp ES:/usr/share/elasticsearch/config/certs/http_ca.crt .`

### 配置 Kibana 所需的入学令牌

```
eyJ2ZXIiOiI4LjkuMCIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiMDI4ODA4MjcwNDllYWVmMjNmMzc0YjU2ZjYyNjZmMGVjNDViMjViOGQ2ZWU2MTMxMmU3YWNkYTAzYzhmMjk2MyIsImtleSI6IjI2Nkszb2tCb1hKR1JuMDZoV2VMOmlNZlU5YlZ2UUt5WVMyaGlLdE1IYncifQ==
```

注册令牌的有效期为 30 分钟。 如果您需要生成新的注册令牌，请在现有节点上运行 elasticsearch-create-enrollment-token 工具。 该工具位于 Docker 容器的 Elasticsearch bin 目录中。

例如，在现有 es01 节点上运行以下命令，为新的 Elasticsearch 节点生成注册令牌：

`docker exec -it ES /usr/share/elasticsearch/bin/elasticsearch-create-enrollment-token -s node`

### 加入集群所需的入学令牌

```
eyJ2ZXIiOiI4LjkuMCIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiMDI4ODA4MjcwNDllYWVmMjNmMzc0YjU2ZjYyNjZmMGVjNDViMjViOGQ2ZWU2MTMxMmU3YWNkYTAzYzhmMjk2MyIsImtleSI6IjNLNkszb2tCb1hKR1JuMDZoV2VMOndJRmJoNGJnVHdxVEYzTEVad0tnY3cifQ==
```

### 如果在 Docker 中运行,可使用以下命令加入集群

```
docker run -e "ENROLLMENT_TOKEN=<token>" docker.elastic.co/elasticsearch/elasticsearch:8.8.2
```

# 配置 Kibana

`docker pull docker.elastic.co/kibana/kibana:<版本号>`

# 运行 Kibana

`docker run -d --name my_kibana -p 5601:5601 -e ELASTICSEARCH_HOSTS=http://<Elasticsearch_IP>:<Elasticsearch_Port> docker.elastic.co/kibana/kibana:<版本号>`
