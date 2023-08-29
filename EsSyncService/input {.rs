input {
  jdbc {
    jdbc_driver_class => "com.mysql.jdbc.Driver"
    jdbc_driver_library => "/ssd/1/share/xxx/logstash/current/config/custom/mysql-connector-java-8.0.18.jar"
    jdbc_connection_string => "jdbc:mysql://xxx:3306/DataCentre?useUnicode=true&characterEncoding=utf-8&useSSL=false&allowLoadLocalInfile=false&autoDeserialize=false"
    jdbc_user => ""
    jdbc_password => ""
    jdbc_paging_enabled => "true"
    jdbc_page_size => "1000"
    statement => "SELECT UNIX_TIMESTAMP(UpdateTime) AS lastRunTime,Id AS id,MemoryLibId AS memoryLibId,SrcContent AS srcContent,SrcContentMD5 AS srcContentMD5,TgtContent AS tgtContent,TgtContentMD5 AS tgtContentMD5,ReleaseNumber AS releaseNumber,Matching AS matching,SrcLanguage AS srcLanguage,TgtLanguage AS tgtLanguage,Seq AS seq,CreateTime AS createTime,UpdateTime AS updateTime,`Status` AS 'status' FROM MemoryItem where updateTime > FROM_UNIXTIME(:sql_last_value) AND updateTime < NOW()"
    schedule => "*/3 * * * * *" #每1s执行一次
    record_last_run => true
    lowercase_column_names => false
    last_run_metadata_path => "/ssd/1/xxx/logstash/data/last_run_metadata_update_time_numeric.txt"
    clean_run => false
    tracking_column => "lastRunTime"
    tracking_column_type => "numeric"
    use_column_value => true
  }
}
filter {
  mutate {
    remove_field => ["lastRunTime"]
  }
}
output {
 elasticsearch {
    hosts => "http://xxx:9200"
    index => "memoryitem"
    user => ""
    password => ""
    document_id => "%{id}"
  }
  file_extend {
    path => "/ssd/1/xxx/logstash/logs/debug/memoryItem"
  }
}