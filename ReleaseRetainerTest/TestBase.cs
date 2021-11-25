using Newtonsoft.Json;
using NUnit.Framework;
using DevOpsDeploy;
using System.Collections.Generic;
using System.IO;

namespace DevOpsDeployTest
{
    public class TestBase
    {

        internal CustomerModel CustomerModel { get; set; }

        #region Setup
        [SetUp]
        public void Setup()
        {
            // reinitialise customer model to avoid data modified in previous tests impacting subsequent tests
            CustomerModel = new CustomerModel();

            // Load test data files
            CustomerModel.Releases.AddItems(JsonConvert.DeserializeObject<List<ReleaseModel>>(readTestDataFile("Releases")));
            CustomerModel.Projects.AddItems(JsonConvert.DeserializeObject<List<ProjectModel>>(readTestDataFile("Projects")));
            CustomerModel.Environments.AddItems(JsonConvert.DeserializeObject<List<EnvironmentModel>>(readTestDataFile("Environments")));
            CustomerModel.Deployments.AddItems(JsonConvert.DeserializeObject<List<DeploymentModel>>(readTestDataFile("Deployments")));
        }

        private string readTestDataFile(string entityName)
        {
            var filename = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", $"{entityName}.json");
            return File.ReadAllText(filename);
        }

        #endregion
    }
}

