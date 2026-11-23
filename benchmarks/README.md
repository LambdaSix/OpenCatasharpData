# OpenCatasharpData performance benchmarks

BenchmarkDotNet 0.15.8 on .NET 10. Benchmark project: benchmarks/OpenCatasharpData.Benchmarks.

## What's measured

- RecordParsingBenchmarks: generic item, comestible, armor, gun and magazine single-record parsing from prepared JSON strings, plus handler dispatch + DTO deserialization of an already parsed JsonElement.
- LoadingBenchmarks: parse a JSON array from a memory stream, load a single file, recursively load a directory into a typed catalog, construct an in-memory catalog, look up an ID and query by DTO type.

Fixtures are deterministic and inspired by LambdaSix/Cataclysm-DDA, with multiple nested fields and varied JSON shapes. Setup generates fixture files and seed definitions outside measured operations. The suite does not download Cataclysm-DDA content or require a separate fork checkout.

## Run locally

Run on an otherwise idle computer without a debugger, using Release configuration.

~~~bash
# Complete run; default BenchmarkDotNet job (best for a local baseline)
dotnet run --project benchmarks/OpenCatasharpData.Benchmarks -c Release -- \
  --filter '*' --artifacts ./benchmark-output/reference

# Short job, matching CI
dotnet run --project benchmarks/OpenCatasharpData.Benchmarks -c Release -- \
  --job Short --filter '*' --artifacts ./benchmark-output/candidate
~~~

Reports, including full JSON, CSV and Markdown, will be under the requested artifacts directory's results/ subdirectory.

Compare results with only Python's standard library:

~~~bash
python3 benchmarks/scripts/compare_benchmarks.py \
  --baseline ./benchmark-output/reference/results \
  --current ./benchmark-output/candidate/results \
  --time-threshold 25 --allocation-threshold 25 \
  --fail-on-regression
~~~

Do not compare a local default-job run with CI's short-job run. The hardware, OS, runtime, job, and SDK should match across references and candidates.

## PR regression baseline

The Benchmark regression GitHub Actions workflow measures the PR base commit followed by the candidate commit on the same GitHub-hosted runner. It uses the same short BenchmarkDotNet job and fixture definitions for each version.

- The PR base SHA is the performance reference. This avoids relying on committed measurements obtained on a different machine.
- Timing regressions above 25% are flagged unless the conservative two-standard-deviation noise guard is larger. Managed allocations growing above 25% AND at least 64 bytes/operation are flagged.
- A flagged regression fails the comparison step. This is a heuristic guard, not proof of a statistical regression; investigate and rerun noisy cases.
- The GitHub Actions job summary includes a comparison table. Raw JSON and other results are uploaded as artifacts with a 90-day retention period.
- On this first PR, the base commit does not yet have the benchmark project. The workflow publishes an initial measurement table and artifact rather than inventing historical baseline values. After the suite merges, later PRs will automatically compare their base revision and candidate.
- A manual workflow_dispatch run also creates a reference report + artifact without comparing versions.

Strong regression gating with narrow thresholds needs stable dedicated benchmark hardware; hosted runners are inherently variable. Keep benchmark methods and fixtures stable when comparing changes to the library. If a fixture is intentionally modified, treat its results as a new baseline.

## Interpreting numbers

The scope of each measurement matters. RecordParsingBenchmarks measure record parsing and typed DTO construction. The pre-parsed-element case isolates parsing overhead from JSON DOM construction. LoadingBenchmarks include stream or disk operations and, for directory loading, index construction.

Mean time is reported in nanoseconds per operation. MemoryDiagnoser measures managed allocated bytes per operation, not peak process memory.

Use the raw JSON reports for analysis and repeat suspicious changes with consistent machine conditions.
