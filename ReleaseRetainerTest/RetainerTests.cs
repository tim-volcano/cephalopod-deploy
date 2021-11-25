using NUnit.Framework;
using DevOpsDeploy;
using System.Collections.Generic;

namespace DevOpsDeployTest
{
    public class RetainerTests : TestBase
    {
        /// <summary>
        /// Confirm the correct number of records have been loaded from TestData
        /// </summary>
        [Test]
        public void AssertTestDataLoaded()
        {
            Assert.AreEqual(7, CustomerModel.Releases.Count, "Incorrect number of release records.");
            Assert.AreEqual(2, CustomerModel.Projects.Count, "Incorrect number of project records.");
            Assert.AreEqual(2, CustomerModel.Environments.Count, "Incorrect number of environment records.");
            Assert.AreEqual(14, CustomerModel.Deployments.Count, "Incorrect number of deployment records.");
        }

        #region Test against TestData for different maxRelease values
        /// <summary>
        /// Confirm correct results return when setting maxResults to 1
        /// </summary>
        [Test]
        public void AssertTestData_1_release()
        {
            AssertTestData_n_releases(1, 3, new List<string> { "Release-3", "Release-7", "Release-4" });
        }

        /// <summary>
        /// Confirm correct results return when setting maxResults to 2
        /// </summary>
        [Test]
        public void AssertTestData_2_releases()
        {
            AssertTestData_n_releases(2, 6, new List<string> { "Release-3", "Release-7", "Release-4", "Release-2", "Release-6", "Release-5" });

        }

        /// <summary>
        /// Confirm correct results return when setting maxResults to 3 (should return all releases)
        /// </summary>
        [Test]
        public void AssertTestData_3_releases()
        {
            AssertTestData_n_releases(3, 7, new List<string> { "Release-3", "Release-7", "Release-4", "Release-2", "Release-6", "Release-5", "Release-1" });

        }

        /// <summary>
        /// Confirm correct results return when setting maxResults to 4 (should return all releases)
        /// </summary>
        [Test]
        public void AssertTestData_4_releases()
        {
            AssertTestData_n_releases(4, 7, new List<string> { "Release-3", "Release-7", "Release-4", "Release-2", "Release-6", "Release-5", "Release-1" });

        }

        /// <summary>
        /// Confirm correct results return when setting maxResults to a value larger than the number of deployments (should return all releases)
        /// </summary>
        [Test]
        public void AssertTestData_100_releases()
        {
            AssertTestData_n_releases(100, 7, new List<string> { "Release-3", "Release-7", "Release-4", "Release-2", "Release-6", "Release-5", "Release-1" });
        }


        /// <summary>
        /// Generic helper function
        /// </summary>
        /// <param name="n">Max release to return for Expense/Project</param>
        /// <param name="expectedCount">Expected number of releases to be returned</param>
        /// <param name="expectedReleases">Release Ids of the releases to be returned</param>
        /// <param name="model">Customer data model. If not supplied the TestBase.CustomerModel will be used</param>
        private void AssertTestData_n_releases(int n, int expectedCount, List<string> expectedReleases, CustomerModel model = null)
        {
            if (model == null)
                model = CustomerModel;

            var result = new Retainer().GetReleasesToRetain(n, model);
            Assert.AreEqual(expectedCount, result.Count, "Incorrect number of releases retained");
            foreach (var release in expectedReleases)
                Assert.IsTrue(result.Contains(release), $"Result does not contain expected release '{release}'.");
        }

        #endregion

        #region Test for invalid parameters
        /// <summary>
        /// Test where specifying a maxReleases of zero
        /// </summary>
        [Test]
        public void AssertParameters_0_release()
        {
            AssertTestData_n_releases(0, 0, new List<string>());
        }

        /// <summary>
        /// Confirm graceful failure when negative maxReleases
        /// </summary>
        [Test]
        public void AssertParameters_NegativeValue_release()
        {
            AssertTestData_n_releases(-1, 0, new List<string>());
        }

        /// <summary>
        /// Test graceful failure when null customerModel
        /// </summary>
        [Test]
        public void AssertParameters_null()
        {
            var result = new Retainer().GetReleasesToRetain(1, null);
            Assert.AreEqual(0, result.Count, "Incorrect number of releases retained");
        }

        /// <summary>
        /// Test graceful failure when no Releases
        /// </summary>
        [Test]
        public void AssertParameters_No_Releases()
        {
            CustomerModel.Releases.Clear();
            var result = new Retainer().GetReleasesToRetain(1, CustomerModel);
            Assert.AreEqual(0, result.Count, "Incorrect number of releases retained");
        }

        /// <summary>
        /// Test graceful failure when no deployments
        /// </summary>
        [Test]
        public void AssertParameters_No_Deployments()
        {
            CustomerModel.Deployments.Clear();
            var result = new Retainer().GetReleasesToRetain(1, CustomerModel);
            Assert.AreEqual(0, result.Count, "Incorrect number of releases retained");
        }

        /// <summary>
        /// Test graceful failure when no projects
        /// </summary>
        [Test]
        public void AssertParameters_No_Projects()
        {
            CustomerModel.Projects.Clear();
            var result = new Retainer().GetReleasesToRetain(1, CustomerModel);
            Assert.AreEqual(0, result.Count, "Incorrect number of releases retained");
        }

        /// <summary>
        /// Test graceful failure when no environments
        /// </summary>
        [Test]
        public void AssertParameters_No_Environments()
        {
            CustomerModel.Environments.Clear();
            var result = new Retainer().GetReleasesToRetain(1, CustomerModel);
            Assert.AreEqual(0, result.Count, "Incorrect number of releases retained");
        }
        #endregion

        #region Test for entities not existing
        /// <summary>
        /// Test for poor data integrity - Environment Foreign key record doesn't exist
        /// </summary>
        [Test]
        public void AssertEntity_Environment_Not_Exist()
        {
            CustomerModel.Environments.Remove("Environment-1");
            AssertTestData_n_releases(1, 2, new List<string> { "Release-3", "Release-4"});
        }

        /// <summary>
        /// Test for poor data integrity - Project Foreign key record doesn't exist
        /// </summary>
        [Test]
        public void AssertEntity_Project_Not_Exist()
        {
            CustomerModel.Projects.Remove("Project-1");
            AssertTestData_n_releases(1, 2, new List<string> { "Release-4", "Release-7" });
        }

        /// <summary>
        /// Test for poor data integrity - Release Foreign key record doesn't exist
        /// </summary>
        [Test]
        public void AssertEntity_Release_Not_Exist()
        {
            CustomerModel.Releases.Remove("Release-3");
            AssertTestData_n_releases(1, 3, new List<string> { "Release-2", "Release-4", "Release-7" });
        }
        #endregion

        #region Test for entity data being corrupted
        /// <summary>
        /// Test for invalid data not directly related to the retention rule
        /// </summary>
        [Test]
        public void AssertEntity_Releases_Null_Version()
        {
            CustomerModel.Releases["Release-3"].Version = null;
            AssertTestData_n_releases(1, 3, new List<string> { "Release-3", "Release-4", "Release-7" });
        }

        /// <summary>
        /// Test for invalid data not directly related to the retention rule
        /// </summary>
        [Test]
        public void AssertEntity_Releases_Null_CreateDate()
        {
            CustomerModel.Releases["Release-3"].Created = null;
            AssertTestData_n_releases(1, 3, new List<string> { "Release-3", "Release-4", "Release-7" });
        }

        /// <summary>
        /// Test for invalid data directly related to the retention rule
        /// </summary>
        [Test]
        public void AssertEntity_Releases_Null_ProjectId()
        {
            CustomerModel.Releases["Release-3"].ProjectId = null;
            AssertTestData_n_releases(1, 3, new List<string> { "Release-4", "Release-7", "Release-2" });
        }

        /// <summary>
        /// Test for invalid data not directly related to the retention rule
        /// </summary>
        [Test]
        public void AssertEntity_Project_Null_Name()
        {
            CustomerModel.Projects["Project-1"].Name = null;
            AssertTestData_n_releases(1, 3, new List<string> { "Release-3", "Release-4", "Release-7" });
        }

        /// <summary>
        /// Test for invalid data not directly related to the retention rule
        /// </summary>
        [Test]
        public void AssertEntity_Environment_Null_Name()
        {
            CustomerModel.Environments["Environment-1"].Name = null;
            AssertTestData_n_releases(1, 3, new List<string> { "Release-3", "Release-4", "Release-7" });
        }

        /// <summary>
        /// Test for invalid data directly related to the retention rule
        /// </summary>
        [Test]
        public void AssertEntity_Deployment_Null_ReleaseId()
        {
            CustomerModel.Deployments["Deployment-14"].ReleaseId = null;
            AssertTestData_n_releases(1, 3, new List<string> { "Release-3", "Release-7", "Release-5" });
        }

        /// <summary>
        /// Test for invalid data directly related to the retention rule
        /// </summary>
        [Test]
        public void AssertEntity_Deployment_Null_EnvironmentId()
        {
            CustomerModel.Deployments["Deployment-14"].EnvironmentId = null;
            AssertTestData_n_releases(1, 3, new List<string> { "Release-3", "Release-5", "Release-7" });
        }

        /// <summary>
        /// Test for invalid data directly related to the retention rule
        /// </summary>
        [Test]
        public void AssertEntity_Deployment_Null_DeployedAt()
        {
            CustomerModel.Deployments["Deployment-14"].DeployedAt = null;
            AssertTestData_n_releases(1, 3, new List<string> { "Release-3", "Release-5", "Release-7" });
        }
        #endregion

        #region Test for duplicate releases in results
        /// <summary>
        /// Test for scenario where the two most recent releases are both for the same release. We do not want duplicates
        /// </summary>
        [Test]
        public void Assert_Duplicates()
        {
            // Setup data so that there are two Project/Environment models but only 1 release. We do not want that release to appear twice in the results
            var model = new CustomerModel();
            model.Deployments.Add("D1", new DeploymentModel { Id = "D1", EnvironmentId = "E1", ReleaseId = "R1", DeployedAt = "2000-01-01T12:00:00" });
            model.Deployments.Add("D2", new DeploymentModel { Id = "D2", EnvironmentId = "E2", ReleaseId = "R1", DeployedAt = "2000-01-01T12:00:01" });
            model.Releases.Add("R1", new ReleaseModel { Id = "R1", ProjectId = "P1", Created = "2000-01-01T12:00:00", Version = "1.0.0" });
            model.Projects.Add("P1", new ProjectModel { Id = "P1", Name = "Project-1" });
            model.Environments.Add("E1", new EnvironmentModel { Id = "E1", Name = "Environment-1" });

            AssertTestData_n_releases(1, 1, new List<string> { "R1" }, model);
        }

        [Test]
        public void Assert_Duplicate_Flooding()
        {
            // Setup data so that there are two Project/Environment models but only 1 release. We do not want that release to appear twice in the results
            var model = new CustomerModel();
            model.Deployments.Add("D1", new DeploymentModel { Id = "D1", EnvironmentId = "E1", ReleaseId = "R2", DeployedAt = "2000-01-01T12:00:00" });
            model.Deployments.Add("D2", new DeploymentModel { Id = "D2", EnvironmentId = "E1", ReleaseId = "R1", DeployedAt = "2000-01-02T12:00:01" });
            model.Deployments.Add("D3", new DeploymentModel { Id = "D3", EnvironmentId = "E1", ReleaseId = "R1", DeployedAt = "2000-01-03T12:00:01" });
            model.Deployments.Add("D4", new DeploymentModel { Id = "D4", EnvironmentId = "E1", ReleaseId = "R1", DeployedAt = "2000-01-04T12:00:01" });
            model.Deployments.Add("D5", new DeploymentModel { Id = "D5", EnvironmentId = "E1", ReleaseId = "R1", DeployedAt = "2000-01-05T12:00:01" });
            model.Releases.Add("R1", new ReleaseModel { Id = "R1", ProjectId = "P1", Created = "2000-01-01T12:00:00", Version = "1.0.0" });
            model.Releases.Add("R2", new ReleaseModel { Id = "R2", ProjectId = "P1", Created = "2000-01-02T12:00:00", Version = "1.0.1" });
            model.Projects.Add("P1", new ProjectModel { Id = "P1", Name = "Project-1" });
            model.Environments.Add("E1", new EnvironmentModel { Id = "E1", Name = "Environment-1" });

            AssertTestData_n_releases(2, 2, new List<string> { "R1", "R2" }, model);
        }

        #endregion
    }
}

