

namespace JaegerWebAPI.Services;

public class TracerService : ITracer
{
    private readonly ITracer tracer;

    public TracerService()
    {
        var sampler = new ConstSampler(true);

        // 创建并返回 Jaeger.Tracer 类的实例
        tracer = new Tracer.Builder("my-service-name")
            .WithSampler(sampler)
            .Build();
    }

    public IScopeManager ScopeManager => tracer.ScopeManager;

    public ISpan ActiveSpan => tracer.ActiveSpan;

    public ISpanBuilder BuildSpan(string operationName)
    {
        return tracer.BuildSpan(operationName);
    }

    public ISpanContext Extract<TCarrier>(IFormat<TCarrier> format, TCarrier carrier)
    {
        return tracer.Extract(format, carrier);
    }

    public void Inject<TCarrier>(ISpanContext spanContext, IFormat<TCarrier> format, TCarrier carrier)
    {
        tracer.Inject(spanContext, format, carrier);
    }
}