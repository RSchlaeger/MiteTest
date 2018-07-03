using System;
using System.Collections.Generic;
using System.Text;

namespace MiteTest
{
    class Project
    {
        public int Budget { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int ConsumedBudget { get; set; }

    }

    class ProjectWrapper
    {
        public Project Project { get; set; }
    }
}
