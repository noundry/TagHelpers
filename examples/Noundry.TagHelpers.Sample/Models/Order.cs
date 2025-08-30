using System;
using System.ComponentModel.DataAnnotations;

namespace Noundry.TagHelpers.Sample.Models;

public class Order
{
    public int Id { get; set; }

    [Display(Name = "Placed By", Description = "The customer that placed the order.")]
    public Customer Customer { get; set; }

    public int CustomerId { get; set; }

    [Display(Name = "Placed on", Description = "The date and time the order was placed.")]
    public DateTime PlacedOn { get; set; }

    [DataType(DataType.Currency)]
    public decimal Total { get; set; }
}
