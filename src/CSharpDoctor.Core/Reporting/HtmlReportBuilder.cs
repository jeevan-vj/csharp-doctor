using System.Net;
using System.Text;
using CSharpDoctor.Core.Configuration;
using CSharpDoctor.Core.Diagnostics;
using CSharpDoctor.Core.Inspection;

namespace CSharpDoctor.Core.Reporting;

/// <summary>Builds a self-contained HTML dashboard from inspection results.</summary>
public static class HtmlReportBuilder
{
    public static string Build(InspectResult result, InspectMode mode, CSharpDoctorConfig? config = null)
    {
        var jsonPayload = JsonReportBuilder.Build(result, mode);
        var recommendations = BuildRecommendations(result, config);
        var builder = new StringBuilder();

        builder.AppendLine("<!DOCTYPE html>");
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("<meta charset=\"utf-8\" />");
        builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        builder.AppendLine("<title>CSharp Doctor Report</title>");
        builder.AppendLine("<style>");
        builder.AppendLine(HtmlReportAssets.Css);
        builder.AppendLine("</style>");
        builder.AppendLine("</head>");
        builder.AppendLine("<body>");
        builder.AppendLine("<div class=\"layout\">");
        AppendNav(builder);
        builder.AppendLine("<main>");

        AppendSummary(builder, result, mode);
        AppendQualityGates(builder, result.QualityGates);
        AppendHotSpots(builder, result.HotSpots);
        AppendDomainSections(builder, result.Diagnostics);
        AppendFindings(builder, result.Diagnostics);
        AppendRecommendations(builder, recommendations);
        AppendRawJson(builder, jsonPayload);

        builder.AppendLine("</main>");
        builder.AppendLine("</div>");
        builder.AppendLine("<script type=\"application/json\" id=\"report-data\">");
        builder.AppendLine(jsonPayload);
        builder.AppendLine("</script>");
        builder.AppendLine("<script>");
        builder.AppendLine(HtmlReportAssets.JavaScript);
        builder.AppendLine("</script>");
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");

        return builder.ToString();
    }

    private static HtmlRecommendationGroups BuildRecommendations(InspectResult result, CSharpDoctorConfig? config)
    {
        var checks = new HtmlReportChecksRan(
            Lint: result.InspectorRuns?.ContainsKey("lint") == true,
            Wiring: result.InspectorRuns?.ContainsKey("wiring") == true,
            Ef: result.InspectorRuns?.ContainsKey("ef") == true,
            Architecture: result.InspectorRuns?.ContainsKey("architecture") == true,
            Sca: result.InspectorRuns?.ContainsKey("sca") == true);

        var hints = new HtmlReportConfigHints(
            QualityGatesDisabled: config?.QualityGates.Enabled != true,
            NoBaseline: string.IsNullOrWhiteSpace(config?.QualityGates.BaselinePath),
            NoArchitecturePolicy: config?.ArchitecturePolicy.ForbiddenAssemblyReferences.Count == 0
                && config?.ArchitecturePolicy.ForbiddenNamespaceDependencies.Count == 0);

        return HtmlReportViewModel.BuildRecommendationGroups(
            result.Diagnostics,
            result.Metrics ?? ScanMetricsCalculator.Compute(result.Diagnostics, new DebtWeightsConfig()),
            result.QualityGates,
            checks,
            hints);
    }

    private static void AppendNav(StringBuilder builder)
    {
        builder.AppendLine("<nav>");
        builder.AppendLine("<h1>CSharp Doctor</h1>");
        builder.AppendLine("<a href=\"#summary\">Summary</a>");
        builder.AppendLine("<a href=\"#quality-gates\">Quality gates</a>");
        builder.AppendLine("<a href=\"#hot-spots\">Hot spots</a>");
        builder.AppendLine("<a href=\"#domains\">Domains</a>");
        builder.AppendLine("<a href=\"#findings\">Findings</a>");
        builder.AppendLine("<a href=\"#recommendations\">Recommendations</a>");
        builder.AppendLine("<a href=\"#raw-json\">Raw JSON</a>");
        builder.AppendLine("</nav>");
    }

    private static void AppendSummary(StringBuilder builder, InspectResult result, InspectMode mode)
    {
        var statusClass = result.ExitCode == 0 ? "pass" : "fail";
        var statusText = result.ExitCode == 0 ? "Passed" : "Failed";
        var debt = result.Metrics?.DebtScore ?? 0;
        var nextAction = result.ExitCode == 0
            ? "Review top hot spots for preventive cleanup."
            : result.QualityGates is { Enabled: true, Passed: false }
                ? "Fix failed quality gates first."
                : "Resolve errors and high-priority warnings.";

        builder.AppendLine("<section id=\"summary\" class=\"hero\">");
        builder.AppendLine($"<div class=\"status {statusClass}\">{Encode(statusText)}</div>");
        builder.AppendLine($"<p class=\"muted\">{Encode(nextAction)}</p>");
        builder.AppendLine("<div class=\"metrics\">");
        builder.AppendLine($"<span class=\"metric\"><strong>{result.ErrorCount}</strong> errors</span>");
        builder.AppendLine($"<span class=\"metric\"><strong>{result.WarningCount}</strong> warnings</span>");
        builder.AppendLine($"<span class=\"metric\"><strong>{result.Diagnostics.Count}</strong> total</span>");
        builder.AppendLine($"<span class=\"metric\"><strong>{debt}</strong> debt score</span>");
        builder.AppendLine($"<span class=\"metric\">mode <strong>{Encode(mode.ToString().ToLowerInvariant())}</strong></span>");
        builder.AppendLine($"<span class=\"metric\"><strong>{result.Elapsed.TotalMilliseconds:F0}</strong> ms</span>");
        builder.AppendLine("</div>");
        builder.AppendLine("</section>");
    }

    private static void AppendQualityGates(StringBuilder builder, QualityGateEvaluation? gates)
    {
        builder.AppendLine("<section id=\"quality-gates\" class=\"panel\">");
        builder.AppendLine("<h2>Quality gates</h2>");

        if (gates is not { Enabled: true })
        {
            builder.AppendLine("<p class=\"muted\">Quality gates are disabled. Enable qualityGates in config to enforce aggregate thresholds.</p>");
            builder.AppendLine("</section>");
            return;
        }

        builder.AppendLine($"<p>Overall: <span class=\"badge {(gates.Passed ? "pass" : "fail")}\">{(gates.Passed ? "Passed" : "Failed")}</span></p>");
        builder.AppendLine("<table><thead><tr><th>Gate</th><th>Actual</th><th>Limit</th><th>Status</th></tr></thead><tbody>");

        foreach (var gate in gates.Gates.OrderBy(static g => g.Passed).ThenBy(static g => g.Name))
        {
            var target = string.IsNullOrWhiteSpace(gate.Target) ? gate.Name : $"{gate.Name} ({gate.Target})";
            builder.AppendLine("<tr>");
            builder.AppendLine($"<td>{Encode(target)}</td>");
            builder.AppendLine($"<td>{gate.Actual}</td>");
            builder.AppendLine($"<td>{gate.Limit?.ToString() ?? "—"}</td>");
            builder.AppendLine($"<td><span class=\"badge {(gate.Passed ? "pass" : "fail")}\">{(gate.Passed ? "Pass" : "Fail")}</span></td>");
            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody></table>");
        builder.AppendLine("</section>");
    }

    private static void AppendHotSpots(StringBuilder builder, HotSpotsReport? hotSpots)
    {
        builder.AppendLine("<section id=\"hot-spots\" class=\"panel\">");
        builder.AppendLine("<h2>Hot spots</h2>");

        if (hotSpots is null || hotSpots.FixPriority.Count == 0)
        {
            builder.AppendLine("<p class=\"muted\">No hot spots ranked for this scan.</p>");
            builder.AppendLine("</section>");
            return;
        }

        builder.AppendLine("<div class=\"tabs\">");
        builder.AppendLine("<button type=\"button\" class=\"tab active\" data-tab-group=\"hotspots\" data-tab=\"priority\">Fix priority</button>");
        builder.AppendLine("<button type=\"button\" class=\"tab\" data-tab-group=\"hotspots\" data-tab=\"rule\">By rule</button>");
        builder.AppendLine("<button type=\"button\" class=\"tab\" data-tab-group=\"hotspots\" data-tab=\"file\">By file</button>");
        builder.AppendLine("</div>");

        AppendHotSpotPanel(builder, "hotspots", "priority", hotSpots.FixPriority, active: true);
        AppendHotSpotPanel(builder, "hotspots", "rule", hotSpots.IssuesByRule);
        AppendHotSpotPanel(builder, "hotspots", "file", hotSpots.IssuesByFile);

        builder.AppendLine("</section>");
    }

    private static void AppendHotSpotPanel(
        StringBuilder builder,
        string group,
        string name,
        IReadOnlyList<HotSpotEntry> entries,
        bool active = false)
    {
        builder.AppendLine($"<div class=\"tab-panel{(active ? " active" : string.Empty)}\" data-tab-group=\"{group}\" data-tab=\"{name}\">");
        builder.AppendLine("<table><thead><tr><th>Target</th><th>Count</th><th>Debt</th><th>Priority</th><th></th></tr></thead><tbody>");

        foreach (var entry in entries)
        {
            var filterAttr = entry.Kind == "file"
                ? $"data-filter-file=\"{EncodeAttribute(entry.Key)}\""
                : $"data-filter-rule=\"{EncodeAttribute(entry.Key)}\"";
            builder.AppendLine("<tr>");
            builder.AppendLine($"<td>{Encode(entry.Key)}</td>");
            builder.AppendLine($"<td>{entry.Count}</td>");
            builder.AppendLine($"<td>{entry.DebtScore}</td>");
            builder.AppendLine($"<td>{entry.PriorityScore}</td>");
            builder.AppendLine($"<td><button type=\"button\" {filterAttr}>View findings</button></td>");
            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody></table>");
        builder.AppendLine("</div>");
    }

    private static void AppendDomainSections(StringBuilder builder, IReadOnlyList<DiagnosticRecord> diagnostics)
    {
        builder.AppendLine("<section id=\"domains\" class=\"panel\">");
        builder.AppendLine("<h2>Domain highlights</h2>");
        builder.AppendLine("<div class=\"domain-grid\">");

        AppendDomainCard(builder, "Security / SCA", diagnostics.Where(static d =>
            d.Category.Equals("Security", StringComparison.OrdinalIgnoreCase)
            || d.Rule.StartsWith("CSDS", StringComparison.OrdinalIgnoreCase)));

        AppendDomainCard(builder, "Architecture", diagnostics.Where(static d =>
            d.Rule.StartsWith("CSDA", StringComparison.OrdinalIgnoreCase)
            || d.Category.Equals("Architecture", StringComparison.OrdinalIgnoreCase)));

        AppendDomainCard(builder, "Maintainability", diagnostics.Where(static d =>
            d.Category.Equals("Maintainability", StringComparison.OrdinalIgnoreCase)
            || d.Rule.StartsWith("CSD03", StringComparison.OrdinalIgnoreCase)));

        builder.AppendLine("</div>");
        builder.AppendLine("</section>");
    }

    private static void AppendDomainCard(
        StringBuilder builder,
        string title,
        IEnumerable<DiagnosticRecord> diagnostics)
    {
        var list = diagnostics.Take(8).ToList();
        builder.AppendLine("<div class=\"recommendation-card panel\">");
        builder.AppendLine($"<h3>{Encode(title)}</h3>");

        if (list.Count == 0)
        {
            builder.AppendLine("<p class=\"muted\">No findings in this domain.</p>");
        }
        else
        {
            builder.AppendLine("<ul>");
            foreach (var diagnostic in list)
            {
                builder.AppendLine(
                    $"<li><span class=\"badge {SeverityClass(diagnostic.Severity)}\">{Encode(diagnostic.Severity.ToString())}</span> "
                    + $"{Encode(diagnostic.Plugin)}/{Encode(diagnostic.Rule)}: {Encode(diagnostic.Message)}</li>");
            }

            builder.AppendLine("</ul>");
        }

        builder.AppendLine("</div>");
    }

    private static void AppendFindings(StringBuilder builder, IReadOnlyList<DiagnosticRecord> diagnostics)
    {
        var categories = diagnostics.Select(static d => d.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(static c => c);

        builder.AppendLine("<section id=\"findings\" class=\"panel\">");
        builder.AppendLine("<h2>Findings explorer</h2>");
        builder.AppendLine("<div class=\"filters\">");
        builder.AppendLine("<input id=\"finding-search\" type=\"search\" placeholder=\"Search file, rule, message…\" aria-label=\"Search findings\" />");
        builder.AppendLine("<select id=\"finding-severity\" aria-label=\"Filter by severity\"><option value=\"\">All severities</option><option value=\"error\">Error</option><option value=\"warning\">Warning</option><option value=\"info\">Info</option></select>");
        builder.AppendLine("<select id=\"finding-category\" aria-label=\"Filter by category\"><option value=\"\">All categories</option>");
        foreach (var category in categories)
        {
            builder.AppendLine($"<option value=\"{EncodeAttribute(category)}\">{Encode(category)}</option>");
        }

        builder.AppendLine("</select>");
        builder.AppendLine("</div>");
        builder.AppendLine("<table><thead><tr><th>Finding</th><th>Location</th><th>Severity</th></tr></thead><tbody>");

        foreach (var diagnostic in diagnostics.OrderByDescending(static d => d.Severity).ThenBy(static d => d.FilePath))
        {
            var searchBlob = $"{diagnostic.FilePath} {diagnostic.Plugin}/{diagnostic.Rule} {diagnostic.Message} {diagnostic.Help}"
                .ToLowerInvariant();
            builder.AppendLine(
                $"<tr class=\"finding-row\" data-severity=\"{EncodeAttribute(diagnostic.Severity.ToString().ToLowerInvariant())}\" "
                + $"data-category=\"{EncodeAttribute(diagnostic.Category)}\" data-search=\"{EncodeAttribute(searchBlob)}\">");
            builder.AppendLine("<td>");
            builder.AppendLine("<details>");
            builder.AppendLine(
                $"<summary>{Encode(diagnostic.Plugin)}/{Encode(diagnostic.Rule)} — {Encode(diagnostic.Message)}</summary>");
            if (!string.IsNullOrWhiteSpace(diagnostic.Help))
            {
                builder.AppendLine($"<div class=\"help\">Recommended: {Encode(diagnostic.Help)}</div>");
            }

            builder.AppendLine($"<div class=\"help muted\">Identity: {Encode(DiagnosticIdentity.Build(diagnostic))}</div>");
            builder.AppendLine("</details>");
            builder.AppendLine("</td>");
            builder.AppendLine($"<td>{Encode(diagnostic.FilePath)}:{diagnostic.Line}:{diagnostic.Column}</td>");
            builder.AppendLine($"<td><span class=\"badge {SeverityClass(diagnostic.Severity)}\">{Encode(diagnostic.Severity.ToString())}</span></td>");
            builder.AppendLine("</tr>");
        }

        builder.AppendLine("</tbody></table>");
        builder.AppendLine("</section>");
    }

    private static void AppendRecommendations(StringBuilder builder, HtmlRecommendationGroups groups)
    {
        builder.AppendLine("<section id=\"recommendations\" class=\"panel\">");
        builder.AppendLine("<h2>Recommendation board</h2>");
        builder.AppendLine("<div class=\"recommendation-grid\">");
        AppendRecommendationCard(builder, "Fix now", groups.FixNow);
        AppendRecommendationCard(builder, "Next sprint", groups.NextSprint);
        AppendRecommendationCard(builder, "Watch", groups.Watch);
        AppendRecommendationCard(builder, "Configure", groups.Configure);
        builder.AppendLine("</div>");
        builder.AppendLine("</section>");
    }

    private static void AppendRecommendationCard(
        StringBuilder builder,
        string title,
        IReadOnlyList<HtmlRecommendationItem> items)
    {
        builder.AppendLine("<div class=\"recommendation-card\">");
        builder.AppendLine($"<h3>{Encode(title)}</h3>");

        if (items.Count == 0)
        {
            builder.AppendLine("<p class=\"muted\">Nothing in this lane.</p>");
        }
        else
        {
            builder.AppendLine("<ul>");
            foreach (var item in items.Take(12))
            {
                var location = string.IsNullOrWhiteSpace(item.LinkTarget) ? string.Empty : $" <span class=\"muted\">({Encode(item.LinkTarget)})</span>";
                builder.AppendLine($"<li><strong>{Encode(item.Title)}</strong>{location}<br />{Encode(item.Detail)}</li>");
            }

            builder.AppendLine("</ul>");
        }

        builder.AppendLine("</div>");
    }

    private static void AppendRawJson(StringBuilder builder, string jsonPayload)
    {
        builder.AppendLine("<section id=\"raw-json\" class=\"panel\">");
        builder.AppendLine("<h2>Raw JSON</h2>");
        builder.AppendLine("<details><summary>Show embedded schema v2 payload</summary>");
        builder.AppendLine($"<pre class=\"raw-json\">{Encode(jsonPayload)}</pre>");
        builder.AppendLine("</details>");
        builder.AppendLine("</section>");
    }

    private static string SeverityClass(DiagnosticSeverity severity) =>
        severity switch
        {
            DiagnosticSeverity.Error => "error",
            DiagnosticSeverity.Warning => "warning",
            _ => "info",
        };

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string EncodeAttribute(string value) => WebUtility.HtmlEncode(value).Replace("\"", "&quot;", StringComparison.Ordinal);
}
