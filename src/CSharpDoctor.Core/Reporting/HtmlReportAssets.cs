namespace CSharpDoctor.Core.Reporting;

internal static class HtmlReportAssets
{
    internal const string Css = """
        :root {
          --bg: #f6f5f2;
          --surface: #ffffff;
          --text: #1c1b19;
          --muted: #5c5a56;
          --border: #ddd9d2;
          --pass: #1f6b45;
          --fail: #b42318;
          --warn: #9a6700;
          --info: #3b5bdb;
          --accent: #2f5d50;
        }
        * { box-sizing: border-box; }
        body {
          margin: 0;
          font-family: "Segoe UI", system-ui, sans-serif;
          background: var(--bg);
          color: var(--text);
          line-height: 1.45;
        }
        .layout { display: grid; grid-template-columns: 220px 1fr; min-height: 100vh; }
        nav {
          position: sticky; top: 0; align-self: start; height: 100vh;
          padding: 1.5rem 1rem; background: #eceae4; border-right: 1px solid var(--border);
        }
        nav h1 { font-size: 1rem; margin: 0 0 1rem; }
        nav a { display: block; color: var(--text); text-decoration: none; padding: 0.35rem 0; }
        nav a:hover { color: var(--accent); }
        main { padding: 1.5rem 2rem 3rem; max-width: 1200px; }
        section { margin-bottom: 2rem; }
        h2 { margin: 0 0 0.75rem; font-size: 1.25rem; }
        .hero {
          background: var(--surface); border: 1px solid var(--border); border-radius: 12px;
          padding: 1.25rem 1.5rem; margin-bottom: 1.5rem;
        }
        .status { font-size: 1.5rem; font-weight: 700; margin-bottom: 0.5rem; }
        .status.pass { color: var(--pass); }
        .status.fail { color: var(--fail); }
        .metrics { display: flex; flex-wrap: wrap; gap: 1rem; color: var(--muted); }
        .metric strong { color: var(--text); }
        .panel {
          background: var(--surface); border: 1px solid var(--border); border-radius: 10px;
          padding: 1rem 1.25rem;
        }
        table { width: 100%; border-collapse: collapse; font-size: 0.92rem; }
        th, td { text-align: left; padding: 0.5rem 0.4rem; border-bottom: 1px solid var(--border); vertical-align: top; }
        .badge {
          display: inline-block; padding: 0.1rem 0.45rem; border-radius: 999px;
          font-size: 0.75rem; font-weight: 600; text-transform: uppercase;
        }
        .badge.error { background: #fde8e6; color: var(--fail); }
        .badge.warning { background: #fff4d6; color: var(--warn); }
        .badge.info { background: #e7edff; color: var(--info); }
        .badge.pass { background: #e3f5ec; color: var(--pass); }
        .badge.fail { background: #fde8e6; color: var(--fail); }
        .bar-track { background: #eee9e2; border-radius: 4px; height: 8px; overflow: hidden; }
        .bar-fill { height: 100%; background: var(--accent); }
        .tabs { display: flex; gap: 0.5rem; margin-bottom: 0.75rem; }
        .tab {
          border: 1px solid var(--border); background: var(--surface); padding: 0.35rem 0.75rem;
          border-radius: 6px; cursor: pointer;
        }
        .tab.active { border-color: var(--accent); color: var(--accent); font-weight: 600; }
        .tab-panel { display: none; }
        .tab-panel.active { display: block; }
        .filters { display: flex; flex-wrap: wrap; gap: 0.5rem; margin-bottom: 0.75rem; }
        .filters input, .filters select { padding: 0.35rem 0.5rem; border: 1px solid var(--border); border-radius: 6px; }
        .finding-row details summary { cursor: pointer; font-weight: 600; }
        .finding-row .help { color: var(--muted); margin-top: 0.35rem; }
        .recommendation-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 1rem; }
        .recommendation-card h3 { margin: 0 0 0.5rem; font-size: 0.95rem; }
        .recommendation-card ul { margin: 0; padding-left: 1.1rem; }
        .recommendation-card li { margin-bottom: 0.45rem; }
        .muted { color: var(--muted); }
        .domain-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1rem; }
        pre.raw-json {
          max-height: 320px; overflow: auto; background: #f0eeea; padding: 0.75rem;
          border-radius: 8px; font-size: 0.8rem;
        }
        @media print {
          nav { display: none; }
          .layout { display: block; }
          .filters, .tabs { display: none; }
        }
        """;

    internal const string JavaScript = """
        (function () {
          const data = JSON.parse(document.getElementById('report-data').textContent);
          const findings = data.diagnostics || [];

          document.querySelectorAll('.tab').forEach(function (tab) {
            tab.addEventListener('click', function () {
              const group = tab.dataset.tabGroup;
              const name = tab.dataset.tab;
              document.querySelectorAll('.tab[data-tab-group="' + group + '"]').forEach(function (t) {
                t.classList.toggle('active', t === tab);
              });
              document.querySelectorAll('.tab-panel[data-tab-group="' + group + '"]').forEach(function (panel) {
                panel.classList.toggle('active', panel.dataset.tab === name);
              });
            });
          });

          const search = document.getElementById('finding-search');
          const severity = document.getElementById('finding-severity');
          const category = document.getElementById('finding-category');
          function applyFilters() {
            const q = (search.value || '').toLowerCase();
            const sev = severity.value;
            const cat = category.value;
            document.querySelectorAll('.finding-row').forEach(function (row) {
              const matchQ = !q || row.dataset.search.includes(q);
              const matchSev = !sev || row.dataset.severity === sev;
              const matchCat = !cat || row.dataset.category === cat;
              row.style.display = matchQ && matchSev && matchCat ? '' : 'none';
            });
          }
          if (search) { search.addEventListener('input', applyFilters); }
          if (severity) { severity.addEventListener('change', applyFilters); }
          if (category) { category.addEventListener('change', applyFilters); }

          document.querySelectorAll('[data-filter-rule]').forEach(function (btn) {
            btn.addEventListener('click', function () {
              if (search) { search.value = btn.dataset.filterRule; }
              document.getElementById('findings').scrollIntoView({ behavior: 'smooth' });
              applyFilters();
            });
          });

          document.querySelectorAll('[data-filter-file]').forEach(function (btn) {
            btn.addEventListener('click', function () {
              if (search) { search.value = btn.dataset.filterFile; }
              document.getElementById('findings').scrollIntoView({ behavior: 'smooth' });
              applyFilters();
            });
          });
        })();
        """;
}
