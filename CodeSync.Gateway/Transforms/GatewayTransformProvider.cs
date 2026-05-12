using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace CodeSync.Gateway.Transforms;

/// <summary>
/// Applied to every proxied request via YARP's transform pipeline.
/// Injects standard forwarding headers so downstream services know
/// the real client IP, original scheme, and that requests arrived
/// through the CodeSync gateway.
/// </summary>
public sealed class GatewayTransformProvider : ITransformProvider
{
    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // No custom validation needed — built-in YARP validation is sufficient.
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        // No custom validation needed.
    }

    public void Apply(TransformBuilderContext context)
    {
        // ── 1. Preserve the original Host header ──────────────────────────
        context.AddOriginalHost(useOriginal: false);

        // ── 2. Add X-Forwarded-* headers ─────────────────────────────────
        context.AddXForwardedFor(action: ForwardedTransformActions.Append);
        context.AddXForwardedHost(action: ForwardedTransformActions.Append);
        context.AddXForwardedProto(action: ForwardedTransformActions.Append);
        context.AddXForwardedPrefix(action: ForwardedTransformActions.Append);

        // ── 3. Stamp a custom gateway header on every proxied request ─────
        context.AddRequestHeader("X-Gateway",        "CodeSync-YARP",  append: false);
        context.AddRequestHeader("X-Gateway-Version","1.0",            append: false);

        // ── 4. Remove sensitive internal headers from client requests ──────
        context.AddRequestHeaderRemove("X-Internal-Secret");
        context.AddRequestHeaderRemove("X-Admin-Token");

        // ── 5. Add CORS header to every proxied response ──────────────────
        context.AddResponseHeader(
            "X-Powered-By", "CodeSync-Gateway",
            append: false);
    }
}