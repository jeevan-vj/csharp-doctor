---
description: Pack and push CSharpDoctor.Cli to NuGet.org from local (restore, test, pack, push).
argument-hint: [version] e.g. 0.2.0
---

# Publish CSharpDoctor.Cli to NuGet

Pack the global tool and push `.nupkg` + `.snupkg` to [nuget.org](https://www.nuget.org) from the developer machine. Use this for first publish, hotfixes, or when CI secrets are not configured yet.

**CI alternative:** tag `v<version>` and let [`.github/workflows/release.yml`](../../.github/workflows/release.yml) publish (requires `NUGET_API_KEY` GitHub secret).

## Usage

```
/nuget-publish
/nuget-publish 0.2.0
/nuget-publish 0.2.0-preview.1
```

- **version** (optional): SemVer to pack and push. If omitted, read `Version` from `src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj` (default `0.1.0` when unset).

## Preflight

Run these checks before packing or pushing. Stop on hard failures; warn on soft ones.

1. **.NET SDK on PATH** — Agent shells often lack `dotnet`. Always prefix commands:

   ```bash
   export PATH="$HOME/.dotnet:$PATH"
   export DOTNET_ROOT="$HOME/.dotnet"
   which dotnet && dotnet --version
   ```

   Expect SDK `10.0.x` per [`global.json`](../../global.json).

2. **NuGet API key** — Required for push. Never commit or echo the key.

   - Create at nuget.org → Account → API Keys
   - Scope: `Push new packages and package versions`
   - Glob: `CSharpDoctor.*`
   - Load from repo-root `.env` (gitignored):

     ```bash
     # .env should contain: NUGET_API_KEY=...
     set -a && [ -f .env ] && . ./.env && set +a
     if [ -z "${NUGET_API_KEY:-}" ]; then
       echo "NUGET_API_KEY not set. Add to .env or export before push."
       exit 1
     fi
     ```

3. **ID prefix reservation** — On nuget.org, reserve `CSharpDoctor.*` before the first push (Account → ID Prefix Reservations).

4. **Resolve version** — Set `VERSION` from `$ARGUMENTS` or csproj:

   ```bash
   VERSION="${ARGUMENTS:-}"
   if [ -z "$VERSION" ]; then
     VERSION=$(grep -m1 '<Version' src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj \
       | sed -E 's/.*>([^<]+)<.*/\1/' | tr -d ' ')
   fi
   echo "Publishing version: $VERSION"
   ```

5. **Working tree** — `git status --porcelain`. If dirty, warn that the pushed package may not match a tagged commit. Recommend committing and tagging `v$VERSION` after a successful push.

6. **Tests** — Full solution must pass before push (same bar as CI).

## Plan

State the intended action before executing:

| Step | Command | Notes |
|------|---------|-------|
| Restore | `dotnet restore CSharpDoctor.sln` | |
| Build | `dotnet build -c Release /p:Version=$VERSION` | |
| Test | `dotnet test -c Release --no-build` | Block push on failure |
| Pack | `dotnet pack ... -o ./artifacts /p:Version=$VERSION` | Produces `.nupkg` + `.snupkg` |
| Push | `dotnet nuget push` × 2 | `--skip-duplicate` safe for retries |
| Verify | `dotnet tool install` smoke test | Optional but recommended |

**Destructive / irreversible:** Pushing a version to nuget.org cannot be undone (only unlisted). **Ask for explicit confirmation** before `dotnet nuget push` if the version is new or production (`$VERSION` has no `-preview`/`-rc` suffix).

## Commands

Run from the repository root.

```bash
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
cd "$(git rev-parse --show-toplevel)"

# Version: use argument or csproj default
VERSION="${1:-}"  # agent maps $ARGUMENTS to this
if [ -z "$VERSION" ]; then
  VERSION=$(grep -m1 '<Version' src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj \
    | sed -E 's/.*>([^<]+)<.*/\1/' | tr -d ' ')
fi

set -a && [ -f .env ] && . ./.env && set +a

mkdir -p ./artifacts

dotnet restore CSharpDoctor.sln

dotnet build CSharpDoctor.sln \
  -c Release --no-restore \
  /p:Version="$VERSION"

dotnet test CSharpDoctor.sln -c Release --no-build --verbosity normal

dotnet pack src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj \
  -c Release --no-build \
  -o ./artifacts \
  /p:Version="$VERSION"

ls -la ./artifacts/*."$VERSION".nupkg ./artifacts/*."$VERSION".snupkg
```

**Push** (after user confirms for stable releases):

```bash
dotnet nuget push "./artifacts/CSharpDoctor.Cli.${VERSION}.nupkg" \
  --api-key "$NUGET_API_KEY" \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate

dotnet nuget push "./artifacts/CSharpDoctor.Cli.${VERSION}.snupkg" \
  --api-key "$NUGET_API_KEY" \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate
```

**Smoke test install** (after push; nuget.org indexing may take 1–5 minutes):

```bash
dotnet tool install -g CSharpDoctor.Cli --version "$VERSION"
csharp-doctor --help
```

**Local feed smoke test** (before push, no API key):

```bash
mkdir -p /tmp/csd-smoke && cd /tmp/csd-smoke
dotnet new tool-manifest
dotnet tool install CSharpDoctor.Cli --version "$VERSION" \
  --add-source "$(git rev-parse --show-toplevel)/artifacts"
dotnet csharp-doctor --help
```

## Verification

After push, confirm the package is live:

1. **CLI output** — `dotnet nuget push` should report success (not `409` unless `--skip-duplicate` and version already exists).
2. **NuGet.org** — Open `https://www.nuget.org/packages/CSharpDoctor.Cli/$VERSION` (may 404 briefly while indexing).
3. **Global tool** — `dotnet tool install -g CSharpDoctor.Cli --version $VERSION` succeeds.
4. **Symbols** — Package page shows symbol package if `.snupkg` pushed.

On failure, common causes:

| Error | Fix |
|-------|-----|
| `403 Forbidden` | Wrong API key, expired key, or glob does not include `CSharpDoctor.Cli` |
| `409 Conflict` | Version already published; bump version or use `--skip-duplicate` |
| `NU5104` / pack warnings | Already suppressed in csproj (`NU5104` for prerelease deps) |
| Tests failed | Fix tests; do not push |

## Summary

Present a structured result:

```
## NuGet Publish Result
- **Package**: CSharpDoctor.Cli
- **Version**: <version>
- **Artifacts**: ./artifacts/CSharpDoctor.Cli.<version>.nupkg (+ .snupkg)
- **Source**: nuget.org
- **Status**: success | skipped-duplicate | failed
- **Tests**: passed | failed
- **Commit**: <short-sha or "dirty working tree">
```

If push succeeded, include the package URL: `https://www.nuget.org/packages/CSharpDoctor.Cli/<version>`

## Next Steps

- **Tag the release** — `git tag v$VERSION && git push origin v$VERSION` (triggers CI release workflow; duplicate push is harmless with `--skip-duplicate`).
- **GitHub Release** — CI creates one on tag push; or create manually with `./artifacts/*.*nupkg` attached.
- **Bump repo defaults** — Align `src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj`, `README.md`, and `action.yml` to the shipped version and rule count.
- **CI secret** — Store the same API key as `NUGET_API_KEY` in GitHub repo secrets for tag-driven releases.
- **Trusted Publishing** — After first publish, enable OIDC on nuget.org and uncomment the `nuget/login@v1` block in `release.yml`.
- **Install docs** — Users install with:

  ```bash
  dotnet tool install -g CSharpDoctor.Cli --version <version>
  csharp-doctor .
  ```
