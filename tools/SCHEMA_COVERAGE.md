# CDDA item schema coverage

Run schema_coverage.py against a **pinned local checkout** of
LambdaSix/Cataclysm-DDA data/json/items. The script scans every JSON array,
counts item subtype memberships, identifies common and subtype DTO properties,
and publishes the most frequent unmapped top-level and nested fields.

This is deliberately a *static schema inventory*, not a successful
deserialization guarantee. Authored fields covered by a named DTO property
count as mapped; unknown data retained in JsonExtensionData do not.
Inherited definitions are not expanded and mods are excluded by default.

Example:

    python3 tools/schema_coverage.py \
      --data-dir /path/to/Cataclysm-DDA/data/json/items \
      --model-dir src/OpenCatasharpData/Models \
      --source-revision <fork-commit-sha> \
      --model-revision <library-commit-sha> \
      --output artifacts/item-schema-coverage.md

GitHub Actions (schema-coverage.yml) scans the pinned source revision and
uploads the Markdown report as a build artifact and in the run summary.
The SHA is fixed so subsequent reports can be compared meaningfully.
Repoint that SHA deliberately when updating game data.
