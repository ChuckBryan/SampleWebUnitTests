using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntegrationTest
{
    using Xunit;

    public class Class1
    {
        
        public Class1()
        {
            
        }
        [Fact]
        public void DoSomething()
        {
            Assert.True(1 ==1);
        }
    }
}
