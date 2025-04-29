using Relativity.API;
using System;

namespace LTAS_User_Management.Utilities
{
    internal class LTASUMHelper
    {
        private readonly IHelper _helper;
        private readonly IAPILog _logger;

        public IHelper Helper => _helper;
        public IAPILog Logger => _logger;

        public LTASUMHelper(IHelper helper, IAPILog logger)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _logger = logger ?? throw new ArgumentNullException(nameof(_logger));
        }
    }
}
