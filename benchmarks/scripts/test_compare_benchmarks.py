import json
from pathlib import Path
import tempfile
import unittest

from compare_benchmarks import compare, initial_report, load_reports


def sample(mean, allocated=128, stddev=1):
    return {"mean": mean, "allocated": allocated, "stddev": stddev}


class ComparatorTests(unittest.TestCase):
    def test_time_regression(self):
        text, count = compare({"Demo.Parse": sample(100)}, {"Demo.Parse": sample(150)})
        self.assertEqual(count, 1)
        self.assertIn("REGRESSION", text)

    def test_improvement(self):
        _, count = compare({"Demo.Parse": sample(100)}, {"Demo.Parse": sample(75)})
        self.assertEqual(count, 0)

    def test_memory_regression(self):
        _, count = compare({"Demo.Parse": sample(100, 100)}, {"Demo.Parse": sample(100, 200)})
        self.assertEqual(count, 1)

    def test_ignore_tiny_allocation_difference(self):
        _, count = compare({"Demo.Parse": sample(100, 10)}, {"Demo.Parse": sample(100, 11)})
        self.assertEqual(count, 0)

    def test_no_matching_benchmarks_is_error(self):
        text, count = compare({"Demo.Old": sample(100)}, {"Demo.New": sample(100)})
        self.assertGreater(count, 0)
        self.assertIn("No common benchmarks", text)

    def test_first_reference(self):
        self.assertIn("Demo.Parse", initial_report({"Demo.Parse": sample(100)}))

    def test_read_full_reports(self):
        with tempfile.TemporaryDirectory() as folder:
            report = {
                "Benchmarks": [{
                    "Namespace": "Demo", "Type": "Records", "Method": "Parse",
                    "Parameters": "", "Statistics": {"Mean": 50, "StandardDeviation": 3},
                    "Memory": {"BytesAllocatedPerOperation": 123},
                }]
            }
            file = Path(folder) / "Records-report-full.json"
            file.write_text(json.dumps(report), encoding="utf-8")
            results = load_reports(Path(folder))
            self.assertEqual(results["Demo.Records.Parse"]["allocated"], 123)


if __name__ == "__main__":
    unittest.main()
