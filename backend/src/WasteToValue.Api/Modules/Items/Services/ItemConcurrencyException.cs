using System;

namespace WasteToValue.Api.Modules.Items.Services;

public class ItemConcurrencyException : Exception
{
    public ItemConcurrencyException(string message) : base(message)
    {
    }

    public ItemConcurrencyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
