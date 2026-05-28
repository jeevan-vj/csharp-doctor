# Suppression

## Config

- `ignore.rules` — rule ids (`CSD0001` or `csharp-doctor/CSD0001`)
- `ignore.files` — glob patterns
- `ignore.tags` — rule tags (`design`, `security`, …)
- `rules` / `categories` severity `off` drops findings before reporting

## Inline comments

| Comment | Effect |
|---------|--------|
| `// csharp-doctor-disable-next-line` | Suppress next line |
| `// csharp-doctor-disable-line` | Suppress current line |
| `// csharp-doctor-disable-line csharp-doctor/CSD0001` | Suppress specific rule |

Set `"respectInlineDisables": false` for audit mode (ignores inline disables).

## Surfaces

Rules tagged `design` are shown in local CLI but excluded from PR comments, score, and CI failure by default.
