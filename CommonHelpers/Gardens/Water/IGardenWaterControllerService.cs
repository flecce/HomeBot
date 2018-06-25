using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Gardens.Water
{
    public interface IGardenWaterControllerService
    {
        bool Open();
        bool Close();
    }
}
