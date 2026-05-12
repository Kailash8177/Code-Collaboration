using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Model;

namespace CodeSync.Gateway.Controllers;

/// <summary>
/// Exposes YARP's live runtime configuration — useful for
/// debugging which routes and clusters are currently loaded.
/// </summary>
[ApiController]
[Route("diagnostics")]
public sealed class ProxyDiagnosticsController : ControllerBase
{
    private readonly IProxyStateLookup _proxyState;

    public ProxyDiagnosticsController(IProxyStateLookup proxyState)
    {
        _proxyState = proxyState;
    }

    // ── GET /diagnostics/routes ───────────────────────────────────────────
    /// <summary>Lists all YARP routes currently loaded in memory.</summary>
    [HttpGet("routes")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetRoutes()
    {
        var routes = _proxyState.GetRoutes().Select(r => new
        {
            routeId   = r.Config.RouteId,
            clusterId = r.Config.ClusterId,
            path      = r.Config.Match.Path,
            authPolicy= r.Config.AuthorizationPolicy,
            metadata  = r.Config.Metadata
        });

        return Ok(new { loadedRoutes = routes });
    }

    // ── GET /diagnostics/clusters ─────────────────────────────────────────
    /// <summary>Lists all YARP clusters and their destination health.</summary>
    [HttpGet("clusters")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetClusters()
    {
        var clusters = _proxyState.GetClusters().Select(c => new
        {
            clusterId           = c.Model.Config.ClusterId,
            loadBalancingPolicy = c.Model.Config.LoadBalancingPolicy,
            destinations        = c.DestinationsState.AllDestinations.Select(d => new
            {
                destinationId = d.DestinationId,
                address       = d.Model.Config.Address,
                health        = d.Health.Passive.ToString()
            })
        });

        return Ok(new { loadedClusters = clusters });
    }
}