using StrategyIntegration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyIntegration
{
    public class IntegrationStrategy
    {
        private IIntegrationStrategy _strategy;

        public IntegrationStrategy(IIntegrationStrategy strategy)
        {
            _strategy = strategy;
        }

        public void SetStrategy(IIntegrationStrategy strategy)
        {
            _strategy = strategy;
        }

        public IIntegrationStrategy GetStrategy()
        {
            return _strategy;
        }

        public void ExecuteIntegration(Dictionary<string,object> source, string destination)
        {
            _strategy.IntegrateData(source, destination);
        }
    }
}
