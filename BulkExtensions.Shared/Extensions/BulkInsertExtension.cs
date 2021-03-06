﻿using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using EntityFramework.BulkExtensions.Commons.Context;
using EntityFramework.BulkExtensions.Commons.Flags;
using EntityFramework.BulkExtensions.Commons.Helpers;
using EntityFramework.BulkExtensions.Commons.Mapping;

namespace EntityFramework.BulkExtensions.Commons.Extensions
{
    internal static class BulkInsertExtension
    {
        internal static void BulkInsertToTable<TEntity>(this IDbContextWrapper context, IList<TEntity> entities,
            string tableName, Operation operationType, BulkOptions options) where TEntity : class
        {
            var properties = context.EntityMapping
                .GetPropertiesByOperation(operationType)
                .ToList();
            if (context.EntityMapping.WillOutputGeneratedValues(options))
            {
                properties.Add(new PropertyMapping
                {
                    ColumnName = SqlHelper.Identity,
                    PropertyName = SqlHelper.Identity
                });
            }

            using (var bulkcopy = new SqlBulkCopy((SqlConnection)context.Connection, SqlBulkCopyOptions.Default,
                (SqlTransaction)context.Transaction))
            {
                foreach (var column in properties)
                {
                    bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                bulkcopy.BatchSize = context.BatchSize;
                bulkcopy.DestinationTableName = tableName;
                bulkcopy.BulkCopyTimeout = context.Timeout;
                bulkcopy.WriteToServer(entities.ToDataReader(context.EntityMapping, properties));
            }
        }
    }
}