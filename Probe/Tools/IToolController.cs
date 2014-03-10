using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Probe.Tools
{
    public interface IToolController
    {
        /// <summary>
        /// Gets the name for tray menu.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description for tray tool tip.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the icon for tray menu.
        /// </summary>
        Image Icon { get; }

        /// <summary>
        /// Gets a value indicating whether tool is installed.
        /// </summary>
        /// <value>
        /// Returns <c>true</c> if this instance is installed otherwise returns <c>false</c>.
        /// </value>
        bool IsInstalled { get; }

        /// <summary>
        /// Gets a value indicating whether tool has updates.
        /// </summary>
        /// <value>
        /// Returns	<c>true</c> if this instance has updates otherwise returns <c>false</c>.
        /// </value>
        bool HasUpdates { get; }

        /// <summary>
        /// Installs tool.
        /// </summary>
        void Install();

        /// <summary>
        /// Runs tool.
        /// </summary>
        void Run();
    }
}
