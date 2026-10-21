"""Require an executed SQL integration suite without failures or skipped cases."""
import sys
import xml.etree.ElementTree as ET
from pathlib import Path


def verify(path: Path) -> None:
    ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
    root = ET.parse(path).getroot()
    counters = root.find(".//t:Counters", ns)
    if counters is None:
        raise ValueError("TRX counters are missing")
    total = int(counters.get("total", "0"))
    passed = int(counters.get("passed", "0"))
    if total == 0 or passed != total or int(counters.get("executed", "0")) != total:
        raise ValueError("All backend tests must execute and pass, without skipped cases")
    results = root.findall(".//t:UnitTestResult", ns)
    if not any("ModelIntegritySqlServerTests." in result.get("testName", "")
               and result.get("outcome") == "Passed" for result in results):
        raise ValueError("No passing SQL Server model-integrity tests were found")


if __name__ == "__main__":
    try:
        verify(Path(sys.argv[1]))
    except (IndexError, ValueError, OSError, ET.ParseError) as error:
        raise SystemExit(f"SQL integration verification failed: {error}")
    print("All backend tests passed, including SQL Server integration; no cases were skipped.")
