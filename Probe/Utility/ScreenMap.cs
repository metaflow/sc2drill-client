using System.Collections.Generic;
using System.Drawing;

namespace Probe.Utility
{
    /// <summary>
    /// Represents screen mapping for specified screen resolution.
    /// </summary>
    internal class ScreenMap
    {
        private static readonly Dictionary<Size, ScreenMap> _map = new Dictionary<Size, ScreenMap>();

        static ScreenMap()
        {
            _map.Add(new Size(1680, 1050), new ScreenMap(new Rectangle(1260, 4, 375, 28)));
            _map.Add(new Size(1920, 1200), new ScreenMap(new Rectangle(1470, 5, 420, 34)));
        }

        ScreenMap(Rectangle mineralArea)
        {
            MineralArea = mineralArea;
        }

        /// <summary>
        /// Gets the screen map for the specified screen size.
        /// </summary>
        /// <param name="screenSize">Size of the screen.</param>
        /// <returns cref='ScreenMap'>Screen map.</returns>
        public static ScreenMap Get(Size screenSize)
        {
            if (!_map.ContainsKey(screenSize))
            {
                var s = new Size(375, 28);
                var offsetTopRight = new Point(45, 4);

                var map =
                    new ScreenMap(
                        new Rectangle(new Point(screenSize.Width - s.Width - offsetTopRight.X, offsetTopRight.Y), s)
                        );

                _map.Add(screenSize, map);
            }

            return _map[screenSize];
        }

        /// <summary>
        /// Gets the mineral area.
        /// </summary>
        public Rectangle MineralArea { get; private set; }
    }
}
