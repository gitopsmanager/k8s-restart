using k8s;
using k8s.Models;
using System.Text.Json;

namespace K8sControlApi.Services;

public class K8sService
{
    private readonly IKubernetes _client;

    public K8sService()
    {
        var config = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildConfigFromConfigFile();
        _client = new Kubernetes(config);
    }

    private bool ShouldSkip(V1Deployment dep) =>
        dep.Spec.Selector.MatchLabels != null &&
        dep.Spec.Selector.MatchLabels.TryGetValue("restart", out var val) &&
        val.Equals("ignore", StringComparison.OrdinalIgnoreCase);

    private bool ShouldSkip(V1Pod pod) =>
        pod.Metadata.Labels != null &&
        pod.Metadata.Labels.TryGetValue("restart", out var val) &&
        val.Equals("ignore", StringComparison.OrdinalIgnoreCase);

    public async Task StopNamespace(string ns)
    {
        var deployments = await _client.AppsV1.ListNamespacedDeploymentAsync(ns);
        foreach (var dep in deployments.Items)
        {
            var patch = new V1Patch("{\"spec\":{\"replicas\":0}}", V1Patch.PatchType.StrategicMergePatch);
            await _client.AppsV1.PatchNamespacedDeploymentAsync(patch, dep.Metadata.Name, ns);
        }
    }

    public async Task StartNamespace(string ns)
    {
        var deployments = await _client.AppsV1.ListNamespacedDeploymentAsync(ns);
        foreach (var dep in deployments.Items)
        {
            int replicas = 0;
            if (dep.Metadata.Annotations != null &&
                dep.Metadata.Annotations.TryGetValue("kubectl.kubernetes.io/last-applied-configuration", out var json))
            {
                try
                {
                    var lastApplied = JsonSerializer.Deserialize<V1Deployment>(json);
                    replicas = lastApplied?.Spec?.Replicas;
                }
                catch { }
            }

            var patchJson = $"{{\"spec\":{{\"replicas\":{replicas}}}}}";
            var patch = new V1Patch(patchJson, V1Patch.PatchType.StrategicMergePatch);
            await _client.AppsV1.PatchNamespacedDeploymentAsync(patch, dep.Metadata.Name, ns);
        }
    }

    public async Task RestartNamespace(string ns)
    {
        var pods = await _client.CoreV1.ListNamespacedPodAsync(ns);
        foreach (var pod in pods.Items)
        {
            if (ShouldSkip(pod))
                continue;

            var rsRef = pod.Metadata.OwnerReferences?.FirstOrDefault(o => o.Kind == "ReplicaSet");
            if (rsRef != null)
            {
                try
                {
                    var rs = await _client.AppsV1.ReadNamespacedReplicaSetAsync(rsRef.Name, ns);
                    var depRef = rs.Metadata.OwnerReferences?.FirstOrDefault(o => o.Kind == "Deployment");
                    if (depRef != null)
                    {
                        var dep = await _client.AppsV1.ReadNamespacedDeploymentAsync(depRef.Name, ns);
                        if (ShouldSkip(dep))
                            continue;
                    }
                }
                catch { }
            }

            await _client.CoreV1.DeleteNamespacedPodAsync(pod.Metadata.Name, ns);
        }
    }

    public async Task RestartDeployment(string ns, string deployment)
    {
        var dep = await _client.AppsV1.ReadNamespacedDeploymentAsync(deployment, ns);
        if (ShouldSkip(dep)) return;

        var selector = string.Join(",", dep.Spec.Selector.MatchLabels.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        var pods = await _client.CoreV1.ListNamespacedPodAsync(ns, labelSelector: selector);
        foreach (var pod in pods.Items)
        {
            if (ShouldSkip(pod)) continue;
            await _client.CoreV1.DeleteNamespacedPodAsync(pod.Metadata.Name, ns);
        }
    }

    public async Task StopDeployment(string ns, string deployment)
    {
        var patch = new V1Patch("{\"spec\":{\"replicas\":0}}", V1Patch.PatchType.StrategicMergePatch);
        await _client.AppsV1.PatchNamespacedDeploymentAsync(patch, deployment, ns);
    }

    public async Task StartDeployment(string ns, string deployment)
    {
        var dep = await _client.AppsV1.ReadNamespacedDeploymentAsync(deployment, ns);

        int replicas = 0;
        if (dep.Metadata.Annotations != null &&
            dep.Metadata.Annotations.TryGetValue("kubectl.kubernetes.io/last-applied-configuration", out var json))
        {
            try
            {
                var lastApplied = JsonSerializer.Deserialize<V1Deployment>(json);
                replicas = lastApplied?.Spec?.Replicas;
            }
            catch { }
        }

        var patchJson = $"{{\"spec\":{{\"replicas\":{replicas}}}}}";
        var patch = new V1Patch(patchJson, V1Patch.PatchType.StrategicMergePatch);
        await _client.AppsV1.PatchNamespacedDeploymentAsync(patch, deployment, ns);
    }

    public async Task RestartPod(string ns, string pod)
    {
        var podObj = await _client.CoreV1.ReadNamespacedPodAsync(pod, ns);
        if (ShouldSkip(podObj)) return;

        var rsRef = podObj.Metadata.OwnerReferences?.FirstOrDefault(o => o.Kind == "ReplicaSet");
        if (rsRef != null)
        {
            try
            {
                var rs = await _client.AppsV1.ReadNamespacedReplicaSetAsync(rsRef.Name, ns);
                var depRef = rs.Metadata.OwnerReferences?.FirstOrDefault(o => o.Kind == "Deployment");
                if (depRef != null)
                {
                    var dep = await _client.AppsV1.ReadNamespacedDeploymentAsync(depRef.Name, ns);
                    if (ShouldSkip(dep))
                        return;
                }
            }
            catch { }
        }

        await _client.CoreV1.DeleteNamespacedPodAsync(pod, ns);
    }
}