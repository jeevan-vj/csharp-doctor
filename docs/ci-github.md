# GitHub Actions

```yaml
- uses: actions/checkout@v4
  with:
    fetch-depth: 0

- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '9.0.x'

- name: CSharp Doctor
  uses: ./
  with:
    directory: .
    diff: origin/main
    fail-on: error
    annotations: true
```

## SARIF upload

```yaml
- name: Scan
  run: dotnet csharp-doctor . --format sarif > csharp-doctor.sarif

- uses: github/codeql-action/upload-sarif@v3
  with:
    sarif_file: csharp-doctor.sarif
```
