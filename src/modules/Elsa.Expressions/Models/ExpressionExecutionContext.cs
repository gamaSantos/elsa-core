using Elsa.Expressions.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.Expressions.Models;

/// <summary>
/// Provides context to workflow expressions. 
/// </summary>
public class ExpressionExecutionContext(
    IServiceProvider serviceProvider,
    MemoryRegister memory,
    ExpressionExecutionContext? parentContext = null,
    IDictionary<object, object>? transientProperties = null,
    Action? onChange = null,
    CancellationToken cancellationToken = default)
{
    /// <summary>
    /// A scoped service provider.
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// A shared register of computer memory. 
    /// </summary>
    public MemoryRegister Memory { get; } = memory;

    /// <summary>
    /// A dictionary of transient properties.
    /// </summary>
    public IDictionary<object, object> TransientProperties { get; set; } = transientProperties ?? new Dictionary<object, object>();

    /// <summary>
    /// Provides access to the parent <see cref="ExpressionExecutionContext"/>, if there is any.
    /// </summary>
    public ExpressionExecutionContext? ParentContext { get; set; } = parentContext;

    /// <summary>
    /// A cancellation token.
    /// </summary>
    public CancellationToken CancellationToken { get; } = cancellationToken;

    /// <summary>
    /// Returns the <see cref="MemoryBlock"/> pointed to by the specified memory block reference.
    /// </summary>
    public MemoryBlock GetBlock(Func<MemoryBlockReference> blockReference) => GetBlock(blockReference());

    /// <summary>
    /// Returns the <see cref="MemoryBlock"/> pointed to by the specified memory block reference.
    /// </summary>
    public MemoryBlock GetBlock(MemoryBlockReference blockReference) => GetBlockInternal(blockReference) ?? throw new Exception($"Failed to retrieve memory block with reference {blockReference.Id}");

    /// <summary>
    /// Returns the <see cref="MemoryBlock"/> pointed to by the specified memory block reference.
    /// </summary>
    public bool TryGetBlock(MemoryBlockReference blockReference, out MemoryBlock block)
    {
        var b = GetBlockInternal(blockReference);
        block = b ?? null!;
        return b != null;
    }

    /// <summary>
    /// Returns the value of the memory block pointed to by the specified memory block reference.
    /// </summary>
    public object? Get(Func<MemoryBlockReference> blockReference) => Get(blockReference());

    /// <summary>
    /// Returns the value of the memory block pointed to by the specified memory block reference.
    /// </summary>
    public object? Get(MemoryBlockReference blockReference) => GetBlock(blockReference).Value;

    /// <summary>
    /// Returns the value of the memory block pointed to by the specified memory block reference.
    /// </summary>
    public bool TryGet(MemoryBlockReference blockReference, out object? value)
    {
        if (TryGetBlock(blockReference, out var block))
        {
            value = block.Value;
            return true;
        }

        value = null;
        return false;
    }

    /// <summary>
    /// Returns the value of the memory block pointed to by the specified memory block reference. 
    /// </summary>
    public T? Get<T>(Func<MemoryBlockReference> blockReference) => Get<T>(blockReference());

    /// <summary>
    /// Returns the value of the memory block pointed to by the specified memory block reference.
    /// </summary>
    public T? Get<T>(MemoryBlockReference blockReference) => Get(blockReference).ConvertTo<T>();

    /// <summary>
    /// Sets the value of the memory block pointed to by the specified memory block reference.
    /// </summary>
    public void Set(Func<MemoryBlockReference> blockReference, object? value, Action<MemoryBlock>? configure = null) => Set(blockReference(), value, configure);

    /// <summary>
    /// Sets the value of the memory block pointed to by the specified memory block reference.
    /// </summary>
    public void Set(MemoryBlockReference blockReference, object? value, Action<MemoryBlock>? configure = null)
    {
        var block = GetBlockInternal(blockReference) ?? Memory.Declare(blockReference);
        block.Value = value;
        configure?.Invoke(block);
        onChange?.Invoke();
    }

    /// <summary>
    /// Returns the service of the specified type.
    /// </summary>
    public T GetRequiredService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    private MemoryBlock? GetBlockInternal(MemoryBlockReference blockReference)
    {
        if (blockReference.Id == null!)
            return null;
        
        var currentContext = this;

        while (currentContext != null)
        {
            var register = currentContext.Memory;

            if (register.TryGetBlock(blockReference.Id, out var block))
                return block;

            currentContext = currentContext.ParentContext;
        }

        return null;
    }
}