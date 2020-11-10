using System;
using System.Collections.Generic;

namespace TestApp.Client.Pages
{
  using Models;

  public partial class CustomInputPage
  {
    public Record RecordForm { get; set; } = new Record();
    public List<Person> People { get; set; } = new List<Person>();

    protected override void OnInitialized()
    {
      People.Add(new Person { Id = Guid.NewGuid(), Name = "John" });
      People.Add(new Person { Id = Guid.NewGuid(), Name = "Steve" });
      People.Add(new Person { Id = Guid.NewGuid(), Name = "Dave" });
      People.Add(new Person { Id = Guid.NewGuid(), Name = "Alan" });
    }

    private void HandleValidSubmit()
    {
      Console.WriteLine("Valid Submit");
    }
  }
}
