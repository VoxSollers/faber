namespace Faber.Bootstrap.Workflow;

public interface IBootstrapWorkflow
{
    Task RunAsync(CancellationToken cancellationToken);
}
