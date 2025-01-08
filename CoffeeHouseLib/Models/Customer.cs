using System;
using System.Collections.Generic;

namespace CoffeeHouseLib.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Phone { get; set; } = null!;

    public bool IdRole { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
