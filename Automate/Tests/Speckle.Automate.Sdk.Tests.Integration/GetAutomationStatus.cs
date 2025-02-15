using GraphQL;
using Speckle.Core.Api;

namespace Speckle.Automate.Sdk.Tests.Integration;

public class FunctionRun
{
  public string StatusMessage { get; set; }
}

public class AutomationRun
{
  public string Status { get; set; }
  public IList<FunctionRun> FunctionRuns { get; set; }
}

public class AutomationStatus
{
  public string Status { get; set; }
  public IList<AutomationRun> AutomationRuns { get; set; }
}

public class ModelAutomationStatus
{
  public AutomationStatus AutomationStatus { get; set; }
}

public class ProjectAutomationStatus
{
  public ModelAutomationStatus Model { get; set; }
}

public class AutomationStatusResponseModel
{
  public ProjectAutomationStatus Project { get; set; }
}

public static class AutomationStatusOperations
{
  public static async Task<AutomationStatus> Get(string projectId, string modelId, Client speckleClient)
  {
    GraphQLRequest query =
      new(
        """
        query AutomationRuns(
            $projectId: String!
            $modelId: String!
        )
        {
        project(id: $projectId) {
        model(id: $modelId) {
        automationStatus {
        id
        status
        statusMessage
        automationRuns {
          id
          automationId
          versionId
          createdAt
          updatedAt
          status
          functionRuns {
            id
            functionId
            elapsed
            status
            contextView
            statusMessage
            results
            resultVersions {
              id
            }
          }
        }
        }
        }
        }
        }
        """,
        variables: new { projectId, modelId, }
      );
    var response = await speckleClient.ExecuteGraphQLRequest<AutomationStatusResponseModel>(query);
    return response.Project.Model.AutomationStatus;
  }
}
