using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStateSample.Client.Features.Counter.IncrementCount
{
  public class IncrementCountAction : IRequest<CounterState>
  {
    public int Amount { get; set; }
  }
}
