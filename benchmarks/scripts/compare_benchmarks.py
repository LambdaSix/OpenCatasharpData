#!/usr/bin/env python3
"""Compare BenchmarkDotNet full-JSON reports from base and candidate.

Mean timings are nanoseconds per invocation, allocations are bytes per
invocation. Best used for paired runs on the same machine and SDK.
"""
import argparse
import json
import math
from pathlib import Path
import sys


def load_reports(location: Path):
    paths = [location] if location.is_file() else sorted(location.rglob("*-report-full.json"))
    if not paths:
        raise ValueError(f"No BenchmarkDotNet full JSON reports at {location}")
    results = {}
    for path in paths:
        report = json.loads(path.read_text(encoding="utf-8"))
        if not isinstance(report.get("Benchmarks"), list):
            raise ValueError(f"Missing Benchmarks array: {path}")
        for item in report["Benchmarks"]:
            key = ".".join(str(item.get(p)) for p in ("Namespace", "Type", "Method") if item.get(p))
            params = item.get("Parameters", "")
            if params:
                key += "(" + (params if isinstance(params, str) else json.dumps(params, sort_keys=True)) + ")"
            statistics = item.get("Statistics") or {}
            mean = statistics.get("Mean")
            if mean is None or not math.isfinite(float(mean)) or float(mean) <= 0:
                raise ValueError(f"Invalid mean for {key}: {path}")
            memory = item.get("Memory") or item.get("MemoryStats") or {}
            allocated = memory.get("BytesAllocatedPerOperation", item.get("BytesAllocatedPerOperation"))
            if allocated is not None and (not math.isfinite(float(allocated)) or float(allocated) < 0):
                raise ValueError(f"Invalid allocation value for {key}: {path}")
            if key in results:
                raise ValueError(f"Duplicate benchmark identity {key}; run one job/runtime per comparison.")
            results[key] = {
                "mean": float(mean),
                "stddev": float(statistics.get("StandardDeviation") or 0),
                "allocated": float(allocated) if allocated is not None else None,
            }
    if not results:
        raise ValueError("No successful measurements in exported files")
    return results


def compare(baseline, current, time_threshold=25, allocation_threshold=25):
    lines = [
        "## BenchmarkDotNet regression comparison", "",
        f"Time threshold: +{time_threshold:g}%; allocation threshold: +{allocation_threshold:g}%. "
        "Timing is mean ns/op; allocations are B/op.", "",
        "| Benchmark | Baseline ns/op | Candidate ns/op | Time delta | Baseline B/op | Candidate B/op | Allocation delta | Result |",
        "|---|---:|---:|---:|---:|---:|---:|---|",
    ]
    failures = 0
    before = set(baseline)
    after = set(current)
    for key in sorted(before & after):
        old, new = baseline[key], current[key]
        time_change = (new["mean"] / old["mean"] - 1) * 100
        # Conservative noise guard. This is not a significance test.
        noise = 2 * math.hypot(old["stddev"], new["stddev"]) / old["mean"] * 100
        time_regressed = time_change > max(time_threshold, noise)
        old_alloc, new_alloc = old["allocated"], new["allocated"]
        if old_alloc is not None and new_alloc is not None:
            alloc_change = (new_alloc / old_alloc - 1) * 100 if old_alloc > 0 else None
            alloc_regressed = ((alloc_change is not None and alloc_change > allocation_threshold)
                               or (old_alloc == 0 and new_alloc > 0)) and (new_alloc - old_alloc >= 64)
            allocated_before, allocated_after = f"{old_alloc:,.0f}", f"{new_alloc:,.0f}"
            allocation_delta = f"{alloc_change:+.1f}%" if alloc_change is not None else "new"
        else:
            alloc_regressed = False
            allocated_before = allocated_after = allocation_delta = "n/a"
        regressed = time_regressed or alloc_regressed
        failures += int(regressed)
        status = "REGRESSION" if regressed else ("noisy" if time_change > time_threshold else "OK")
        lines.append(
            f"| {key} | {old['mean']:,.1f} | {new['mean']:,.1f} | {time_change:+.1f}% "
            f"| {allocated_before} | {allocated_after} | {allocation_delta} | {status} |"
        )

    added, removed = sorted(after - before), sorted(before - after)
    if added:
        lines += ["", "**New benchmarks without a reference:** " + ", ".join(added)]
    if removed:
        lines += ["", "**Benchmarks missing from candidate:** " + ", ".join(removed)]
    if not before & after:
        failures += 1
        lines += ["", "**No common benchmarks: comparison is invalid.**"]
    lines += ["", f"**Flagged regressions: {failures}.** Runner noise can require a repeat to confirm."]
    return "\n".join(lines) + "\n", failures


def initial_report(current):
    lines = [
        "## BenchmarkDotNet initial reference", "",
        "The base commit has no benchmark project. This report establishes a first measurement. "
        "Future PRs will compare base and candidate on the same runner.", "",
        "| Benchmark | Mean ns/op | Allocated B/op |",
        "|---|---:|---:|",
    ]
    for key, metrics in sorted(current.items()):
        allocated = metrics["allocated"]
        allocated_text = f"{allocated:,.0f}" if allocated is not None else "n/a"
        lines.append(f"| {key} | {metrics['mean']:,.1f} | {allocated_text} |")
    return "\n".join(lines) + "\n"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--current", type=Path, required=True)
    parser.add_argument("--baseline", type=Path)
    parser.add_argument("--time-threshold", type=float, default=25)
    parser.add_argument("--allocation-threshold", type=float, default=25)
    parser.add_argument("--markdown", type=Path, help="Append Markdown to this path")
    parser.add_argument("--fail-on-regression", action="store_true")
    args = parser.parse_args()
    if args.time_threshold < 0 or args.allocation_threshold < 0:
        parser.error("Thresholds must be non-negative")

    try:
        candidate = load_reports(args.current)
        if args.baseline is None:
            output, failures = initial_report(candidate), 0
        else:
            output, failures = compare(
                load_reports(args.baseline), candidate, args.time_threshold, args.allocation_threshold
            )
    except (ValueError, OSError, json.JSONDecodeError) as exc:
        print(f"Benchmark report error: {exc}", file=sys.stderr)
        return 2

    print(output)
    if args.markdown:
        with args.markdown.open("a", encoding="utf-8") as destination:
            destination.write(output)
    return int(failures > 0 and args.fail_on_regression)


if __name__ == "__main__":
    sys.exit(main())
