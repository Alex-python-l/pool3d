import importlib.util
import tempfile
import unittest
from pathlib import Path


class PreparationGuards(unittest.TestCase):
    def setUp(self):
        spec = importlib.util.spec_from_file_location('prepare_integration', Path(__file__).with_name('prepare_integration.py'))
        if not spec.origin or not Path(spec.origin).is_file():
            self.fail('Preparation guard implementation is missing')
        self.module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.module)

    def test_existing_destination_is_preserved(self):
        with tempfile.TemporaryDirectory(dir=Path(__file__).resolve().parent) as raw:
            root = Path(raw)
            target = root / 'deliverables' / 'fresh'
            target.mkdir(parents=True)
            marker = target / 'user.txt'
            marker.write_text('preserve')
            with self.assertRaises(FileExistsError):
                self.module.validate_destination(root, target)
            self.assertEqual(marker.read_text(), 'preserve')

    def test_outside_destination_is_rejected_without_writes(self):
        with tempfile.TemporaryDirectory(dir=Path(__file__).resolve().parent) as raw:
            root = Path(raw) / 'workspace'
            root.mkdir()
            target = Path(raw) / 'outside'
            with self.assertRaises(ValueError):
                self.module.validate_destination(root, target)
            self.assertFalse(target.exists())

    def test_fresh_child_is_accepted_without_creation(self):
        with tempfile.TemporaryDirectory(dir=Path(__file__).resolve().parent) as raw:
            root = Path(raw)
            target = root / 'deliverables' / 'fresh'
            self.assertEqual(self.module.validate_destination(root, target), target.resolve())
            self.assertFalse(target.exists())


if __name__ == '__main__':
    unittest.main()
