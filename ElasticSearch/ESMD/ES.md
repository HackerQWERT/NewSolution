# Start ES

$cd /path/to/elasticsearch$

$./bin/elasticsearch$

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

## 使用 elasticsearch-plugin 进行安装（从版本 v5.5.1 开始支持）：

https://github.com/medcl/elasticsearch-analysis-ik

`./bin/elasticsearch-plugin install https://github.com/medcl/elasticsearch-analysis-ik/releases/download/v8.8.2/elasticsearch-analysis-ik-8.8.2.zip`

# Elasticsearch 安全配置信息

Elasticsearch 的安全功能已经自动配置完成:

- ✅ 启用了认证
- ✅ 集群连接被加密

### elastic 用户密码

#### 8.9.0

`li*M8VNdSsCRvU0IHoE3`

#### 8.8.2

`eGIutE2ZGircY53s30tf`

可以使用 `bin/elasticsearch-reset-password -u elastic` 重置密码。

### HTTP CA 证书 SHA256 指纹

#### 8.9.0

` 67d28593b8f52ad07bdef8da44fccba1ad05fb2e9ae0bc9729d471eff1ee88a3`

#### 8.8.2

` bb7754cc33e56594e4009d67086864064d25310d7d265ba2941b8fd46101288d`

### 复制证书

`docker cp es01:/usr/share/elasticsearch/config/certs/http_ca.crt .`

### 配置 Kibana 所需的入学令牌

#### 8.9.0

```
  eyJ2ZXIiOiI4LjkuMCIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiNjdkMjg1OTNiOGY1MmFkMDdiZGVmOGRhNDRmY2NiYTFhZDA1ZmIyZTlhZTBiYzk3MjlkNDcxZWZmMWVlODhhMyIsImtleSI6IjZEc2k0b2tCbGxtX2pnZ21PUlpxOk4zcU9yUnktUlJXcmxUZG1Sdm1KNUEifQ==

```

#### 8.8.2

```
 eyJ2ZXIiOiI4LjguMiIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiYmI3NzU0Y2MzM2U1NjU5NGU0MDA5ZDY3MDg2ODY0MDY0ZDI1MzEwZDdkMjY1YmEyOTQxYjhmZDQ2MTAxMjg4ZCIsImtleSI6Im5jTlE0b2tCMUl3bnREREZNV0d0OmFCYWExN3ZwUlVxZEJ1SV9sZERwREEifQ==
```

注册令牌的有效期为 30 分钟。 如果您需要生成新的注册令牌，请在现有节点上运行 elasticsearch-create-enrollment-token 工具。 该工具位于 Docker 容器的 Elasticsearch bin 目录中。

例如，在现有 es01 节点上运行以下命令，为新的 Elasticsearch 节点生成注册令牌：

`docker exec -it ES /usr/share/elasticsearch/bin/elasticsearch-create-enrollment-token -s node`

### 加入集群所需的入学令牌

#### 8.9.0

```
  eyJ2ZXIiOiI4LjkuMCIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiNjdkMjg1OTNiOGY1MmFkMDdiZGVmOGRhNDRmY2NiYTFhZDA1ZmIyZTlhZTBiYzk3MjlkNDcxZWZmMWVlODhhMyIsImtleSI6IjV6c2k0b2tCbGxtX2pnZ21PUlpxOlRBQVBNZjNDVGFxM1ltUGRTVW1mMEEifQ==

```

#### 8.8.2

```
  eyJ2ZXIiOiI4LjguMiIsImFkciI6WyIxNzIuMTguMC4yOjkyMDAiXSwiZmdyIjoiYmI3NzU0Y2MzM2U1NjU5NGU0MDA5ZDY3MDg2ODY0MDY0ZDI1MzEwZDdkMjY1YmEyOTQxYjhmZDQ2MTAxMjg4ZCIsImtleSI6Im5zTlE0b2tCMUl3bnREREZNV0d0OlV3bFZiMmNyU2Jhc0tiQV9kc1Voa0EifQ==
```

### 如果在 Docker 中运行,可使用以下命令加入集群

```

docker run -e "ENROLLMENT_TOKEN=<token>" docker.elastic.co/elasticsearch/elasticsearch:8.9.0

```

# Linux 安装 ES

```
wget https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-8.8.2-linux-x86_64.tar.gz
wget https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-8.8.2-linux-x86_64.tar.gz.sha512
shasum -a 512 -c elasticsearch-8.8.2-linux-x86_64.tar.gz.sha512
tar -xzf elasticsearch-8.8.2-linux-x86_64.tar.gz
cd elasticsearch-8.8.2/
```

```
LICENSE.txt  NOTICE.txt  README.asciidoc  bin  config  elasticsearch-8.9.0  elasticsearch-8.9.0-darwin-x86_64.tar.gz  jdk  lib  logs  modules  plugins
```


# $IKAnalyzer.cfg.xml$
```
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE properties SYSTEM "http://java.sun.com/dtd/properties.dtd">
<properties>
	<comment>IK Analyzer 扩展配置</comment>
	<!--用户可以在这里配置自己的扩展字典 -->
	<entry key="ext_dict">custom/mydict.dic;custom/single_word_low_freq.dic</entry>
	 <!--用户可以在这里配置自己的扩展停止词字典-->
	<entry key="ext_stopwords">custom/ext_stopword.dic</entry>
 	<!--用户可以在这里配置远程扩展字典 -->
	<entry key="remote_ext_dict">location</entry>
 	<!--用户可以在这里配置远程扩展停止词字典-->
	<entry key="remote_ext_stopwords">http://xxx.com/xxx.dic</entry>
</properties>
```



# $ES热更新$
```
热更新 IK 分词使用方法
目前该插件支持热更新 IK 分词，通过上文在 IK 配置文件中提到的如下配置
```

$用户可以在这里配置远程扩展字典$*

` <entry key="remote_ext_dict">location</entry>`

$用户可以在这里配置远程扩展停止词字典$

`<entry key="remote_ext_stopwords">location</entry>`


其中 *`location`* 是指一个 url，比如 *http://yoursite.com/getCustomDict*，该请求只需满足以下两点即可完成分词热更新。

该 http 请求需要返回两个头部(header)，一个是 *`Last-Modified`*，一个是 *`ETag`*，这两者都是字符串类型，只要有一个发生变化，该插件就会去抓取新的分词进而更新词库。

该 http 请求返回的内容格式是一行一个分词，换行符用 \n 即可。

满足上面两点要求就可以实现热更新分词了，*`不需要重启 ES 实例`*。

可以将需自动更新的热词放在一个 *`UTF-8`* 编码的 .txt 文件里，放在 nginx 或其他简易 http server 下，当 .txt 文件修改时，http server 会在客户端请求该文件时自动返回相应的 Last-Modified 和 ETag。可以另外做一个工具来从业务系统提取相关词汇，并更新这个 .txt 文件。

have fun.