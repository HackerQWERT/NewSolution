## Elasticsearch 安全配置信息

Elasticsearch 的安全功能已经自动配置完成:

- ✅ 启用了认证
- ✅ 集群连接被加密

### elastic 用户密码

`*1cUXTMYr*=PnbN_F0E-`

可以使用 `bin/elasticsearch-reset-password -u elastic` 重置密码。

### HTTP CA 证书 SHA256 指纹

`7d8553575da1e029d27f5426826d0561b9706ccc0df62415880e0a78cf96a770`

### 配置 Kibana 所需的入学令牌

```
eyJ2ZXIiOiI4LjguMiIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiN2Q4NTUzNTc1ZGExZTAyOWQyN2Y1NDI2ODI2ZDA1NjFiOTcwNmNjYzBkZjYyNDE1ODgwZTBhNzhjZjk2YTc3MCIsImtleSI6Im94V3hpNGtCS0dOa0FPTkRyWFN6OjRDaUUzMU1LUTlDd19VVDhRSjk1SHcifQ==
```

注册令牌的有效期为 30 分钟。 如果您需要生成新的注册令牌，请在现有节点上运行 elasticsearch-create-enrollment-token 工具。 该工具位于 Docker 容器的 Elasticsearch bin 目录中。

例如，在现有 es01 节点上运行以下命令，为新的 Elasticsearch 节点生成注册令牌：

docker exec -it es01 /usr/share/elasticsearch/bin/elasticsearch-create-enrollment-token -s node

### 加入集群所需的入学令牌

```
eyJ2ZXIiOiI4LjguMiIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiN2Q4NTUzNTc1ZGExZTAyOWQyN2Y1NDI2ODI2ZDA1NjFiOTcwNmNjYzBkZjYyNDE1ODgwZTBhNzhjZjk2YTc3MCIsImtleSI6InBCV3hpNGtCS0dOa0FPTkRyWFN6OjNOdjR2LUxZUm11WGhvM1JZWGhFYXcifQ==
```

### 如果在 Docker 中运行,可使用以下命令加入集群

```
docker run -e "ENROLLMENT_TOKEN=<token>" docker.elastic.co/elasticsearch/elasticsearch:8.8.2
```
