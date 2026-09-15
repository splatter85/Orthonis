using System.Collections.Immutable;
using System.Text;
using System.Text.Json.Nodes;
using Orthonis.Core;
using Orthonis.Modules;
using Orthonis.Cli;
using App = Orthonis.Cli.Program;

namespace Orthonis.Tests;

public static partial class Program
{
    private static async Task<CaseSnapshot> TwoModules(string scenario = "missing") =>
        await App.CreateEngine("synthetic:" + scenario).CollectAsync(CaseSnapshot.Create("synthetic:" + scenario),
            [new("startup.inventory", 1, null), new("reliability.summary", 1, null)]);
    private static byte[] Edit(DiscoveryPlan plan, Action<JsonObject> change)
    {
        var node = JsonNode.Parse(JsonCodec.Encode(plan))!.AsObject(); change(node);
        return Encoding.UTF8.GetBytes(node.ToJsonString());
    }

    private static async Task PlanChecks()
    {
        await Test("second module shares the core without changing Startup", async () =>
        {
            var s = await TwoModules(); Equal(3, s.Evidence.Length); Equal(4, App.CreateEngine(s.SourceLabel).Capabilities.Length);
            True(s.Evidence.Any(e => e.ModuleId == "startup")); True(s.Evidence.Any(e => e.ModuleId == "reliability"));
            True(s.Findings.All(f => f.EvidenceIds.All(id => s.Evidence.Any(e => e.Id == id))));
        });
        await Test("approved two-module plan produces follow-up evidence", async () =>
        {
            var s = await TwoModules(); var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel)); var plan = plans.Example(s);
            Equal(2, plan.Requests.Length); var next = await plans.ExecuteAsync(s, JsonCodec.Encode(plan), true);
            Equal(2, next.Revision); Equal(5, next.Evidence.Length); Equal(1, next.AppliedPlanIds.Length);
            True(next.Evidence.Any(e => e.CapabilityId == "startup.inspect")); True(next.Evidence.Any(e => e.CapabilityId == "reliability.inspect"));
            Equal(3, s.Evidence.Length); Equal(0, s.AppliedPlanIds.Length);
        });
        await Test("preview performs zero collector calls", async () =>
        {
            var s = await Initial(); var count = new CountingModule(); var plans = new DiscoveryPlans(new Investigation([count]));
            var plan = new DiscoveryPlan(1, "discovery", "plan-preview", s.CaseId, s.Revision, JsonCodec.Hash(s), "Inspect again",
                [s.Evidence[0].Id], [new("count.inventory", 1, null)]);
            plans.Preview(s, JsonCodec.Encode(plan)); Equal(0, count.Calls);
            await ThrowsAsync<RefusalException>(() => plans.ExecuteAsync(s, JsonCodec.Encode(plan), false)); Equal(0, count.Calls);
        });
        await Test("invalid later plan operation causes zero calls", async () =>
        {
            var s = await Initial(); var count = new CountingModule(); var plans = new DiscoveryPlans(new Investigation([count]));
            var plan = new DiscoveryPlan(1, "discovery", "plan-invalid", s.CaseId, s.Revision, JsonCodec.Hash(s), "Inspect again",
                [s.Evidence[0].Id], [new("count.inventory", 1, null), new("registry.delete", 1, null)]);
            await ThrowsAsync<RefusalException>(() => plans.ExecuteAsync(s, JsonCodec.Encode(plan), true)); Equal(0, count.Calls);
        });
        foreach (var mutation in new (string Name, Action<JsonObject> Change)[]
        {
            ("wrong case", n => n["caseId"] = "case-other"),
            ("stale revision", n => n["basedOnRevision"] = 0),
            ("wrong digest", n => n["basedOnSnapshotHash"] = new string('0', 64)),
            ("repair kind", n => n["kind"] = "repair"),
            ("future schema", n => n["schemaVersion"] = 2),
            ("extra authority field", n => n["approved"] = true),
            ("missing field", n => n.Remove("kind")),
            ("null hypothesis", n => n["hypothesis"] = null),
            ("empty requests", n => n["requests"] = new JsonArray()),
            ("null request entry", n => n["requests"] = new JsonArray((JsonNode?)null)),
            ("invented evidence", n => n["evidenceRefs"] = new JsonArray("ev-invented")),
            ("empty evidence", n => n["evidenceRefs"] = new JsonArray()),
            ("wrong-module target", n => n["requests"] = JsonNode.Parse("[{\"capabilityId\":\"startup.inspect\",\"capabilityVersion\":1,\"targetId\":\"application-001\"}]")),
            ("nested command field", n => n["requests"]![0]!["command"] = "do not execute"),
            ("too many requests", n => n["requests"] = new JsonArray(Enumerable.Range(0, 9).Select(_ => n["requests"]![0]!.DeepClone()).ToArray()))
        })
        {
            await Test("plan rejects " + mutation.Name, async () =>
            {
                var s = await TwoModules(); var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel));
                var bytes = Edit(plans.Example(s), mutation.Change);
                Throws<RefusalException>(() => plans.Preview(s, bytes)); Equal(1, s.Revision);
            });
        }
        await Test("plan replay refused even when rebound to current digest", async () =>
        {
            var s = await TwoModules(); var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel)); var plan = plans.Example(s);
            var next = await plans.ExecuteAsync(s, JsonCodec.Encode(plan), true);
            var replay = plan with { BasedOnRevision = next.Revision, BasedOnSnapshotHash = JsonCodec.Hash(next) };
            Throws<RefusalException>(() => plans.Preview(next, JsonCodec.Encode(replay)));
        });
        await Test("same revision with changed evidence invalidates old plan", async () =>
        {
            var s = await TwoModules(); var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel)); var plan = plans.Example(s);
            var changed = s with { SourceLabel = "synthetic:healthy" };
            Throws<RefusalException>(() => plans.Preview(changed, JsonCodec.Encode(plan)));
        });
        await Test("nested duplicate JSON key rejected", async () =>
        {
            var s = await TwoModules(); var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel));
            var json = Encoding.UTF8.GetString(JsonCodec.Encode(plans.Example(s))).Replace("\"capabilityVersion\":1", "\"capabilityVersion\":1,\"capabilityVersion\":1", StringComparison.Ordinal);
            Throws<RefusalException>(() => plans.Preview(s, Encoding.UTF8.GetBytes(json)));
        });
        await Test("truncated and invalid UTF8 plans rejected", async () =>
        {
            var s = await TwoModules(); var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel)); var bytes = JsonCodec.Encode(plans.Example(s));
            Throws<RefusalException>(() => plans.Preview(s, bytes[..^1]));
            Throws<RefusalException>(() => plans.Preview(s, [0xff, 0xfe, 0xfd]));
        });
        await Test("excessive nesting rejected", () => Sync(() =>
            Throws<RefusalException>(() => JsonCodec.Decode<DiscoveryPlan>(Encoding.UTF8.GetBytes(new string('[', 20) + "0" + new string(']', 20))))));
        await Test("failure outcome is retained without leaking exception detail", async () =>
        {
            var count = new CountingModule(); var next = await new Investigation([count]).CollectAsync(CaseSnapshot.Create("synthetic:healthy"), [new("count.inventory", 1, null)]);
            Equal(CollectionStatus.Failed, next.Evidence[0].Status); Equal(0, next.Findings.Length);
            True(!Encoding.UTF8.GetString(JsonCodec.Encode(next)).Contains("Test collector failure", StringComparison.Ordinal));
        });
        await Test("healthy two-module case makes no attention diagnosis", async () =>
        {
            var s = await TwoModules("healthy"); var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel));
            var next = await plans.ExecuteAsync(s, JsonCodec.Encode(plans.Example(s)), true); Equal(0, next.Findings.Length);
        });
        await Test("denied modules can be retried without inventing targets", async () =>
        {
            var s = await TwoModules("denied"); var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel)); var plan = plans.Example(s);
            True(plan.Requests.All(r => r.TargetId is null));
            var next = await plans.ExecuteAsync(s, JsonCodec.Encode(plan), true);
            True(next.Evidence.All(e => e.Status == CollectionStatus.PermissionDenied)); Equal(0, next.Findings.Length);
        });
        await Test("injected write failure preserves prior case", async () =>
        {
            var root = Path.Combine(Path.GetTempPath(), "orthonis-fault-" + Guid.NewGuid().ToString("N"));
            try
            {
                var s = await TwoModules(); using var store = new CaseStore(root, create: true); store.Save(s, null);
                var before = File.ReadAllBytes(Path.Combine(root, "case.json"));
                var plans = new DiscoveryPlans(App.CreateEngine(s.SourceLabel)); var next = await plans.ExecuteAsync(s, JsonCodec.Encode(plans.Example(s)), true);
                Throws<IOException>(() => store.Save(next, s.Revision, () => throw new IOException("injected before replacement")));
                True(before.SequenceEqual(File.ReadAllBytes(Path.Combine(root, "case.json")))); Equal(1, store.Load().Revision);
                Equal(0, Directory.GetFiles(root, "*.tmp").Length);
            }
            finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
        });
        await Test("report includes a valid concrete return example", async () =>
        {
            var s = await TwoModules(); var engine = App.CreateEngine(s.SourceLabel); var plans = new DiscoveryPlans(engine); var example = plans.Example(s);
            var report = Report.Render(s, engine.Capabilities, example);
            True(report.Contains(example.PlanId, StringComparison.Ordinal)); True(report.Contains("not a signature or permission", StringComparison.Ordinal));
            plans.Preview(s, JsonCodec.Encode(example));
        });
    }
}
