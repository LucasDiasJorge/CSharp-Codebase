using System;

namespace CustomFilterApi.Services;

public interface ISelectedServiceAccessor
{
    IBusinessService? Selected { get; set; }
}

public class SelectedServiceAccessor : ISelectedServiceAccessor
{
    public IBusinessService? Selected { get; set; }
}
