﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakoDeployCore;
using TakoDeployCore.DataContext;
using TakoDeployCore.Model;
using TakoDeployXUnit.Fixtures;
using Xunit;

namespace TakoDeployXUnit.Deployment
{
    [Collection("Database collection")]
    public class MultiDatabaseDeploymentTest
    {
        DatabaseFixture DBF;
        public MultiDatabaseDeploymentTest(DatabaseFixture database)
        {
            DBF = database;
            DocumentManager.Current = new DocumentManager();
        }

        [Fact]
        public async void Deploy()
        {
            DocumentManager.Current.DeploymentEvent += (a, b) => 
            {
                if (b.Exception != null)
                {
                    throw b.Exception;
                }
            };
            ClearAndAddSource();
            CleanAndAddNorthwind();
            
            //if (DocumentManager.Current.Deployment.Status != DeploymentStatus.Idle)
            //    throw new Exception("Fix this test");
            await DocumentManager.Current.Deploy();
            //if(DocumentManager.Current.Deployment.Status == DeploymentStatus.Error)
            //    throw new Exception("Fix this test");
            foreach (var item in DocumentManager.Current.Deployment.Targets)
            {
                var count = GetTableCount(item);
                if (count < 11)
                    Assert.True(false);
            }
            Assert.True(true);
        }

        private int? GetTableCount(TargetDatabase target)
        {
            var factory = DbProviderFactories.GetFactory(target.ProviderName);
            int? result = null;
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = target.ConnectionString;
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(1) as [TablesCount] FROM Sys.Tables";
                result = command.ExecuteScalar() as int?;
            }
            return result;
        }
        
        private void CleanAndAddNorthwind()
        {
            DocumentManager.Current.Deployment.ScriptFiles.Clear();
            DocumentManager.Current.Deployment.ScriptFiles.Add(
                new TakoDeployCore.Model.SqlScriptFile() {
                    Content = ScriptManager.GetNorthwind()
                });
        }

        private void ClearAndAddSource()
        {
            DocumentManager.Current.Deployment.Sources.Clear();
            DocumentManager.Current.Deployment.Sources.Add(
                new TakoDeployCore.Model.SourceDatabase()
                {
                    ConnectionString = DBF.ConnectionString,
                    ProviderName = DBF.ProviderName,
                    Type = SourceType.DataSource,
                    NameFilter = "Tako"
                }
            );
        }
    }
}
