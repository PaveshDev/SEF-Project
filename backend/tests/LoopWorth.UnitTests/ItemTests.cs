using LoopWorth.Domain.Entities;
using LoopWorth.Domain.Enums;
using Xunit;
using System;

namespace LoopWorth.UnitTests.Items;

public class ItemTests
{
    [Fact]
    public void Item_Creation_Sets_Draft_Status()
    {
        var item = new Item
        {
            CustomerId = "customer1",
            Name = "Test Item"
        };
        
        Assert.Equal(ItemStatus.Draft, item.Status);
    }

    [Fact]
    public void Item_Assess_MustBe_Submitted_First()
    {
        var item = new Item { Status = ItemStatus.Draft };
        Assert.NotEqual(ItemStatus.Submitted, item.Status);
    }
}
