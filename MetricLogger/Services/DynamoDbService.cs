using System;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using MetricLogger.Model;
using System.Collections.Generic;
using System.Threading;

namespace MetricLogger.Services
{
    public class DynamoDbService
    {
        private const string _tableName = "Metrics";
        private const string _key = "MetricId";

        private readonly AmazonDynamoDBClient _dynamo;

        public DynamoDbService()
        {
            var credentials = new BasicAWSCredentials(Environment.GetEnvironmentVariable("AWSAccessKey"), Environment.GetEnvironmentVariable("AWSSecret"));

            _dynamo = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
        }

        public bool AddMetric(MetricLog metric)
        {
            try
            {
                var tableResponse = _dynamo.ListTablesAsync().Result;

                if (!tableResponse.TableNames.Contains(_tableName))
                {
                    CreateTable(_tableName);
                }

                bool isTableAvailable = false;
                while (!isTableAvailable)
                {
                    Thread.Sleep(5000);
                    var tableStatus = _dynamo.DescribeTableAsync(_tableName).Result;
                    isTableAvailable = tableStatus.Table.TableStatus == "ACTIVE";
                }

                var context = new DynamoDBContext(_dynamo);

                var result = context.SaveAsync<MetricLog>(metric, (result) => 
                {
                    if (result.Exception == null)
                    {
                        Console.WriteLine(result);
                    }
                    else
                    {
                        Console.WriteLine(result.Exception);
                    }
                });

                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }

            return true;
        }

        private void CreateTable(string tableName)
        {
            var result = _dynamo.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 3,
                    WriteCapacityUnits = 1
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName = _key,
                        KeyType = KeyType.HASH
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition {
                        AttributeName = _key,
                        AttributeType = ScalarAttributeType.S
                    }
                }
            }).Result;

            Console.WriteLine(result.HttpStatusCode);
        }
    }
}
