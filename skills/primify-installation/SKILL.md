# primify-installation

## When to use this skill
- The user is onboarding to the Primify library or a project wants to consume `Primify` and its generator.
- You need to document how to add the NuGet package, pull in the source generator, or keep Flowgen available.
- Someone asks for the canonical package version or how to wire up the runtime plus analyzer.

## Installation guidance
1. Add `Primify` to the consuming project via NuGet:
   ```xml
   <PackageReference Include="Primify" Version="1.9.3" />
   ```
2. Include the source generator when you build locally by pointing at `Primify.Generators`:
   ```xml
   <ProjectReference Include="../src/Primify.Generators/Primify.Generators.csproj"
                     OutputItemType="Analyzer"
                     ReferenceOutputAssembly="false" />
   <Analyzer Include="../src/Primify.Generators/bin/$(Configuration)/netstandard2.0/Flowgen.dll" />
   ```
   This mirrors the packaging flow in `src/Primify/Primify.csproj`.
3. If you package for others (NuGet or CI), make sure `Flowgen.dll` ships alongside the generator under `analyzers/dotnet/cs`; the generator itself copies Flowgen out of the Flowgen package (`$(PkgFlowgen)/lib/netstandard2.0/Flowgen.dll`).
4. Use the `[Primify<T>]` attribute on classes/structs. Add optional private static helpers `Normalize(T)` and `void Validate(T)` when you need transformation or guarding logic.
5. `Normalize` runs before `Validate`; treat it as a cleanup step (e.g., clamp negatives to `0` or trim whitespace) and let `Validate` cover separate invariants such as upper bounds or maximum lengths. The `StringClassWithNormalizeAndValidate` sample proves the generator honors that order without introducing conflicts between the two helpers.

## Testing and verification
- Run `dotnet run --project tests/Primify.Tests/Primify.Tests.csproj -c Release` to ensure the generator produces the expected code.
- The benchmarks project relies on Flowgen being available as both an analyzer and package reference; keep `Flowgen` pinned to `0.7.0` with `GeneratePathProperty="true"` when editing `Primify.Generators`.

## Troubleshooting
- GitHub Actions (Publish workflow) must authenticate with a PAT that has `read:packages` because the Flowgen feed is hosted at `https://nuget.pkg.github.com/koddek/download/flowgen`. The repo stores that PAT in `GH_PACKAGES_TOKEN`.
- When Flowgen fails to load, the generator never initializes (`CS8784`) and `From` methods disappear.

## Reference
- Project-level guidance lives in `README.md` and `src/Primify/Primify.csproj`.
