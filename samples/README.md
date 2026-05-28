# Sample projects for manual smoke tests

## Class library with intentional issues (`Sample/`)

Expected findings: `CSD0001` (SQL concat), `CSD0002` (MD5).

### Pack, install, and scan (from repo root)

```bash
dotnet pack src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj -c Release -o ./nupkg

# Global install from local feed
dotnet tool install -g --add-source ./nupkg CSharpDoctor.Cli

# Or install into ./.tools (no global pollution)
dotnet tool install CSharpDoctor.Cli --tool-path ./.tools --add-source ./nupkg --version 0.1.0
./.tools/csharp-doctor samples/Sample

# Scan
csharp-doctor samples/Sample
```

On macOS, if `dotnet` lives under `~/.dotnet`, set:

```bash
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
```
