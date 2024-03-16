using System;
using System.IO.Hashing;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NuGettier.Upm;

public interface IGuidFactory
{
    NuGettier.Upm.MetaGen.Guid GenerateGuid(string value);
}
