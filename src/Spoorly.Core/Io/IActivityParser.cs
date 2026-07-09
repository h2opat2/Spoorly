
using Spoorly.Core.Model;

namespace Spoorly.Core.Io;

public interface IActivityParser
{
    Activity Parse(Stream stream);

}