import importlib.util
import unittest
from pathlib import Path
import numpy as np


class BallSetTests(unittest.TestCase):
    def setUp(self):
        path = Path(__file__).resolve().parents[3] / 'deliverables/Pool3D-Integrated-V1/Source/generate_ball_set.py'
        if not path.exists(): self.fail('Full ball-set generator is missing')
        spec = importlib.util.spec_from_file_location('ballset', path)
        self.module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(self.module)

    def test_complete_set_pairs_solid_and_stripe_colours(self):
        balls = self.module.make_arrays(256)
        self.assertEqual(sorted(balls), list(range(16)))
        for number in range(1, 8):
            np.testing.assert_array_equal(balls[number][128, 0], balls[number + 8][128, 0])
            self.assertGreater(int(balls[number + 8][0, 0, 0]), 230)
        self.assertLess(int(balls[8][128, 0, 0]), 30)
        self.assertGreater(int(balls[0][128, 0, 0]), 230)

    def test_two_digit_labels_fit_number_disc(self):
        for number in (6, 9, 10, 11, 12, 13, 14, 15):
            mask = self.module.label_mask(number)
            rows, columns = np.where(mask > .01)
            self.assertGreater(columns.min(), 20)
            self.assertLess(columns.max(), 1004)
            self.assertGreater(rows.min(), 20)
            self.assertLess(rows.max(), 1004)
            self.assertGreater(len(rows), 20000)

    def test_uv_seam_and_poles_are_continuous(self):
        for image in self.module.make_arrays(256).values():
            np.testing.assert_array_equal(image[:, 0], image[:, -1])
            self.assertTrue(np.all(image[0] == image[0, 0]))
            self.assertTrue(np.all(image[-1] == image[-1, 0]))


if __name__ == '__main__': unittest.main()
