using System.Collections.Generic;
using System.Linq;

namespace DevOpsDeploy
{
    public class Retainer
    {
        Log _log = new Log();

        /// <summary>
        /// Generate a list of the top 'n' most recently deployed releases for a customer.
        /// 
        /// Where 'n' is <paramref name="maxResults"/>
        /// 
        /// Retention logic applied:
        ///   For each Project/Environment combination, keep the 'n' most recent deployed release(s)
        /// 
        /// </summary>
        /// <param name="maxResults">Number of releases to return</param>
        /// <param name="customerModel">Customers deployment data</param>
        /// <returns>Top 'n' most recently deployed Release Ids</returns>
        public List<string> GetReleasesToRetain(int maxResults, CustomerModel customerModel)
        {
            // Validate parameters to ensure we can process the data
            if (!hasValidParameters(maxResults, customerModel))
                return new List<string>();

            // Transform the data so that we have a collection of deployments grouped by Project/Environment
            var projectEnvironmentDeployments = DeriveProjectEnvironmentDeployments(customerModel);
          
            // Apply retention rule over transformed data to determine retained releases
            return CalculateReleasesToRetain(customerModel, maxResults, projectEnvironmentDeployments);
        }

        #region Private routines

        /// <summary>
        /// Validate the parameters supplied to the GetReleasesToRetain routine
        /// </summary>
        /// <returns>True if parameters are valid and processing can continue. False if processing should be halted</returns>
        private bool hasValidParameters(int maxResults, CustomerModel customerModel)
        {
            if (maxResults <= 0)
            {
                _log.WriteError($"Invalid maxResults parameter value '{maxResults}'.");
                return false;
            }

            if (customerModel is null)
            {
                _log.WriteError($"Invalid customerModel parameter value 'null'");
                return false;
            }

            if (customerModel.Releases is null || customerModel.Releases.Count == 0)
            {
                _log.WriteError($"Invalid customerModel.Releases parameter value.");
                return false;
            }

            if (customerModel.Deployments is null || customerModel.Deployments.Count == 0)
            {
                _log.WriteError($"Invalid customerModel.Deployments parameter value.");
                return false;
            }

            if (customerModel.Environments is null || customerModel.Environments.Count == 0)
            {
                _log.WriteError($"Invalid customerModel.Environments parameter value.");
                return false;
            }

            if (customerModel.Projects is null || customerModel.Projects.Count == 0)
            {
                _log.WriteError($"Invalid customerModel.Projects parameter value.");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Iterate through the Customer Model to build collections of deployments grouped by Project/Environment.
        /// </summary>
        private ProjectEnvironments DeriveProjectEnvironmentDeployments(CustomerModel customerModel)
        {
            var projectEnvironments = new ProjectEnvironments();

            // Iterate through deployments to build Project/Environment object containing all deployments made to them
            foreach (var item in customerModel.Deployments)
            {
                var deployment = item.Value;

                // Get the release in order to get the Project Id
                if (string.IsNullOrWhiteSpace(deployment.ReleaseId) || !customerModel.Releases.TryGetValue(deployment.ReleaseId, out var release))
                {
                    _log.WriteError($"Release Id '{deployment.ReleaseId}' from Deployment '{deployment.Id}' does not exist. Release will not be retained.");
                    continue;
                }

                // Check the Project exists

                if (string.IsNullOrWhiteSpace(release.ProjectId) || !customerModel.Projects.TryGetValue(release.ProjectId, out var project))
                {
                    _log.WriteError($"Project Id '{release.ProjectId}' does not exist. Release '{release.Id}' from Deployment '{deployment.Id}' will not be retained.");
                    continue;
                }

                // Check the Environment is valid
                if (string.IsNullOrWhiteSpace(deployment.EnvironmentId) || !customerModel.Environments.TryGetValue(deployment.EnvironmentId, out var environment))
                {
                    _log.WriteError($"Environment Id '{deployment.EnvironmentId}' does not exist. Release '{release.Id}' from Deployment '{deployment.Id}' will not be retained.");
                    continue;
                }

                // If the Project/Environment combination doesn't exist, create it
                projectEnvironments.TryGetValue(release.ProjectId, deployment.EnvironmentId, out var projectEnvironment);
                if (projectEnvironment is null)
                {
                    projectEnvironment = new ProjectEnvironment(release.ProjectId, deployment.EnvironmentId);
                    projectEnvironments.Add(projectEnvironment);
                }

                // Add the deployment to the Project/Environment combination
                projectEnvironment.Deployments.Add(deployment);
            }

            return projectEnvironments;
        }

        /// <summary>
        /// Iterates through deployments grouped by Project/Environment (<paramref name="projectEnvironmentDeployments"/>) returning the <paramref name="maxResults"/> most recent releases for each Project/Environment combination
        /// </summary>
        /// <returns>List of Release Ids to retain.</returns>
        private List<string> CalculateReleasesToRetain(CustomerModel customerModel, int maxResults, ProjectEnvironments projectEnvironmentDeployments)
        {
            // Create our object to return
            var releasesToReturn = new List<string>();

            // Iterate through the Project/Environment combinations to look at their top <maxResults> deployments and add their Release Ids to the retainedReleases variable
            foreach (var item in projectEnvironmentDeployments)
            {
                var deployments = item.Value;
                var retainedReleases = new List<string>(); // list of releases we want to retain for this Project/Environment combination

                // loop through the top <maxResults> where deployments are ordered by DeployedAt. 
                foreach (var deployment in deployments.Deployments.OrderByDescending(x => x.DeployedAt))
                {
                    var release = customerModel.Releases[deployment.ReleaseId];

                    // Log when this release was most recently deployed for this Project/Environment
                    _log.WriteLine($"Retaining Release '{deployment.ReleaseId}'. Version '{release.Version}'. Most recently deployed for Project/Environment '{deployments.ProjectId}/{deployments.EnvironmentId}' at '{deployment.DeployedAt}'. ");

                    // Only add the release if it isn't already in the list to avoid duplicates
                    if (!retainedReleases.Contains(deployment.ReleaseId))
                        retainedReleases.Add(deployment.ReleaseId);

                    // Check if we have reached the maximum number of releases to keep for this Project/Environment
                    if (retainedReleases.Count >= maxResults)
                        break;
                }

                // Add the retained releases to the releases to return object
                releasesToReturn.AddRange(retainedReleases);
            }

            // Clean the releasesToReturn by removing duplicates.
            return releasesToReturn.Distinct().ToList<string>();
        }

        #region Local ProjectenvironmentDeployments classes
        private class ProjectEnvironments : Dictionary<string, ProjectEnvironment>
        {
            internal void Add(ProjectEnvironment item)
            {
                this.Add(item.Key, item);
            }

            internal bool ContainsKeyFor(string projectId, string environmentId)
            {
                var key = ProjectEnvironment.GenerateKey(projectId, environmentId);
                return this.ContainsKey(key);
            }

            internal bool TryGetValue(string projectId, string environmentId, out ProjectEnvironment value)
            {
                var key = ProjectEnvironment.GenerateKey(projectId, environmentId);
                return this.TryGetValue(key, out value);
            }
        }

        private class ProjectEnvironment
        {
            internal ProjectEnvironment(string projectId, string environmentId)
            {
                ProjectId = projectId;
                EnvironmentId = environmentId;
            }

            internal string Key { get { return GenerateKey(ProjectId, EnvironmentId); } }

            internal string ProjectId { get; set; }
            internal string EnvironmentId { get; set; }

            /// <summary>
            /// Deployments sorted by deployment time which is stored in UTC
            /// </summary>
            internal List<DeploymentModel> Deployments = new List<DeploymentModel>();


            internal static string GenerateKey(string projectId, string environmentId)
            {
                return $"{projectId}.{environmentId}";
            }
        }
        #endregion

        #endregion
    }
}
