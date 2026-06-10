using System;
using System.Collections.Generic;
using System.Text;

namespace ThriftMedia.Contracts.Requests
{
    public record RenameStoreRequest(string NewStoreName);
}
