# Azure DevOps

```yaml
- task: DotNetCoreCLI@2
  displayName: Install csharp-doctor
  inputs:
    command: custom
    custom: tool
    arguments: install -g CSharpDoctor.Cli

- script: csharp-doctor $(Build.SourcesDirectory) --format sarif > $(Build.ArtifactStagingDirectory)/csharp-doctor.sarif
  displayName: Run CSharp Doctor

- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: $(Build.ArtifactStagingDirectory)/csharp-doctor.sarif
    ArtifactName: CodeAnalysisLogs

- task: PublishCodeAnalysisResults@2
  inputs:
    SarifOutputFiles: $(Build.ArtifactStagingDirectory)/csharp-doctor.sarif
```
