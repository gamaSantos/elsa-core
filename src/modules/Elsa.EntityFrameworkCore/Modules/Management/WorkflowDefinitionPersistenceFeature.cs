using Elsa.EntityFrameworkCore.Common;
using Elsa.EntityFrameworkCore.Common.Contracts;
using Elsa.EntityFrameworkCore.Handlers;
using Elsa.Features.Attributes;
using Elsa.Features.Services;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.EntityFrameworkCore.Modules.Management;

/// <summary>
/// Configures the <see cref="WorkflowDefinitionsFeature"/> feature with an Entity Framework Core persistence provider.
/// </summary>
[DependsOn(typeof(WorkflowManagementFeature))]
public class EFCoreWorkflowDefinitionPersistenceFeature(IModule module) : PersistenceFeatureBase<EFCoreWorkflowDefinitionPersistenceFeature, ManagementElsaDbContext>(module)
{
    /// Delegate for determining the exception handler.
    public Func<IServiceProvider, IDbExceptionHandler<ManagementElsaDbContext>> DbExceptionHandler { get; set; } = _ => new RethrowDbExceptionHandler(); 

    /// <inheritdoc />
    public override void Configure()
    {
        Module.Configure<WorkflowDefinitionsFeature>(feature =>
        {
            feature.WorkflowDefinitionStore = sp => sp.GetRequiredService<EFCoreWorkflowDefinitionStore>();
        });
    }

    /// <inheritdoc />
    public override void Apply()
    {
        base.Apply();

        Services.AddScoped(DbExceptionHandler);

        AddEntityStore<WorkflowDefinition, EFCoreWorkflowDefinitionStore>();
    }
}