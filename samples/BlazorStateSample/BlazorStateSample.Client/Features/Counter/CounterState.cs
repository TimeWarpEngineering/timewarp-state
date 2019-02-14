using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorState;

namespace BlazorStateSample.Client.Features.Counter
{
    public partial class CounterState : State<CounterState>
    {
        public CounterState() { } // needed for serialization

        protected CounterState(CounterState aState) : this()
        {
            Count = aState.Count;
        }

        public int Count { get; set; }

        public override object Clone() => new CounterState(this);

        protected override void Initialize() => Count = 3;
    }
}
