using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LocalModels
{
    public class ArmUserVhdArray
    {
        /// <summary>
        /// Gets or sets the list of the template image wrapper objects
        /// </summary>
        public IList<ArmUserVhdWrapper> Value { get; set; }
    }

    public class ArmUserVhdWrapper
    {
        /// <summary>
        /// Gets or sets the id of the entity
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the template image type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the template image location
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the details of the template image
        /// </summary>
        public UserVhdWrapper Properties { get; set; }
    }

    public class UserVhdWrapper
    {
        public string label { get; set; }

        public UserVhd OperatingSystemDisk { get; set; }
    }

    public class UserVhd
    {
        public string OsState { get; set; }

        public string DiskName { get; set; }

        public string OperatingSystem { get; set; }

        public string VhdUri { get; set; }
    }
}
