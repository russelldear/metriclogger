﻿using System;
using System.Collections.Generic;
using System.Threading;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using MetricLogger.Model;

namespace MetricLogger.Services
{
    public class DynamoDbService
    {
        private const string _tableName = "MetricLog";
        private const string _key = "MetricId";
        private const string _sortKey = "Timestamp";

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
                EnsureTable();

                var context = new DynamoDBContext(_dynamo);

                context.SaveAsync<MetricLog>(metric).Wait();

                //Console.WriteLine("Metric persisted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }

            return true;
        }

        private void EnsureTable()
        {
            var tableResponse = _dynamo.ListTablesAsync().Result;

            if (!tableResponse.TableNames.Contains(_tableName))
            {
                CreateTable(_tableName);
            }

            var tableStatus = _dynamo.DescribeTableAsync(_tableName).Result;

            var isTableAvailable = tableStatus.Table.TableStatus == "ACTIVE";

            while (!isTableAvailable)
            {
                Thread.Sleep(5000);
                isTableAvailable = tableStatus.Table.TableStatus == "ACTIVE";
            }
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
                    },
                    new KeySchemaElement
                    {
                        AttributeName = _sortKey,
                        KeyType = KeyType.RANGE
                    }
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition {
                        AttributeName = _key,
                        AttributeType = ScalarAttributeType.S
                    },
                    new AttributeDefinition {
                        AttributeName = _sortKey,
                        AttributeType = ScalarAttributeType.S
                    }
                }
            }).Result;

            Console.WriteLine("Table create status: " + result.HttpStatusCode);
        }
    }
}