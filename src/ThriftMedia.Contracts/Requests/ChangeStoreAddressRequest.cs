using System;
using System.Collections.Generic;
using System.Text;

namespace ThriftMedia.Contracts.Requests
{
    public record ChangeStoreAddressRequest(
        string Address1,
        string? Address2,
        string City,
        string PostalCode,
        string? Country,
        string? ProvinceState
    );
}
