using Microsoft.AspNetCore.Mvc;

namespace K8sControlApi.Controllers;

[ApiController]
[Route("namespace/{namespace}")]
public class K8sController : ControllerBase
{
    [HttpPost("restart")]
    public IActionResult RestartNamespace(string @namespace) => Ok($"Restarted all pods in namespace '{@namespace}'");

    [HttpPost("stop")]
    public IActionResult StopNamespace(string @namespace) => Ok($"Stopped all deployments in namespace '{@namespace}'");

    [HttpPost("start")]
    public IActionResult StartNamespace(string @namespace) => Ok($"Started all deployments in namespace '{@namespace}'");

    [HttpPost("deployment/{deployment}/restart")]
    public IActionResult RestartDeployment(string @namespace, string deployment) =>
        Ok($"Restarted pods in deployment '{deployment}' of namespace '{@namespace}'");

    [HttpPost("deployment/{deployment}/stop")]
    public IActionResult StopDeployment(string @namespace, string deployment) =>
        Ok($"Stopped deployment '{deployment}' in namespace '{@namespace}'");

    [HttpPost("deployment/{deployment}/start")]
    public IActionResult StartDeployment(string @namespace, string deployment) =>
        Ok($"Started deployment '{deployment}' in namespace '{@namespace}'");

    [HttpPost("pod/{pod}/restart")]
    public IActionResult RestartPod(string @namespace, string pod) =>
        Ok($"Restarted pod '{pod}' in namespace '{@namespace}'");
}
