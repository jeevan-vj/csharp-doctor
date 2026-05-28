# Threat Model

## Assets

- Source code on disk (read-only during scan)
- Local `.csharp-doctor.json`
- CI environment variables

## Attack surface

| Surface | Risk | Mitigation |
|---------|------|------------|
| File system reads | Untrusted repo code parsed by Roslyn | Run in isolated CI job; no code execution |
| Git subprocess | Command injection via malicious refs | Pass refs through `ProcessStartInfo`; no shell |
| Optional OTLP | Data exfil if misconfigured | Opt-in only; requires explicit endpoint + auth header |
| Score API (future) | Metadata leak | Redact absolute paths; document payload |

## Defaults

- **No telemetry** unless `CSHARP_DOCTOR_OTLP_ENDPOINT` and `CSHARP_DOCTOR_OTLP_AUTH_HEADER` are both set.
- **No network** in core scan path.
- Findings stay on stdout / artifact files unless user configures upload.
