using System.Collections.Generic;

namespace DevOpsDeploy
{
    public class CustomerModel
    {
        public ReleaseModels Releases { get; } = new ReleaseModels();

        public ProjectModels Projects { get; } = new ProjectModels();

        public EnvironmentModels Environments { get; } = new EnvironmentModels();

        public DeploymentModels Deployments { get; } = new DeploymentModels();
    }

    public class ReleaseModels : BaseModelCollection<ReleaseModel> { }
    public class ProjectModels : BaseModelCollection<ProjectModel> { }
    public class EnvironmentModels : BaseModelCollection<EnvironmentModel> { }
    public class DeploymentModels : BaseModelCollection<DeploymentModel> { }
    

    public class DeploymentModel : IBaseModel
    {
        public string Id { get; set; }
        public string ReleaseId { get; set; }
        public string EnvironmentId { get; set; }
        public string DeployedAt { get; set; }
    }

    public class EnvironmentModel : IBaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    
    public class ProjectModel : IBaseModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    
    public class ReleaseModel : IBaseModel
    {
        public string Id { get; set; }
        public string ProjectId { get; set; }
        public string Version { get; set; }
        public string Created { get; set; }
    }

    public class BaseModelCollection<T> : Dictionary<string, T> where T : IBaseModel
    {
        /// <summary>
        /// Adds key / value pairs to collection where key is derived from the Id property
        /// </summary>
        /// <param name="items"></param>
        /// <exception cref="System.ArgumentException">Thrown when an item with the same Id property already exists in the collection.</exception>
        public void AddItems(List<T> items)
        {
            foreach (var item in items)
                this.Add(item.Id, item);
        }
    }

    public interface IBaseModel
    {
        string Id { get; set; }
    }

}
