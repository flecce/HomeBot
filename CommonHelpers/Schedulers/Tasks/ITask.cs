﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Schedulers.Tasks
{
    public interface ITask
    {
        void Run();
        void Init();
    }
}
