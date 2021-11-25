# cephalopod-deploy
Implementation of DevOps Deploy's - Release Retention rule, which determines which releases to keep given a set of Projects, Environments, Releases, Deployments and how many releases to keep.

Assumptions:
* Structure of Json sample files are based on a schema and we can assume compatibility is maintained to support direct deserialisation
* Functionality will be called as part of an internal library and does not require web services
* Non-deployed releases are not to be retained
* DeployedAt times are stored in UTC and therefore can be sorted alphanumerically
* Not possible to have 2 different releases with the same DeployedAt time for a Project/Environment combination
* Deployments that do not have foreign key data integrity should not have their releases included (eg. Project, Environment or Release does not exist)
* DevOps Deploy already have a logging component which can be leveraged. Using Console as a placeholder for logging to save reinventing the wheel writing a logger.
* Version number is irrelevant to the validity of a release and the scope of deciding if it is retained. Documentation states '*may* be version 1.0.0' which I assume to mean it is not a guaranteed format
* Id field on each entity is unique
