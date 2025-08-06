using Microsoft.AspNetCore.Mvc;
using K8sControlApi.Services;

namespace K8sControlApi.Controllers;

[ApiController]
[Route("namespace/{namespace}")]
public class K8sController : ControllerBase
{
    private readonly K8sService _svc;

    public K8sController(K8sService svc)
    {
        _svc = svc;
    }

    [HttpPost("restart")]
    public async Task<IActionResult> RestartNamespace(string @namespace)
    {
        await _svc.RestartNamespace(@namespace);
        return Ok($"Restarted pods in namespace '{@namespace}'");
    }

    [HttpPost("stop")]
    public async Task<IActionResult> StopNamespace(string @namespace)
    {
        await _svc.StopNamespace(@namespace);
        return Ok($"Scaled all deployments in namespace '{@namespace}' to 0");
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartNamespace(string @namespace)
    {
        await _svc.StartNamespace(@namespace);
        return Ok($"Scaled all deployments in namespace '{@namespace}' to last known replica count");
    }

    [HttpPost("deployment/{deployment}/restart")]
    public async Task<IActionResult> RestartDeployment(string @namespace, string deployment)
    {
        await _svc.RestartDeployment(@namespace, deployment);
        return Ok($"Restarted pods in deployment '{deployment}'");
    }

    [HttpPost("deployment/{deployment}/stop")]
    public async Task<IActionResult> StopDeployment(string @namespace, string deployment)
    {
        await _svc.StopDeployment(@namespace, deployment);
        return Ok($"Scaled deployment '{deployment}' to 0");
    }

    [HttpPost("deployment/{deployment}/start")]
    public async Task<IActionResult> StartDeployment(string @namespace, string deployment)
    {
        await _svc.StartDeployment(@namespace, deployment);
        return Ok($"Started deployment '{deployment}'");
    }

    [HttpPost("pod/{pod}/restart")]
    public async Task<IActionResult> RestartPod(string @namespace, string pod)
    {
        await _svc.RestartPod(@namespace, pod);
        return Ok($"Restarted pod '{pod}'");
    }
}